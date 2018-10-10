using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UserManager m_UserManager;

    public LoginScreen m_InitialScreen;
    public UIMenu m_InitialMenu;

    private UIScreen[] m_Screens;
    private UIScreen m_CurrentScreen;

    private UIMenu[] m_Menus;
    private UIMenu m_CurrentMenu;

    private string m_CurrentUser;

	void Start ()
    {
        m_Screens = transform.GetComponentsInChildren<UIScreen>(true);
        foreach (var s in m_Screens)
        {
            s.gameObject.SetActive(true); // To make sure awake/start are called in time
            s.Hide();
        }

        m_Menus = transform.GetComponentsInChildren<UIMenu>(true);
        foreach (var m in m_Menus)
        {
            m.Hide();
        }

        if (m_InitialScreen)
        {
            ShowScreen(m_InitialScreen);
        }

        if (m_CurrentMenu)
        {
            ShowMenu(m_InitialMenu);
        }
	}
	
    public void ShowMenu(UIMenu menu)
    {
        if (m_CurrentMenu)
        {
            m_CurrentMenu.Hide();
        }
        m_CurrentMenu = menu;
        if (m_CurrentMenu)
        {
            m_CurrentMenu.Show();
        }
    }

    public void ShowScreen(UIScreen screen)
    {
        if (m_CurrentScreen)
        {
            m_CurrentScreen.Hide();
        }
        m_CurrentScreen = screen;
        if (m_CurrentScreen)
        {
            m_CurrentScreen.Show();
        }
    }

    public void ShowNoConnection()
    {
        m_InitialScreen.ShowNoConnection();
    }

    public void Logout()
    {
        m_UserManager.EndHacking();

        ShowMenu(null);
        ShowScreen(m_InitialScreen);

        foreach (UIScreen screen in m_Screens)
        {
            screen.OnLogout();
        }

        m_UserManager.Logout();
    }
}
