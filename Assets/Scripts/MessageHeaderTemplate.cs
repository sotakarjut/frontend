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
        //m_SenderImage.mainTexture = new Texture2D(500, 500);
	}

    private void OpenMessage(InboxScreen screen, string id)
    {
        screen.ShowMessage(id);
    }

    private void ToggleThread()
    {

    }

    public void SetData(MessageInfo m, System.DateTime timestamp, string partnerName, InboxScreen screen, int threadLength)
    {
        //m_SenderImage.sprite = senderImage;
        m_TopicText.text = "Aihe: " + (m.title != null && m.title.Length > 0 ? m.title : "(ei otsikkoa)");
        m_SenderText.text = partnerName != null ? partnerName : "Tuntematon";

        if ( threadLength > 1 )
        {
            m_ThreadLengthText.text = threadLength + " viestiä";
        } else
        {
            m_ThreadLengthText.text = "";
        }

        m_TimeStampText.text = MessageManager.GetTimeSince(timestamp);
        m_OpenButton.onClick.RemoveAllListeners();
        m_OpenButton.onClick.AddListener(delegate { OpenMessage(screen, m._id); } );
    }
}
