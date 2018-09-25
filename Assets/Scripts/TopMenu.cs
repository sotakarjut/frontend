﻿using UnityEngine;
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

        if ( m_UserManager.CurrentHackedUser != null )
        {
            m_NameText.text += " (hacked)";
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
