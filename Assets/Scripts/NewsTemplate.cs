using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewsTemplate : MonoBehaviour
{
    public Text m_SenderText;
    public Text m_TopicText;
    public Text m_TimeStampText;
    public Text m_MessageText;

	void Start ()
    {
		
	}

    public void SetData(News n)
    {
        m_TopicText.text = n.title != null && n.title.Length > 0 ? n.title : "(Ei otsikkoa)";
        m_SenderText.text = "Kirjoittaja: " + ( n.author.profile.name != null ? n.author.profile.name : "Tuntematon");
        m_TimeStampText.text = MessageManager.GetTimeSince(MessageManager.ParseTimeStamp(n.createdAt) );
        m_MessageText.text = n.body != null ? n.body : "(Ei sisältöä)";
    }
}
