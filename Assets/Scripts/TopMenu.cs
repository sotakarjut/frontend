﻿using UnityEngine;
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
        m_ClassText.text = profile.@class;
        m_ProfileImage.sprite = m_UserManager.CurrentUserImage; // TODO
        m_BalanceText.text = "BALANCE: " + profile.balance;

        base.Show();
    }

    public override void Show()
    {
        m_UserManager.GetCurrentUserInfo(UserInfoReceived);
    }



}
