using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageHeaderTemplate : MonoBehaviour
{
    public Image m_SenderImage;
    public Text m_SenderText;
    public Text m_TopicText;
    public Text m_TimeStampText;
    public Button m_OpenButton;

	void Start ()
    {
		
	}

    private void OpenMessage(InboxScreen screen, int id)
    {
        screen.ShowMessage(id);
    }

    public void SetData(Message m, Sprite senderImage, InboxScreen screen, int threadLength)
    {
        m_SenderImage.sprite = senderImage;
        m_SenderText.text = m.sender;
        m_TopicText.text = m.topic;
        if ( threadLength > 1 )
        {
            m_SenderText.text += " (" + threadLength + " messages in thread)";
        }
        m_TimeStampText.text = m.timestamp.ToString();
        m_OpenButton.onClick.AddListener(delegate { OpenMessage(screen, m.id); } );

        m_SenderText.fontStyle = m.read ? FontStyle.Normal : FontStyle.Bold;
        m_TopicText.fontStyle = m.read ? FontStyle.Normal : FontStyle.Bold;
        m_TimeStampText.fontStyle = m.read ? FontStyle.Normal : FontStyle.Bold;
    }
}
