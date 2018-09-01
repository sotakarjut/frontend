using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

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

    private string m_HackTarget;

    void Start()
    {
    }

    public override void Show()
    {
        base.Show();

        m_HackProgress.gameObject.SetActive(false);
        m_HackProgressBar.fillAmount = 0f;

        m_HackTargetDropdown.gameObject.SetActive(false);
        m_HackButton.gameObject.SetActive(false);
        m_UserManager.GetUsers(UsersReceived, NoConnection);
    }

    private void NoConnection()
    {
    }

    private void UsersReceived(List<string> users)
    {
        m_HackTargetDropdown.ClearOptions();
        m_HackTargetDropdown.AddOptions(users);
        m_HackTargetDropdown.interactable = true;
        m_HackTargetDropdown.gameObject.SetActive(true);

        m_HackButton.interactable = true;
        m_HackButton.gameObject.SetActive(true);
    }

    public void OnHack()
    {
        int targetIndex = m_HackTargetDropdown.value;
        m_HackTarget = m_UserManager.GetUserByIndex(targetIndex);

        m_UserManager.Hack(m_HackTarget, Hacked, HackFailed, NoConnection);
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
        while (progress < 1f)
        {
            m_HackProgressBar.fillAmount = progress;
            progress += Time.deltaTime / duration;
            yield return null;
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
}
