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
        m_TopicText.text = n.title;
        m_SenderText.text = "Kirjoittaja: " + n.author.profile.name;
        //m_TimeStampText.text = n.GetTimeStamp().ToString("d.M.yyyy H:mm");
        m_TimeStampText.text = MessageManager.GetTimeSince(MessageManager.ParseTimeStamp(n.createdAt) );
        m_MessageText.text = n.body;
    }
}
