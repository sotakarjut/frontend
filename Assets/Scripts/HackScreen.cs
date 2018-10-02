using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HackScreen : UIScreen
{
    public MessageManager m_MessageManager;
    public UserManager m_UserManager;
    public UIManager m_Manager;
    public UIScreen m_InboxScreen;
    public TopMenu m_TopMenu;

    public Dropdown m_HackTargetDropdown;
    public Button m_HackButton;
    public GameObject m_HackProgress;
    public Image m_HackProgressBar;
    public Toggle m_ShowPlayersToggle;

    public GameObject m_LastMessagePanel;
    public Text m_LastMessageText;

    private string m_HackTarget;
    private List<string> m_CachedTargets;
    private List<string> m_CachedTargetNames;

    void Awake()
    {
        m_CachedTargets = new List<string>();
        m_CachedTargetNames = new List<string>();
    }

    public override void Show()
    {
        base.Show();

        m_HackProgress.gameObject.SetActive(false);
        m_HackProgressBar.fillAmount = 0f;

        m_HackTargetDropdown.gameObject.SetActive(false);
        m_HackButton.gameObject.SetActive(false);

        m_ShowPlayersToggle.gameObject.SetActive( m_UserManager.CanCurrentUserImpersonate() );

        bool isGM = m_UserManager.CanCurrentUserImpersonate();
        m_LastMessagePanel.SetActive(isGM);
        if ( isGM )
        {
            m_MessageManager.GetLatest(LatestReceived, NoConnection, true);
        }

        RepopulateTargets();
    }

    private void LatestReceived(List<LatestInfo> latest)
    {
        if (latest != null)
        {
            string result = "";
            for (int i = 0; i < latest.Count; ++i)
            {
                string realname = m_UserManager.GetUserRealName(latest[i].recipient._id);

                result += (realname != null ? realname : "Tuntematon käyttäjä") + " " +
                    MessageManager.GetTimeSince(MessageManager.ParseTimeStamp(latest[i].createdAt)) + "\n";
            }
            m_LastMessageText.text = result;
        }
        else
        {
            m_LastMessageText.text = "";
        }
    }

    private void NoConnection()
    {
        m_Manager.ShowNoConnection();
        m_Manager.Logout();
    }

    public void OnHack()
    {
        if (m_HackTargetDropdown.options.Count > 0)
        {
            int targetIndex = m_HackTargetDropdown.value;
            m_HackTarget = m_CachedTargets[targetIndex];

            //Debug.Log("starting hack for " + m_HackTarget);
            m_UserManager.Hack(m_HackTarget, Hacked, HackFailed, NoConnection);
        }
    }

    private void Hacked(int duration)
    {
        StartCoroutine(HackProgress(duration));
    }

    IEnumerator HackProgress(int duration)
    {
        m_HackProgressBar.fillAmount = 0f;
        m_HackProgress.gameObject.SetActive(true);

        float progress = 0f;
        if (!m_UserManager.CanCurrentUserImpersonate())
        {
            while (progress < 1f)
            {
                m_HackProgressBar.fillAmount = progress;
                progress += Time.deltaTime / duration;
                yield return null;
            }
        }

        HackDone();
    }

    private void HackDone()
    {
        m_HackProgress.gameObject.SetActive(false);
        m_UserManager.SetHackedUser(m_HackTarget);
        m_Manager.ShowScreen(m_InboxScreen);
        m_TopMenu.Show();
    }

    private void HackFailed()
    {

    }

    private void RepopulateUsersReceived(List<string> users)
    {
        bool npcsIncluded = m_UserManager.CanCurrentUserImpersonate();
        bool pcsIncluded = !m_UserManager.CanCurrentUserImpersonate() || m_ShowPlayersToggle.isOn;

        for (int i = 0; i < users.Count; ++i)
        {
            // NPC = cannot be hacked and cannot impersonate
            // Player = can be hacked and cannot impersonate
            string userid = m_UserManager.GetUserIdByName(users[i]);
            if (!m_UserManager.CanImpersonate(userid))
            {
                bool canBeHacked = m_UserManager.CanBeHacked(userid);
                if ( npcsIncluded && !canBeHacked ||
                    pcsIncluded && canBeHacked )
                {
                    m_CachedTargets.Add(userid);
                    m_CachedTargetNames.Add(users[i]);
                }
            }
        }

        m_HackTargetDropdown.AddOptions(m_CachedTargetNames);
        m_HackTargetDropdown.interactable = true;
        m_HackTargetDropdown.gameObject.SetActive(true);

        m_HackButton.interactable = m_CachedTargets.Count > 0;
        m_HackButton.gameObject.SetActive(true);
    }

    private void RepopulateTargets()
    {
        if (m_CachedTargets != null)
        {
            m_HackTargetDropdown.ClearOptions();
            m_CachedTargets.Clear();
            m_CachedTargetNames.Clear();
            m_UserManager.GetUsers(RepopulateUsersReceived, null);
        }
    }

    public void OnShowPlayersToggled(bool enabled)
    {
        RepopulateTargets();
    }
}
