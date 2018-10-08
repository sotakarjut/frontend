using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class NewMessageScreen : UIScreen
{
    public MessageManager m_MessageManager;
    public UserManager m_UserManager;
    public UIManager m_Manager;

    public InboxScreen m_InboxScreen;

    public InputField m_TopicField;
    public Dropdown m_ReceiverDropdown;
    public TMP_InputField m_Message;
    public Button m_SendButton;
    

    private string m_ReplyID;
    private int m_FirstListIndex;

    public override void Show()
    {
        base.Show();

        m_TopicField.text = "";
        m_ReceiverDropdown.value = 0;
        m_Message.text = "";
        m_ReplyID = null;

        m_ReceiverDropdown.interactable = false;
        m_SendButton.interactable = false;

        m_UserManager.GetUsers(UsersReceived, null, NoConnection);
    }

    private void NoConnection()
    {
        m_Manager.ShowNoConnection();
        m_Manager.Logout();
    }

    private void UsersReceived(List<string> users)
    {
        if (users != null)
        {
            List<string> filtered = new List<string>();
            for (int i = 0; i < users.Count; ++i)
            {
                string userid = m_UserManager.GetUserIdByName(users[i]);
                if (!m_UserManager.CanImpersonate(userid))
                {
                    filtered.Add(users[i]);
                }
            }
            filtered.Sort();

            m_FirstListIndex = filtered.Count;
            List<string> lists = m_UserManager.GetMailingListNames();
            lists.Sort();
            if (lists != null)
            {
                for (int i = 0; i < lists.Count; ++i)
                {
                    filtered.Add(lists[i]);
                }
            }

            //filtered.Sort();

            m_ReceiverDropdown.ClearOptions();
            m_ReceiverDropdown.AddOptions(filtered);
            m_ReceiverDropdown.interactable = true;
            m_SendButton.interactable = true;
        } else
        {
            NoConnection();
        }
    }

    public void SetReplyData(string id)
    { 
        if (id != null)
        {
            MessageInfo m = m_MessageManager.GetMessage(id);

            if (m.title != null)
            {
                if (!m.title.StartsWith("Re:"))
                {
                    m_TopicField.text = "Re: " + m.title;
                }
                else
                {
                    m_TopicField.text = m.title;
                }
            } else
            {
                m_TopicField.text = "";
            }

            m_Message.text = Regex.Replace(m.body, "^", ">", RegexOptions.Multiline);

            m_ReplyID = id;

            for (int i = 0; i < m_ReceiverDropdown.options.Count; ++i)
            {
                if ( m_ReceiverDropdown.options[i].text.Equals(m.sender.username))
                {
                    m_ReceiverDropdown.value = i;
                    break;
                }
            }
        }
    }

    private void MessageSent()
    {
        Debug.Log("Message sent successfully");
        m_Manager.ShowScreen(m_InboxScreen);
        m_InboxScreen.ShowSent();
    }

    private void MessageSendFailure()
    {
        Debug.LogError("Error: Message sending failed");
    }

    public void OnSend()
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

        MessageInfo m = new MessageInfo();
        m.replyTo = m_ReplyID;
        m.title = topic;
        if (m_ReceiverDropdown.value < m_FirstListIndex)
        {
            m.recipient = m_UserManager.GetUserIdByName(m_ReceiverDropdown.options[m_ReceiverDropdown.value].text);
        } else
        {
            m.recipient = m_UserManager.GetListIdByName(m_ReceiverDropdown.options[m_ReceiverDropdown.value].text);
        }

        m.body = message;
        //Debug.Log("Sending message to user " + m.recipient + ". Topic = " + topic + "\nMessage = \n" + message);
        m_MessageManager.SendMessage(m, MessageSent, MessageSendFailure, NoConnection);

        m_TopicField.text = "";
        m_ReceiverDropdown.value = 0;
        m_Message.text = "";
    }
}
