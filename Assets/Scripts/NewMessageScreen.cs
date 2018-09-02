﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class NewMessageScreen : UIScreen
{
    public MessageManager m_MessageManager;
    public UserManager m_UserManager;
    public UIManager m_Manager;

    public InboxScreen m_InboxScreen;

    public InputField m_TopicField;
    public Dropdown m_ReceiverDropdown;
    public InputField m_Message;
    public Button m_SendButton;

    private string m_ReplyID;

	void Start ()
    {
	}

    public override void Show()
    {
        base.Show();

        //List<string> users = m_UserManager.GetUsers();
        //m_ReceiverDropdown.ClearOptions();
        //m_ReceiverDropdown.AddOptions(users);

        m_TopicField.text = "";
        m_ReceiverDropdown.value = 0;
        m_Message.text = "";
        m_ReplyID = null;

        m_ReceiverDropdown.interactable = false;
        m_SendButton.interactable = false;

        m_UserManager.GetUsers(UsersReceived, NoConnection);
    }

    private void NoConnection()
    {
        //m_UserManager.NoConnection();

        // TODO: this is for testing without backend
        m_ReceiverDropdown.ClearOptions();
        List<string> users = new List<string>();
        users.Add("NoConnectionUser");
        users.Add("GetABackendUser");
        m_ReceiverDropdown.AddOptions(users);
        m_ReceiverDropdown.interactable = true;
        m_SendButton.interactable = true;
    }

    private void UsersReceived(List<string> users)
    {
        // TODO: filter out impersonators
        List<string> filtered = new List<string>();
        for (int i = 0; i < users.Count; ++i)
        {
            string userid = m_UserManager.GetUserIdByName(users[i]);
            if ( !m_UserManager.CanImpersonate(userid) )
            {
                filtered.Add(users[i]);
            }
        }

        m_ReceiverDropdown.ClearOptions();
        m_ReceiverDropdown.AddOptions(filtered);
        m_ReceiverDropdown.interactable = true;
        m_SendButton.interactable = true;
    }

    public void SetReplyData(string id)
    { 
        if (id != null)
        {
            MessageInfo m = m_MessageManager.GetMessage(id);

            if (!m.title.StartsWith("Re:"))
            {
                m_TopicField.text = "Re: " + m.title;
            } else
            {
                m_TopicField.text = m.title;
            }

            m_Message.text = Regex.Replace(m.body, "^", ">", RegexOptions.Multiline);

            m_ReplyID = id;

            for (int i = 0; i < m_ReceiverDropdown.options.Count; ++i)
            {
                if ( m_ReceiverDropdown.options[i].Equals(m.sender.username))
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
        Debug.Log("Message sending failed");
    }

    public void OnSend()
    {
        string topic = m_TopicField.text;
        string message = m_Message.text;

        MessageInfo m = new MessageInfo();
        m.replyTo = m_ReplyID;
        //m.sender = m_UserManager.CurrentUserName;
        //m.timestamp = System.DateTime.Now;
        m.title = topic;
        m.recipient = m_ReceiverDropdown.options[m_ReceiverDropdown.value].text;
        m.body = message;
        Debug.Log("Sending message to user " + m.recipient + ". Topic = " + topic + "\nMessage = \n" + message);
        m_MessageManager.SendMessage(m, MessageSent, MessageSendFailure, NoConnection);

        m_TopicField.text = "";
        m_ReceiverDropdown.value = 0;
        m_Message.text = "";
    }
}
