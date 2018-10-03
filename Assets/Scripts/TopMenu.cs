using UnityEngine;
using UnityEngine.UI;

public class TopMenu : UIMenu
{
    public UserManager m_UserManager;
    public UIManager m_UIManager;

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

    private void UserInfoReceived(UserManager.UserProfile profile)
    {
        m_NameText.text = profile.name != null ? profile.name : "Tuntematon";
        m_TitleText.text = profile.title != null ? profile.title : "";
        m_GroupText.text = profile.group != null ? profile.group : "";
        m_BalanceText.text = "Varat: " + profile.balance.ToString();

        m_NewMessageButton.interactable = true;
        m_NewMessageButton2.interactable = true;
        m_ReplyButton.interactable = true;
        m_NewMessageButtonImage.color = new Color(1f, 1f, 1f);
        m_NewMessageText.color = new Color(1f, 1f, 1f);
        m_ReplyButtonText.color = new Color(1f, 1f, 1f);

        if ( m_UserManager.CurrentHackedUser != null )
        {
            m_NameText.text += " (hakkeroitu)";

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

        // TODO: Disabled for now for players
        //m_HackButton.SetActive(m_UserManager.CanCurrentUserHack() || canImpersonate );
        m_HackButton.SetActive(canImpersonate);

        m_UserManager.GetUserImage(m_UserManager.CurrentUser, m_ProfileImage);

        base.Show();
    }

    private void NoConnection()
    {
        m_UIManager.ShowNoConnection();
        m_UIManager.Logout();
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
