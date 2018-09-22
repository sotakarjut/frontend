using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoginScreen : UIScreen
{
    public UIManager m_Manager;
    public UserManager m_UserManager;

    public UIMenu m_TopMenu;
    public UIScreen m_InitialScreen;
    public UIScreen m_InitialImpersonatorScreen;

    public Button m_LoginButton;
    public Text m_LoginButtonText;

    public Text m_InvalidLogin;
    public Text m_NoConnection;

    public InputField m_ID;
    public InputField m_PIN;

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
    }

    private void UsersFailed()
    {
        m_NoConnection.gameObject.SetActive(true);
        m_LoginButton.interactable = false;
        m_LoginButtonText.color = new Color(.5f, .5f, .5f);
        StartCoroutine(RetryUsers());
    }

    private IEnumerator RetryUsers()
    {
        yield return new WaitForSeconds(10f);

        m_UserManager.GetUsers(UsersReceived, UsersFailed);
    }

    private void UsersReceived(List<string> users)
    {
        m_NoConnection.gameObject.SetActive(false);
        m_LoginButton.interactable = true;
        m_LoginButtonText.color = new Color(1f,1f,1f);
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
        // TODO: test for connection
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
            Debug.Log("Logging in " + m_ID.text + " with PIN " + m_PIN.text);

            m_UserManager.Login(m_ID.text, m_PIN.text, LoginSuccessful, LoginFailed, NoConnection);
        }
    }

    public void ShowNoConnection()
    {
        m_NoConnection.gameObject.SetActive(true);
    }
}
