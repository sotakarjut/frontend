using UnityEngine;
using UnityEngine.UI;

public class TopMenu : UIMenu
{
    public UserManager m_UserManager;
    public Text m_NameText;
    public Text m_ClassText;
    public Image m_ProfileImage;
    public Text m_BalanceText;

    void Start ()
    {
	}

    private void UserInfoReceived(UserManager.UserProfile profile)
    {
        m_NameText.text = profile.name;
        m_ClassText.text = profile.role + ", " + profile.group;
        m_ProfileImage.sprite = m_UserManager.CurrentUserImage; // TODO
        m_BalanceText.text = "BALANCE: " + profile.balance;

        base.Show();
    }

    private void NoConnection()
    {
        //m_UserManager.NoConnection();

        // TODO: this is for testing without backend
        m_NameText.text = "No connection name";
        m_ClassText.text = "Backendless worker";
        m_ProfileImage.sprite = m_UserManager.CurrentUserImage; // TODO
        m_BalanceText.text = "BALANCE: " + 1234;

        base.Show();
    }

    public override void Show()
    {
        m_UserManager.GetCurrentUserInfo(UserInfoReceived, NoConnection);
    }
}
