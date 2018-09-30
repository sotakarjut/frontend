using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageTemplate : MonoBehaviour
{
    public Text m_SenderText;
    public Text m_ReceiverText;
    public Text m_TopicText;
    public Text m_TimeStampText;
    public Text m_MessageText;

    public void SetData(MessageInfo m, string title, string sender, string recipient, string body)
    {
        m_TopicText.text = title != null && title.Length > 0 ? title : "(Ei otsikkoa)";
        m_SenderText.text = "Lähettäjä: " + (sender != null ? sender : "Tuntematon");
        m_ReceiverText.text = "Vastaanottaja: " + (recipient != null ? recipient : "Tuntematon");
        m_TimeStampText.text = MessageManager.GetTimeSince( MessageManager.ParseTimeStamp(m.createdAt) );
        m_MessageText.text = body != null ? body : "(Ei viestiä)";
    }
}
