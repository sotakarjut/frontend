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

    public Image m_MessageSeparator;

    public Text m_ReceivedButtonText;
    public Text m_SentButtonText;

    public Color m_SelectedBoxTextColor;
    public Color m_UnselectedBoxTextColor;

    private List<MessageHeaderTemplate> m_MessageHeaderInstances;
    private List<Image> m_MessageHeaderSeparators;
    private int m_LastActiveMessageHeader;

    private List<MessageTemplate> m_MessageInstances;
    private List<Image> m_MessageSeparators;
    private int m_LastActiveMessage;
    private InboxMode m_LastShownBox;

    public enum InboxMode
    {
        Received,
        Sent
    }

    void Awake()
    {
        m_MessageHeaderInstances = new List<MessageHeaderTemplate>();
        m_MessageHeaderSeparators = new List<Image>();
        m_MessageInstances = new List<MessageTemplate>();
        m_MessageSeparators = new List<Image>();

        if ( m_MessageHeaderTemplate )
        {
            m_MessageHeaderTemplate.gameObject.SetActive(false);
        }
        if ( m_MessageTemplate)
        {
            m_MessageTemplate.gameObject.SetActive(false);
        }
        if ( m_MessageSeparator )
        {
            m_MessageSeparator.gameObject.SetActive(false);
        }

        m_LastActiveMessage = -1;
        m_LastActiveMessageHeader = -1;

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

    public void ShowMessage(string id)
    {
        m_MessageListPanel.gameObject.SetActive(false);

        m_MessageManager.GetMessages(false, () => { ShowMessageReceived(id); }, NoConnection, MessagesFailed);
    }

    private void NoConnection()
    {

    }

    private void MessagesFailed()
    {

    }

    private void ShowMessageReceived(string id)
    { 
        //List<MessageInfo> showM = new List<MessageInfo>();
        MessageManager.ThreadStructure threads = m_MessageManager.GetThreads();

        // find thread root
        MessageInfo m = m_MessageManager.GetMessage(id);
        int loops = 0;
        while ( m.replyTo != null && loops < 10000)
        {
            m = m_MessageManager.GetMessage(m.replyTo);
            ++loops;
        }
        if ( loops == 10000 )
        {
            Debug.LogError("Inifinite loop in a thread starting from message " + id);
        }

        List<MessageInfo> threadMessages = new List<MessageInfo>();

        loops = 0;
        do
        {
            threadMessages.Add(m);
            if (threads.m_Next.ContainsKey(m._id))
            {
                m = m_MessageManager.GetMessage( threads.m_Next[m._id] );
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

        //m_MessageManager.MarkAsRead(threadMessages[0].id);

        int i = 0;
        for (; i < threadMessages.Count; ++i)
        {
            if (m_MessageInstances.Count <= i )
            {
                if (i > 0)
                {
                    m_MessageSeparators.Add(Instantiate(m_MessageSeparator));
                    m_MessageSeparators[i - 1].transform.SetParent(m_MessageContentParent, false);
                }

                m_MessageInstances.Add( Instantiate<MessageTemplate>(m_MessageTemplate) );
                m_MessageInstances[i].transform.SetParent(m_MessageContentParent, false);
            }

            if (i > 0)
            {
                m_MessageSeparators[i - 1].gameObject.SetActive(true);
            }

            m_MessageInstances[i].SetData(threadMessages[i].title, m_UserManager.GetUserRealName(threadMessages[i].sender._id),
                m_UserManager.GetUserRealName(threadMessages[i].recipient), threadMessages[i].body);
            m_MessageInstances[i].gameObject.SetActive(true);
        }

        // disable the rest of the instances
        for (; i <= m_LastActiveMessage; ++i)
        {
            if (i > 0)
            {
                m_MessageSeparators[i - 1].gameObject.SetActive(false);
            }

            m_MessageInstances[i].gameObject.SetActive(false);
        }

        m_LastActiveMessage = threadMessages.Count - 1;

        m_ReplyButton.onClick.RemoveAllListeners();
        m_ReplyButton.onClick.AddListener(delegate { Reply(threadMessages[threadMessages.Count-1]._id); });

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

    public void Reply(string id)
    {
        m_UIManager.ShowScreen(m_NewMessageScreen);
        m_NewMessageScreen.SetReplyData(id);
    }

    private int CountThreadLength(MessageManager.ThreadStructure threads, string root)
    {
        int result = 1;
        int loops = 0;
        while ( threads.m_Next.ContainsKey(root) && loops < 10000)
        {
            root = threads.m_Next[root];
            ++result;
            ++loops;
        }
        return result;
    }

    private bool CheckThreadVisibility(string root, MessageManager.ThreadStructure threads, InboxMode mode)
    {
        bool otherSenderFound = false;
        
        do
        {
            MessageInfo m = m_MessageManager.GetMessage(root);
            if ( mode == InboxMode.Sent && m.sender.username == m_UserManager.CurrentUserName )
            {
                return true;
            }
            if ( mode == InboxMode.Received && m.sender.username != m_UserManager.CurrentUserName )
            {
                otherSenderFound = true;
            }

            if (threads.m_Next.ContainsKey(root))
            {
                root = threads.m_Next[root];
            } else
            {
                break;
            }
        } while (true);

        return otherSenderFound;
    }

    private System.DateTime GetThreadTimestamp(string root, MessageManager.ThreadStructure threads)
    {
        //System.DateTime result;
        return System.DateTime.Today;

        /*
        result = m_MessageManager.GetMessage(root).timestamp;
        if (threads.m_Next.ContainsKey(root))
        {
            root = threads.m_Next[root];
            do
            {
                Message m = m_MessageManager.GetMessage(root);
                if (m.timestamp > result)
                {
                    result = m.timestamp;
                }
                if (threads.m_Next.ContainsKey(root))
                {
                    root = threads.m_Next[root];
                }
                else
                {
                    break;
                }
            }
            while (true);
        }
        return result;*/
    }

    private string GetLastThreadPartner(MessageManager.ThreadStructure threads, string message)
    {
        while ( threads.m_Next.ContainsKey(message) )
        {
            message = threads.m_Next[message];
        }

        MessageInfo m = m_MessageManager.GetMessage(message);
        if ( m.sender.username != m_UserManager.CurrentUserName )
        {
            return m.sender._id;
        } else
        {
            return m.recipient;
        }
    }

    private void UpdateInboxContents(InboxMode mode)
    {
        m_MessageManager.GetMessages(false, () => { InboxReceived(mode); }, NoConnection, MessagesFailed);
    }

    private void InboxReceived(InboxMode mode)
    { 
        MessageManager.ThreadStructure threads = m_MessageManager.GetThreads();

        // Find and sort the messages that started a thread and should be shown in this mailbox
        List<string> visibleThreadRoots = new List<string>();
        for (int i = 0; i < threads.m_Roots.Count; ++i)
        {
            if (CheckThreadVisibility(threads.m_Roots[i], threads, mode))
            {
                visibleThreadRoots.Add( threads.m_Roots[i]);
            }
        }

        //visibleThreadRoots.Sort((t1, t2) => { return GetThreadTimestamp(messages, threads, t2).CompareTo(GetThreadTimestamp(messages, threads, t1)); });

        int index = 0;
        for (int i = 0; i < visibleThreadRoots.Count; ++i)
        { 
            // create a new instance if it doesn't exist already
            if (m_MessageHeaderInstances.Count <= index)
            {
                if (index > 0)
                {
                    m_MessageHeaderSeparators.Add(Instantiate(m_MessageSeparator));
                    m_MessageHeaderSeparators[index - 1].transform.SetParent(m_MessageListContentParent, false);
                }

                m_MessageHeaderInstances.Add(Instantiate<MessageHeaderTemplate>(m_MessageHeaderTemplate));
                m_MessageHeaderInstances[index].transform.SetParent(m_MessageListContentParent, false);
            }

            if (index > 0)
            {
                m_MessageHeaderSeparators[index - 1].gameObject.SetActive(true);
            }

            // update instance data
            MessageHeaderTemplate instance = m_MessageHeaderInstances[index];
            string message = visibleThreadRoots[i];
            string partner = GetLastThreadPartner(threads, message);
            MessageInfo m = m_MessageManager.GetMessage(message);
            instance.SetData(m, m_UserManager.GetUserRealName(partner), m_UserManager.GetUserImage(partner), this, CountThreadLength(threads, message));
            instance.gameObject.SetActive(true);

            ++index;
        }

        // disable the rest of the instances
        for (; index <= m_LastActiveMessageHeader; ++index )
        {
            if ( index > 0 && index < m_LastActiveMessageHeader )
            {
                m_MessageHeaderSeparators[index - 1].gameObject.SetActive(false);
            }

            m_MessageHeaderInstances[index].gameObject.SetActive(false);
        }

        m_LastActiveMessageHeader = visibleThreadRoots.Count - 1;
    }
}
