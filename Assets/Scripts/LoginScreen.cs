using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : UIScreen
{
    public UIManager m_Manager;
    public UIMenu m_TopMenu;
    public UIScreen m_InitialScreen;

    public Text m_InvalidLogin;
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
            m_Manager.Login(m_ID.text, m_PIN.text, m_InitialScreen);
        }
    }
}
