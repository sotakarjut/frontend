using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/*
[System.Serializable]
public struct Message
{
    public string _id;
    public string replyTo;
    //public string sender;
    public string recipient;
    public string title;
    //public System.DateTime timestamp;
    public string body;
    public bool read;
}*/

[System.Serializable]
public struct Sender
{
    //public UserManager.UserProfile profile;
    public string _id;
    public string username;
}

[System.Serializable]
public struct MessageInfo
{
    public string _id;
    public string body;
    public string recipient; // _id
    public Sender sender;
    public string title;
    public string createdAt;
    public string replyTo;
}

public class MessageManager : MonoBehaviour
{
    //public delegate void MessagesReceivedCallback(Dictionary<string, MessageInfo> messages);
    public delegate void MessagesReceivedCallback();
    public delegate void NoConnectionCallback();
    public delegate void MessageReceivingError();

    public UserManager m_UserManager;

    //private List<Message> m_CachedMessages;
    private Dictionary<string, MessageInfo> m_CachedMessages;
    private int m_NextId;

    void Start ()
    {
    }

    /*
    public void MarkAsRead(int id)
    {

        int index = m_MessageCache[id];
        Message m = m_Messages[index];
        m.read = true;
        m_Messages[index] = m;
    }*/

    public IEnumerator SendMessageCoroutine(MessageInfo m)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", m.title);
        form.AddField("messageBody", m.body);
        form.AddField("recipient", m.recipient);
        if (m.replyTo != null)
        {
            form.AddField("replyTo", m.replyTo);
        }

        UnityWebRequest request = UnityWebRequest.Post("http://localhost:3000/api/messages", form);
       

        //Hashtable header = new Hashtable();
        //header.Add("Content-Type", "application/json");

        /*
        string jsontest = "{ \"title\" : \"jsontit\", \"messageBody\" : \"jsonbody\", \"recipient\" : \"testi\" }";
        byte[] formData = System.Text.Encoding.UTF8.GetBytes(jsontest);
        UnityWebRequest request = UnityWebRequest.Post("http://localhost:3000/api/messages", new Dictionary<string, string>());
        request.uploadHandler = new UploadHandlerRaw(formData);
        request.SetRequestHeader("Content-Type", "application/json");
        */

        m_UserManager.SetCurrentUserAuthorization(request);


        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot send message: " + request.error + ", Code = " + request.responseCode);
        }
        else if (request.isHttpError)
        {
            if (request.responseCode == 400)
            {
                Debug.Log("Http error: Message data missing: " + request.error + ", Code = " + request.responseCode);
            }
            else if (request.responseCode == 404)
            {
                Debug.Log("Http error: Recipient or parent not found: " + request.error + ", Code = " + request.responseCode + " " + request.downloadHandler.text);
            }
            else if (request.responseCode == 500)
            {
                Debug.Log("Http error: Database search failed: " + request.error + ", Code = " + request.responseCode);
            }
        }
        else if (request.responseCode == 200)
        {
            // response contains the message 
            Debug.Log("Message sent: " + request.downloadHandler.text);
        }

    }

    public void SendMessage(MessageInfo m)
    {
        StartCoroutine(SendMessageCoroutine(m));
    }

    public MessageInfo GetMessage(string id)
    {
        return m_CachedMessages[id];
    }

    public IEnumerator GetMessagesCoroutine(MessagesReceivedCallback success, NoConnectionCallback noconnection, MessageReceivingError failure)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/api/messages");
        m_UserManager.SetCurrentUserAuthorization(request);
        yield return request.SendWebRequest();
        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot get messages: " + request.error + ", Code = " + request.responseCode);
            noconnection();
        }
        else if (request.isHttpError)
        {
            if (request.responseCode == 500)
            {
                Debug.Log("Http error: Database search failed: " + request.error + ", Code = " + request.responseCode);
                failure();
            }
        }
        else if (request.responseCode == 200)
        {
            // response contains the message 
            Debug.Log("Messages received: " + request.downloadHandler.text);

            Dictionary<string, MessageInfo> messageData = JsonConvert.DeserializeObject<Dictionary<string, MessageInfo>>(request.downloadHandler.text);


            m_CachedMessages = new Dictionary<string, MessageInfo>();
            foreach (MessageInfo m in messageData.Values)
            {
                m_CachedMessages[m._id] = m;
            }
        }
    }

    public void GetMessages(bool forceReload, MessagesReceivedCallback success, NoConnectionCallback noconnection, MessageReceivingError failure )
    {
        if (m_CachedMessages == null || forceReload)
        {
            StartCoroutine(GetMessagesCoroutine(success, noconnection, failure));
        } else
        {
            success();
        }
    }

    public class ThreadStructure
    {
        public List<string> m_Roots;
        public Dictionary<string, string> m_Next;

        public ThreadStructure()
        {
            m_Roots = new List<string>();
            m_Next = new Dictionary<string, string>();
        }
    }

    public ThreadStructure GetThreads()
    {
        if ( m_CachedMessages == null )
        {
            return null;
        }

        ThreadStructure result = new ThreadStructure();

        foreach (string id in m_CachedMessages.Keys )
        {
            MessageInfo m = GetMessage(id);
            if ( m.replyTo == null )
            {
                result.m_Roots.Add(id);
            } else
            {
                result.m_Next[m.replyTo] = id;
            }
        }

        return result;
    }

}
