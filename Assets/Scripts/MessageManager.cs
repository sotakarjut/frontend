using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

[System.Serializable]
public struct Sender
{
    public string _id;
    public string username;
}

[System.Serializable]
public struct MessageInfo
{
    public string _id;
    public string body;
    public string recipient; // id
    public Sender sender;
    public string title;
    public string createdAt;
    public string replyTo;
}

[System.Serializable]
public struct LatestRecipient
{
    public string _id;
}

[System.Serializable]
public struct LatestMessageInfo
{
    public LatestRecipient recipient;
    public string createdAt;
}

[System.Serializable]
public struct LatestMailingListInfo
{
    public LatestRecipient recipientList;
    public string createdAt;
}

[System.Serializable]
public struct LatestInfo
{
    public string _id;
    public string createdAt;
}

public class MessageManager : MonoBehaviour
{
    public delegate void MessagesReceivedCallback();
    public delegate void NoConnectionCallback();
    public delegate void MessageReceivingError();
    public delegate void MessageSentCallback();
    public delegate void LatestReceivedCallback(List<LatestInfo> latest);
    public delegate void LatestFailedCallback();
    public delegate void MessageSendingError();

    public UserManager m_UserManager;

    private Dictionary<string, MessageInfo> m_CachedMessages;
    private string m_CachedMessageUser;
    private int m_NextId;

    private float m_LastRefresh;

    private List<LatestInfo> m_LatestResults;
    private int m_LatestResultCount;

    private void Awake()
    {
        m_LatestResults = new List<LatestInfo>();
    }

