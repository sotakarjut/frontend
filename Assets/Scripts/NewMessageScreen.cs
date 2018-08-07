using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class NewMessageScreen : UIScreen
{
    public MessageManager m_MessageManager;
    public UserManager m_UserManager;

    public InputField m_TopicField;
    public Dropdown m_ReceiverDropdown;
    public InputField m_Message;
    public Button m_SendButton;

    private int m_ReplyID;

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
        m_ReplyID = -1;

        m_ReceiverDropdown.interactable = false;
        m_SendButton.interactable = false;

        m_UserManager.GetUsers(UsersReceived);
    }

    public void UsersReceived(List<string> users)
    {
        m_ReceiverDropdown.ClearOptions();
        m_ReceiverDropdown.AddOptions(users);
        m_ReceiverDropdown.interactable = true;
        m_SendButton.interactable = true;
    }

    public void SetReplyData(int id)
    { 
        if (id >= 0)
        {
            Message m = m_MessageManager.GetMessage(id);

            if (!m.topic.StartsWith("Re:"))
            {
                m_TopicField.text = "Re: " + m.topic;
            } else
            {
                m_TopicField.text = m.topic;
            }

            m_ReceiverDropdown.value = m_UserManager.GetUserIndex(m.sender);

            m_Message.text = Regex.Replace(m.message, "^", ">", RegexOptions.Multiline);

            m_ReplyID = id;
        }
    }

    public void OnSend()
    {
        string topic = m_TopicField.text;
        int receiver = m_ReceiverDropdown.value;
        string message = m_Message.text;

        Message m = new Message();
        m.parent = m_ReplyID;
        m.sender = m_UserManager.CurrentUserName;
        m.timestamp = System.DateTime.Now;
        m.topic = topic;
        m.receiver = m_UserManager.GetUserByIndex(receiver);
        m.message = message;
        m_MessageManager.SendMessage(m);

        Debug.Log("Sent message to user " + receiver + ". Topic = " + topic + "\nMessage = \n" + message);

        m_TopicField.text = "";
        m_ReceiverDropdown.value = 0;
        m_Message.text = "";
    }
}
