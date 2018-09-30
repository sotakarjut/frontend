using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoginScreen : UIScreen
{
    public UIManager m_Manager;
    public UserManager m_UserManager;
    public MessageManager m_MessageManager;

    public UIMenu m_TopMenu;
    public UIScreen m_InitialScreen;
    public UIScreen m_InitialImpersonatorScreen;

    public Button m_LoginButton;
    public Text m_LoginButtonText;

    public Text m_InvalidLogin;
    public Text m_NoConnection;

    public Text m_LatestText;

    public InputField m_ID;
    public InputField m_PIN;

    private IEnumerator m_LatestRefresher;
    private bool m_UsersReceived;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(m_ID.gameObject);
        m_ID.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private void Update()
    {
        if ( Input.GetKeyDown(KeyCode.Tab))
        {
            EventSystem sys = EventSystem.current;

            if ( sys.currentSelectedGameObject == m_ID.gameObject )
            {
                sys.SetSelectedGameObject(m_PIN.gameObject);
                m_PIN.OnPointerClick(new PointerEventData(EventSystem.current));
            } else if (sys.currentSelectedGameObject == m_PIN.gameObject)
            {
                sys.SetSelectedGameObject(m_ID.gameObject);
                m_ID.OnPointerClick(new PointerEventData(EventSystem.current));
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnLogin();
        }
    }

    public override void Show()
    {
        base.Show();
        m_ID.text = "";
        m_PIN.text = "";
        m_InvalidLogin.gameObject.SetActive(false);
        m_NoConnection.gameObject.SetActive(false);
        m_LoginButton.interactable = false;
        m_LoginButtonText.color = new Color(.5f, .5f, .5f);

        m_UserManager.GetUsers(UsersReceived, UsersFailed);

        m_LatestRefresher = UpdateLatestCoroutine();
        StartCoroutine(m_LatestRefresher);
    }

    public override void Hide()
    {
        if (m_LatestRefresher != null)
        {
            StopCoroutine(m_LatestRefresher);
        }
        base.Hide();
    }

    private IEnumerator UpdateLatestCoroutine()
    {
        while (!m_UsersReceived)
        {
            //Debug.Log("Waiting for users");
            yield return new WaitForSeconds(1f);
        }

        while (true)
        {
            Debug.Log("Getting latest...");
            m_MessageManager.GetLatest(LatestReceived, null);
            yield return new WaitForSeconds(60);
        }
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
            m_LatestText.text = result;
        }
        else
        {
            m_LatestText.text = "";
        }
    }

    private void UsersFailed()
    {
        m_InvalidLogin.gameObject.SetActive(false);
        m_NoConnection.gameObject.SetActive(true);
        m_LoginButton.interactable = false;
        m_LoginButtonText.color = new Color(.5f, .5f, .5f);
        StartCoroutine(RetryUsers());
    }

    private IEnumerator RetryUsers()
    {
        yield return new WaitForSeconds(20f);

        m_UserManager.GetUsers(UsersReceived, UsersFailed);
    }

    private void UsersReceived(List<string> users)
    {
        m_NoConnection.gameObject.SetActive(false);
        m_LoginButton.interactable = true;
        m_LoginButtonText.color = new Color(1f,1f,1f);

        m_UsersReceived = true;
    }

    public void LoginSuccessful()
    {
        m_InvalidLogin.gameObject.SetActive(false);
        m_NoConnection.gameObject.SetActive(false);

        m_Manager.ShowMenu(m_TopMenu);

        if (m_UserManager.CanCurrentUserImpersonate())
        {
            m_Manager.ShowScreen(m_InitialImpersonatorScreen);
        }
        else
        {
            m_Manager.ShowScreen(m_InitialScreen);
        }
    }

    public void LoginFailed()
    {
        m_NoConnection.gameObject.SetActive(false);
        m_InvalidLogin.gameObject.SetActive(true);

        m_ID.text = "";
        m_PIN.text = "";
    }

    public void NoConnection()
    {
        m_InvalidLogin.gameObject.SetActive(false);
        m_NoConnection.gameObject.SetActive(true);
        m_LoginButton.interactable = false;
        m_LoginButtonText.color = new Color(.5f, .5f, .5f);
        
        // TODO: should actually test for connection
        StartCoroutine(RetryUsers());
    }

    public void OnLogin()
    {
        if (m_ID.text.Length == 0 || m_PIN.text.Length == 0)
        {
            m_InvalidLogin.gameObject.SetActive(true);
            m_ID.text = "";
            m_PIN.text = "";
        }
        else if ( m_Manager )
        {
            //Debug.Log("Logging in " + m_ID.text + " with PIN " + m_PIN.text);
            m_UserManager.Login(m_ID.text, m_PIN.text, LoginSuccessful, LoginFailed, NoConnection);
        }
    }

    public void ShowNoConnection()
    {
        m_NoConnection.gameObject.SetActive(true);
    }
}
