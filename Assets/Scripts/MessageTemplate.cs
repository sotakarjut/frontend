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

    public void SetData(MessageInfo m, string title, string sender, string recipient, string body)
    {
        m_TopicText.text = title;
        m_SenderText.text = "Lähettäjä: " + sender;
        m_ReceiverText.text = "Vastaanottaja: " + recipient;
        m_TimeStampText.text = m.GetTimeStamp().ToString("d.M.yyyy H:mm");
        m_MessageText.text = body;
    }
}
