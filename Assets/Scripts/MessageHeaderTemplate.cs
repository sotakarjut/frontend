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

    public Text m_ThreadLengthText;
    //public ThreadMessageTemplate m_Template;
    //public Button m_OpenThreadButton;

    //private bool m_ThreadShown;
    

	void Start ()
    {
		
	}

    private void OpenMessage(InboxScreen screen, string id)
    {
        screen.ShowMessage(id);
    }

    private void ToggleThread()
    {

    }

    public void SetData(MessageInfo m, string partnerName, Sprite senderImage, InboxScreen screen, int threadLength)
    {
        m_SenderImage.sprite = senderImage;
        m_TopicText.text = "Aihe: " + m.title;
        m_SenderText.text = partnerName;

        if ( threadLength > 1 )
        {
            m_ThreadLengthText.text = threadLength + " viestiä";
        } else
        {
            m_ThreadLengthText.text = "";
        }

        m_TimeStampText.text = m.GetTimeStamp().ToString("d.M.yyyy H:mm");
        m_OpenButton.onClick.RemoveAllListeners();
        m_OpenButton.onClick.AddListener(delegate { OpenMessage(screen, m._id); } );

        /*
        m_SenderText.fontStyle = m.read ? FontStyle.Normal : FontStyle.Bold;
        m_TopicText.fontStyle = m.read ? FontStyle.Normal : FontStyle.Bold;
        m_TimeStampText.fontStyle = m.read ? FontStyle.Normal : FontStyle.Bold;
        */
    }
}
