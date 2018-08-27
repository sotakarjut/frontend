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
        m_ReceiverDropdown.ClearOptions();
        m_ReceiverDropdown.AddOptions(users);
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

            //m_ReceiverDropdown.value = m_UserManager.GetUserIndex(m.sender);

            m_Message.text = Regex.Replace(m.body, "^", ">", RegexOptions.Multiline);

            m_ReplyID = id;
        }
    }

    public void OnSend()
    {
        string topic = m_TopicField.text;
        int receiver = m_ReceiverDropdown.value;
        string message = m_Message.text;

        MessageInfo m = new MessageInfo();
        m.replyTo = m_ReplyID;
        //m.sender = m_UserManager.CurrentUserName;
        //m.timestamp = System.DateTime.Now;
        m.title = topic;
        m.recipient = m_UserManager.GetUsernameByIndex(receiver);
        m.body = message;
        m_MessageManager.SendMessage(m);

        Debug.Log("Sent message to user " + receiver + ". Topic = " + topic + "\nMessage = \n" + message);

        m_TopicField.text = "";
        m_ReceiverDropdown.value = 0;
        m_Message.text = "";
    }
}
