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

    public override void Show()
    {
        base.Show();
        m_NameText.text = m_UserManager.CurrentUserName;
        m_ClassText.text = m_UserManager.CurrentUserClass;
        m_ProfileImage.sprite = m_UserManager.CurrentUserImage;
        m_BalanceText.text = "BALANCE: " + m_UserManager.CurrentUserBalance;
    }



}
