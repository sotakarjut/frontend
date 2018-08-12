using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : UIScreen
{
    public UIManager m_Manager;
    public UserManager m_UserManager;

    public UIMenu m_TopMenu;
    public UIScreen m_InitialScreen;

    public Text m_InvalidLogin;
    public Text m_NoConnection;

    public InputField m_ID;
    public InputField m_PIN;

    private void Start()
    {
    }

    public override void Show()
    {
        base.Show();
        m_ID.text = "";
        m_PIN.text = "";
        m_InvalidLogin.gameObject.SetActive(false);
        m_NoConnection.gameObject.SetActive(false);
    }

    public void LoginSuccessful()
    {
        m_Manager.ShowMenu(m_TopMenu);
        m_Manager.ShowScreen(m_InitialScreen);
    }

    public void LoginFailed()
    {
        m_InvalidLogin.gameObject.SetActive(true);
        m_ID.text = "";
        m_PIN.text = "";
    }

    public void NoConnection()
    {
        m_NoConnection.gameObject.SetActive(true);
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