    private IEnumerator GetLatestCoroutine(LatestReceivedCallback success, LatestFailedCallback failure, string path, int totalCount, bool mailinglist)
    {
        string url = "/api/messages/latest/" + path;

        UnityWebRequest request = UnityWebRequest.Get(Constants.serverAddress + url);
        request.chunkedTransfer = false;

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot get latest: " + request.error + ", Code = " + request.responseCode);
            if (failure != null) failure();
        }
        else if (request.isHttpError)
        {
            Debug.Log("Http error: Cannot get latest: " + request.error + ", Code = " + request.responseCode);
            if (failure != null) failure();
        }
        else
        {
            List<LatestInfo> latest = null;
            try
            {
                latest = new List<LatestInfo>();
                // Reprocess the info to a common format to make it unified for both cases
                if (mailinglist)
                {
                    List<LatestMailingListInfo> tmp = JsonConvert.DeserializeObject<List<LatestMailingListInfo>>(request.downloadHandler.text);
                    for (int i = 0; i < tmp.Count; ++i)
                    {
                        LatestInfo info = new LatestInfo();
                        info._id = tmp[i].recipientList._id;
                        info.createdAt = tmp[i].createdAt;
                        latest.Add(info);
                    }
                }
                else
                {
                    List<LatestMessageInfo> tmp = JsonConvert.DeserializeObject<List<LatestMessageInfo>>(request.downloadHandler.text);
                    for (int i = 0; i < tmp.Count; ++i)
                    {
                        LatestInfo info = new LatestInfo();
                        info._id = tmp[i].recipient._id;
                        info.createdAt = tmp[i].createdAt;
                        latest.Add(info);
                    }
                }
            } catch (Exception)
            {
                Debug.LogError("Warning: cannot get latest messages.");
            }
            if (latest != null)
            {
                m_LatestResults.AddRange(latest);

                m_LatestResultCount++;
                if (m_LatestResultCount == totalCount)
                {
                    m_LatestResults.Sort((m1, m2) => { return MessageManager.ParseTimeStamp(m2.createdAt).CompareTo(MessageManager.ParseTimeStamp(m1.createdAt)); });
                    m_LatestResults = m_LatestResults.GetRange(0, Mathf.Min(m_LatestResults.Count, 10 ) );

                    if (success != null) success(m_LatestResults);
                }
            }
        }
    }

    // if we are looking for npcs, mailing lists are not included
    public void GetLatest(LatestReceivedCallback success, LatestFailedCallback failure, bool npcs = false)
    {
        List<string> rolenames = npcs ? m_UserManager.GetNPCRoles() : m_UserManager.GetCharacterRoles();
        m_LatestResultCount = 0;
        m_LatestResults.Clear();

        for (int i = 0; i < rolenames.Count; ++i)
        {
            StartCoroutine(GetLatestCoroutine(success, failure, "role/" + rolenames[i], npcs ? rolenames.Count : rolenames.Count + 1, false));
        }

        if ( !npcs )
        {
            StartCoroutine(GetLatestCoroutine(success, failure, "mailinglist", rolenames.Count + 1, true));
        }
    }

    public static DateTime ParseTimeStamp(string createdAt)
    {
        System.DateTime result;
        if (createdAt != null)
        {
            if (System.DateTime.TryParseExact(createdAt, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result))
            {
                return result;
            }
        }

        Debug.LogWarning("Warning: Can't parse date");
        return DateTime.Now;
    }

    public static string GetTimeSince(DateTime time)
    {
        TimeSpan diff = DateTime.UtcNow - time;
        if ( diff.Days > 0 )
        {
            return diff.Days + " päivää sitten";
        } else if ( diff.Hours > 0 )
        {
            return diff.Hours + " h " + diff.Minutes + " min sitten";
        } else
        {
            return diff.Minutes + " min sitten";
        }
    }

    public bool MessageExists(string id)
    {
        return m_CachedMessages != null && m_CachedMessages.ContainsKey(id);
    }

    public IEnumerator SendMessageCoroutine(MessageInfo m, MessageSentCallback success, MessageSendingError failure, NoConnectionCallback noconnection )
    {
        WWWForm form = new WWWForm();
        form.AddField("recipientId", m.recipient);
        form.AddField("title", m.title);
        form.AddField("messageBody", m.body);
        if (m.replyTo != null)
        {
            form.AddField("replyTo", m.replyTo);
        }

        bool hacked = m_UserManager.CurrentHackedUser != null;

        UnityWebRequest request;
        if (hacked)
        {
            form.AddField("targetId", m_UserManager.CurrentHackedUser);
            request = UnityWebRequest.Post(Constants.serverAddress + "api/hack/messages", form);
        }
        else
        {
            request = UnityWebRequest.Post(Constants.serverAddress + "api/messages", form);
        }

        m_UserManager.SetCurrentUserAuthorization(request);

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot send message: " + request.error + ", Code = " + request.responseCode);
            if (noconnection != null) noconnection();
        }
        else if (request.isHttpError)
        {
            if (request.responseCode == 400)
            {
                Debug.Log("Http error: Message data missing: " + request.error + ", Code = " + request.responseCode + " " + request.downloadHandler.text);
            }
            else if (request.responseCode == 404)
            {
                Debug.Log("Http error: Recipient or parent not found: " + request.error + ", Code = " + request.responseCode + " " + request.downloadHandler.text);
            }
            else if (request.responseCode == 500)
            {
                Debug.Log("Http error: Database search failed: " + request.error + ", Code = " + request.responseCode);
            }

            if (failure != null) failure();
        }
        else if (request.responseCode == 200)
        {
            // response contains the message 
            //Debug.Log("Message sent: " + request.downloadHandler.text);
            GetMessages(true, () => { success(); }, noconnection, () => { failure(); } );
        }
    }

    public string GetUserToShow()
    {
        if (m_UserManager.CurrentHackedUser != null)
        {
            return m_UserManager.CurrentHackedUser;
        } else
        {
            return m_UserManager.CurrentUser;
        }
    }

    public void SendMessage(MessageInfo m, MessageSentCallback success, MessageSendingError failure, NoConnectionCallback noconnection)
    {
        StartCoroutine(SendMessageCoroutine(m, success, failure, noconnection));
    }

    public MessageInfo GetMessage(string id)
    {
        if (m_CachedMessages != null && m_CachedMessages.ContainsKey(id))
        {
            return m_CachedMessages[id];
        } else
        {
            return default(MessageInfo);
        }
    }

    public IEnumerator GetMessagesCoroutine(MessagesReceivedCallback success, NoConnectionCallback noconnection, MessageReceivingError failure)
    {
        bool hack = m_UserManager.CurrentHackedUser != null;

        UnityWebRequest request;
        if (hack)
        {
            WWWForm form = new WWWForm();
            form.AddField("targetId", m_UserManager.CurrentHackedUser);

            request = UnityWebRequest.Get(Constants.serverAddress + "api/hack/messages/" + m_UserManager.CurrentHackedUser);
        } else
        {
            request = UnityWebRequest.Get(Constants.serverAddress + "api/messages");
        }
        m_UserManager.SetCurrentUserAuthorization(request);

        yield return request.SendWebRequest();
        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot get messages: " + request.error + ", Code = " + request.responseCode);
            if ( noconnection != null) noconnection();
        }
        else if (request.isHttpError)
        {
            if (request.responseCode == 500)
            {
                Debug.Log("Http error: Database search failed: " + request.error + ", Code = " + request.responseCode);
                if (failure != null) failure();
            }
        }
        else if (request.responseCode == 200)
        {
            // response contains the message 
            //Debug.Log("Messages received: " + request.downloadHandler.text);
            Dictionary<string, MessageInfo> messageData = null;

            try
            {
                messageData = JsonConvert.DeserializeObject<Dictionary<string, MessageInfo>>(request.downloadHandler.text);
            } catch (Exception)
            {
                Debug.LogError("Error: cannot deserialize messages.");
            }

            if (messageData != null)
            {
                m_CachedMessages = new Dictionary<string, MessageInfo>();
                foreach (MessageInfo m in messageData.Values)
                {
                    if (m._id != null)
                    {
                        m_CachedMessages[m._id] = m;
                    }
                }
                m_CachedMessageUser = GetUserToShow();

                if (success != null) success();
            }
        }
    }

    private bool IsTimeToReloadMessages()
    {
        return Time.time > m_LastRefresh + 30f;
    }

    public void GetMessages(bool forceReload, MessagesReceivedCallback success, NoConnectionCallback noconnection, MessageReceivingError failure )
    {
        if (m_CachedMessages == null || forceReload || IsTimeToReloadMessages() || !GetUserToShow().Equals(m_CachedMessageUser) )
        {
            m_LastRefresh = Time.time;
            StartCoroutine(GetMessagesCoroutine(success, noconnection, failure));
        } else
        {
            if ( success != null ) success();
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
            if ( m.replyTo == null || !m_CachedMessages.ContainsKey(m.replyTo) )
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
