using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Message
{
    public int id;
    public int parent;
    public string sender;
    public string receiver;
    public string topic;
    public System.DateTime timestamp;
    public string message;
    public bool read;
}

public class MessageManager : MonoBehaviour
{
    private List<Message> m_Messages;
    private Dictionary<int, int> m_MessageCache;
    private int m_NextId;

    void Start ()
    {
        m_Messages = new List<Message>();

        Message m = new Message();
        m.id = 1;
        m.parent = -1;
        m.sender = "Sika";
        m.receiver = "Receiver";
        m.topic = "Topic 1";
        m.timestamp = new System.DateTime(2016, 1, 20, 18, 15, 15);
        m.message = "Message 1 contents are here";
        m_Messages.Add(m);
        m.id = 2;
        m.parent = -1;
        m.sender = "Possu";
        m.receiver = "Receiver";
        m.topic = "Topic 2";
        m.timestamp = new System.DateTime(2017, 1, 20, 18, 15, 15);
        m.message = "Message 2 contents are here";
        m_Messages.Add(m);
        m.id = 3;
        m.parent = -1;
        m.sender = "Sika";
        m.receiver = "Receiver";
        m.topic = "Topic 3";
        m.timestamp = new System.DateTime(2018, 1, 20, 18, 15, 15);
        m.message = "Message 3 contents are here";
        m_Messages.Add(m);

        m_NextId = 4;

        m_MessageCache = new Dictionary<int, int>();
        for (int i = 0; i < m_Messages.Count; ++i)
        {
            m_MessageCache.Add(m_Messages[i].id, i);
        }
    }

    public void MarkAsRead(int id)
    {
        int index = m_MessageCache[id];
        Message m = m_Messages[index];
        m.read = true;
        m_Messages[index] = m;
    }

    public void SendMessage(Message m)
    {
        m.id = m_NextId++;
        m_Messages.Add(m);
        m_MessageCache.Add(m.id, m_Messages.Count - 1);
    }

    public int GetMessageIndexByID(int id)
    {
        return m_MessageCache[id];
    }

    public Message GetMessage(int id)
    {
        return m_Messages[m_MessageCache[id]];
    }

    public List<Message> GetMessages(string user)
    {
        return m_Messages;
    }

    public List<int> GetThreads()
    {
        List<int> results = new List<int>();
        Dictionary<int, int> m_IdToIndex = new Dictionary<int, int>();
        for (int i = 0; i < m_Messages.Count; ++i)
        {
            m_IdToIndex[m_Messages[i].id] = i;
            results.Add(-1);
        }

        for (int i = 0; i < m_Messages.Count; ++i)
        {
            if ( m_Messages[i].parent >= 0 )
            {
                results[ m_IdToIndex[m_Messages[i].parent] ] = i;
            }
        }

        return results;
    }

}
