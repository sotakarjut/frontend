using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageTemplate : MonoBehaviour
{
    //public Image m_SenderImage;
    public Text m_SenderText;
    public Text m_ReceiverText;
    public Text m_TopicText;
    public Text m_TimeStampText;
    public Text m_MessageText;

	void Start ()
    {
		
	}

    public void SetData(Message m)
    {
        m_SenderText.text = "From: " + m.sender;
        m_ReceiverText.text = "To: " + m.receiver;
        m_TopicText.text = "Subject: " + m.topic;
        m_TimeStampText.text = m.timestamp.ToString();
        m_MessageText.text = m.message;
    }
}
