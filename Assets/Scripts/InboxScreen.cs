using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InboxScreen : UIScreen
{
    public UserManager m_UserManager;
    public MessageManager m_MessageManager;
    public UIManager m_UIManager;
    public NewMessageScreen m_NewMessageScreen;

    public Transform m_MessageListContentParent;
    public MessageHeaderTemplate m_MessageHeaderTemplate;

    public Transform m_MessageContentParent;
    public MessageTemplate m_MessageTemplate;

    public GameObject m_MessageListPanel;
    public GameObject m_MessagePanel;
    public Button m_ReplyButton;

    public Text m_ReceivedButtonText;
    public Text m_SentButtonText;

    public Color m_SelectedBoxTextColor;
    public Color m_UnselectedBoxTextColor;

    private List<MessageHeaderTemplate> m_MessageHeaderInstances;
    private int m_LastActiveMessageHeader;

    private List<MessageTemplate> m_MessageInstances;
    private int m_LastActiveMessage;
    private InboxMode m_LastShownBox;

    public enum InboxMode
    {
        Received,
        Sent
    }

    void Start ()
    {
        m_MessageHeaderInstances = new List<MessageHeaderTemplate>();
        m_MessageInstances = new List<MessageTemplate>();

        if ( m_MessageHeaderTemplate )
        {
            m_MessageHeaderTemplate.gameObject.SetActive(false);
        }
        if ( m_MessageTemplate)
        {
            m_MessageTemplate.gameObject.SetActive(false);
        }
    }

    public override void Show()
    {
        base.Show();
        ShowInbox();
    }

    public void ShowInbox()
    {
        m_MessagePanel.gameObject.SetActive(false);
        m_MessageListPanel.gameObject.SetActive(true);
        m_LastShownBox = InboxMode.Received;
        m_ReceivedButtonText.color = m_SelectedBoxTextColor;
        m_SentButtonText.color = m_UnselectedBoxTextColor;

        UpdateInboxContents(InboxMode.Received);
    }

    public void ShowSent()
    {
        m_MessagePanel.gameObject.SetActive(false);
        m_MessageListPanel.gameObject.SetActive(true);
        m_LastShownBox = InboxMode.Sent;
        m_ReceivedButtonText.color = m_UnselectedBoxTextColor;
        m_SentButtonText.color = m_SelectedBoxTextColor;

        UpdateInboxContents(InboxMode.Sent);
    }

    public void ShowMessage(int id)
    {
        m_MessageListPanel.gameObject.SetActive(false);

        List<Message> allMessages = m_MessageManager.GetMessages(m_UserManager.CurrentUserName);
        List<Message> threadMessages = new List<Message>();
        List<int> threads = m_MessageManager.GetThreads();
        Message m = m_MessageManager.GetMessage(id);
        int loops = 0;
        while ( m.parent >= 0 && loops < 10000)
        {
            m_MessageManager.GetMessage(m.parent);
            ++loops;
        }
        if ( loops == 10000 )
        {
            Debug.LogError("Inifinite loop in a thread starting from message " + id);
        }

        loops = 0;
        do
        {
            threadMessages.Add(m);
            int currentIndex = m_MessageManager.GetMessageIndexByID(m.id);
            if (threads[currentIndex] >= 0)
            {
                m = allMessages[threads[currentIndex]];
            } else
            {
                break;
            }
            loops++;
        } while (loops < 10000);
        if (loops == 10000)
        {
            Debug.LogError("Inifinite loop in a thread starting from message " + id);
        }

        m_MessageManager.MarkAsRead(threadMessages[0].id);
        int i = 0;
        for (; i < threadMessages.Count; ++i)
        {

            if (m_MessageInstances.Count <= i )
            {
                m_MessageInstances.Add( Instantiate<MessageTemplate>(m_MessageTemplate) );
                m_MessageInstances[i].transform.SetParent(m_MessageContentParent, false);
            }

            m_MessageInstances[i].SetData(threadMessages[i]);
            m_MessageInstances[i].gameObject.SetActive(true);
        }

        // disable the rest of the instances
        for (; i <= m_LastActiveMessage; ++i)
        {
            m_MessageInstances[i].gameObject.SetActive(false);
        }

        m_LastActiveMessage = threadMessages.Count - 1;

        m_ReplyButton.onClick.RemoveAllListeners();
        m_ReplyButton.onClick.AddListener(delegate { Reply(threadMessages[threadMessages.Count-1].id); });

        m_MessagePanel.gameObject.SetActive(true);
    }

    public void OnBack()
    {
        if ( m_LastShownBox == InboxMode.Received )
        {
            ShowInbox();
        } else if ( m_LastShownBox == InboxMode.Sent )
        {
            ShowSent();
        }
    }

    public void Reply(int id)
    {
        m_UIManager.ShowScreen(m_NewMessageScreen);
        m_NewMessageScreen.SetReplyData(id);
    }

    private int CountThreadLength(List<int> threads, int start)
    {
        int result = 1;
        int loops = 0;
        while ( threads[start] >= 0 && loops < 10000)
        {
            start = threads[start];
            ++result;
            ++loops;
        }
        return result;
    }

    private bool CheckThreadVisibility(int root, List<Message> messages, List<int> threads, InboxMode mode)
    {
        bool otherSenderFound = false;
        do
        {
            if ( mode == InboxMode.Sent && messages[root].sender == m_UserManager.CurrentUserName )
            {
                return true;
            }
            if ( mode == InboxMode.Received && messages[root].sender != m_UserManager.CurrentUserName )
            {
                otherSenderFound = true;
            }

            root = threads[root];
        } while (root >= 0);

        return otherSenderFound;
    }

    private System.DateTime GetThreadTimestamp(List<Message> messages, List<int> threads, int root)
    {
        System.DateTime result;
        result = messages[root].timestamp;
        int current = threads[root];
        while (current >= 0)
        {
            if (messages[current].timestamp > result)
            {
                result = messages[current].timestamp;
            }
            current = threads[current];
        }
        return result;
    }

    private string GetLastThreadPartner(List<Message> messages, List<int> threads, int message)
    {
        while ( threads[message] >= 0 )
        {
            message = threads[message];
        }

        if ( messages[message].sender != m_UserManager.CurrentUserName )
        {
            return messages[message].sender;
        } else
        {
            return messages[message].receiver;
        }
    }

    private void UpdateInboxContents(InboxMode mode)
    {
        List<Message> messages = m_MessageManager.GetMessages(m_UserManager.CurrentUserName);

        List<int> threads = m_MessageManager.GetThreads();

        // Find and sort the messages that started a thread and should be shown in this mailbox
        List<int> visibleThreadRoots = new List<int>();
        for (int i = 0; i < messages.Count; ++i)
        {
            if (messages[i].parent < 0)
            {
                if (CheckThreadVisibility(i, messages, threads, mode))
                {
                    visibleThreadRoots.Add(i);
                }
            }
        }

        visibleThreadRoots.Sort((t1, t2) => { return GetThreadTimestamp(messages, threads, t2).CompareTo(GetThreadTimestamp(messages, threads, t1)); });

        int index = 0;
        for (int i = 0; i < visibleThreadRoots.Count; ++i)
        { 
            // create a new instance if it doesn't exist already
            if (m_MessageHeaderInstances.Count <= index)
            {
                m_MessageHeaderInstances.Add(Instantiate<MessageHeaderTemplate>(m_MessageHeaderTemplate));
                m_MessageHeaderInstances[index].transform.SetParent(m_MessageListContentParent, false);
            }

            // update instance data
            MessageHeaderTemplate instance = m_MessageHeaderInstances[index];
            int message = visibleThreadRoots[i];
            string partnerName = GetLastThreadPartner(messages, threads, message);
            instance.SetData(messages[message], partnerName, m_UserManager.GetUserImage(messages[message].sender), this, CountThreadLength(threads, message));
            instance.gameObject.SetActive(true);

            ++index;
        }

        // disable the rest of the instances
        for (; index <= m_LastActiveMessageHeader; ++index )
        {
            m_MessageHeaderInstances[index].gameObject.SetActive(false);
        }

        m_LastActiveMessageHeader = visibleThreadRoots.Count - 1;
    }
}
