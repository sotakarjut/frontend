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
        m_TopicText.text = "Aihe: " + m.topic;
        m_SenderText.text = "Lähettäjä: " + m.sender;
        m_ReceiverText.text = "Vastaanottaja: " + m.receiver;
        m_TimeStampText.text = m.timestamp.ToString();
        m_MessageText.text = m.message;
    }
}
