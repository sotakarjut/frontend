using UnityEngine;
using UnityEngine.UI;

public class TopMenu : UIMenu
{
    public UserManager m_UserManager;
    public Text m_NameText;
    public Text m_TitleText;
    public Text m_GroupText;
    public Image m_ProfileImage;
    public Text m_BalanceText;
    public GameObject m_HackButton;
    public Button m_NewMessageButton;
    public Button m_NewMessageButton2;
    public Image m_NewMessageButtonImage;
    public Text m_NewMessageText;

    public Button m_ReplyButton;
    public Text m_ReplyButtonText;

    void Start ()
    {
	}

    private void UserInfoReceived(UserManager.UserProfile profile)
    {
        m_NameText.text = profile.name;
        m_TitleText.text = profile.title;
        m_GroupText.text = profile.group;
        //m_ProfileImage.sprite = m_UserManager.CurrentUserImage; // TODO
        m_BalanceText.text = "BALANCE: " + profile.balance;

        m_NewMessageButton.interactable = true;
        m_NewMessageButton2.interactable = true;
        m_ReplyButton.interactable = true;
        m_NewMessageButtonImage.color = new Color(1f, 1f, 1f);
        m_NewMessageText.color = new Color(1f, 1f, 1f);
        m_ReplyButtonText.color = new Color(1f, 1f, 1f);

        if ( m_UserManager.CurrentHackedUser != null )
        {
            m_NameText.text += " (hacked)";

            if ( m_UserManager.GetCurrentUserHackerLevel() < 3)
            {
                m_NewMessageButton.interactable = false;
                m_NewMessageButton2.interactable = false;
                m_ReplyButton.interactable = false;
                m_NewMessageButtonImage.color = new Color(.3f, .3f, .3f);
                m_NewMessageText.color = new Color(.3f, .3f, .3f);
                m_ReplyButtonText.color = new Color(.3f, .3f, .3f);
            }
        }

        bool canImpersonate = m_UserManager.CanCurrentUserImpersonate();
        m_HackButton.SetActive(m_UserManager.CanCurrentUserHack() || canImpersonate );

        m_UserManager.GetUserImage(m_UserManager.CurrentUser, m_ProfileImage);

        base.Show();
    }

    private void NoConnection()
    {
        //m_UserManager.NoConnection();

        /*
        // TODO: this is for testing without backend
        m_NameText.text = "No connection name";
        m_TitleText.text = "Worker";
        m_GroupText.text = "Without connection";
        m_ProfileImage.sprite = m_UserManager.CurrentUserImage; // TODO
        m_BalanceText.text = "BALANCE: " + 1234;
        */

        base.Show();
    }

    public override void Show()
    {
        m_UserManager.GetCurrentUserInfo(UserInfoReceived, NoConnection);
    }

    public override void Hide()
    {
        base.Hide();
    }
}
