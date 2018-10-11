using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class EditNewsScreen : UIScreen
{
    public NewsManager m_NewsManager;
    public UserManager m_UserManager;
    public UIManager m_Manager;

    public NewsFeedScreen m_NewsFeedScreen;

    public InputField m_TopicField;
    public TMP_InputField m_Message;
    public Button m_SendButton;
    public Button m_UpdateButton;

    private string m_UpdateID;
    private int m_FirstListIndex;

    public override void Show()
    {
        base.Show();

        m_TopicField.text = "";
        m_Message.text = "";
        m_UpdateID = null;

        m_SendButton.gameObject.SetActive(true);
        m_UpdateButton.gameObject.SetActive(false);
    }

    private void NoConnection()
    {
        m_Manager.ShowNoConnection();
        m_Manager.Logout();
    }

    public void SetEditData(string id)
    { 
        if (id != null)
        {
            News n = m_NewsManager.GetNewsItem(id);

            if ( n._id != null )
            {
                m_UpdateID = n._id;

                if (n.title != null)
                {
                    m_TopicField.text = n.title;
                }

                if (n.body != null )
                {
                    m_Message.text = n.body;
                }

                m_UpdateButton.gameObject.SetActive(true);
                m_SendButton.gameObject.SetActive(false);
            }
        }
    }

    public void EditNews(string id)
    {
        m_Manager.ShowScreen(this);
        SetEditData(id);
    }

    /*
    private void NewsSent(List<News> news)
    {
        Debug.Log("News sent successfully");
        m_Manager.ShowScreen(m_NewsFeedScreen);
    }*/

    private void MessageSendFailure()
    {
        Debug.LogError("Error: News sending failed");
    }

    public void OnSend(bool update)
    {
        string topic;

        if (m_TopicField.text == null || m_TopicField.text.Length == 0)
        {
            topic = "(Ei otsikkoa)";
        } else
        {
            topic = m_TopicField.text;
        }

        string message;
        if (m_Message.text == null || m_Message.text.Length == 0)
        {
            message = "(Ei sisältöä)";
        } else
        {
            message = m_Message.text;
        }

        m_NewsManager.SendNews(topic, message, m_UpdateID, () => { m_NewsFeedScreen.ForceRefresh(); m_Manager.ShowScreen(m_NewsFeedScreen); }, NoConnection);

        m_TopicField.text = "";
        m_Message.text = "";
    }
}
