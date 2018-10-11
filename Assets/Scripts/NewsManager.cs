using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Text;

[System.Serializable]
public struct Profile
{
    public string name;
}

[System.Serializable]
public struct Author
{
    public Profile profile;
    public string _id;
}

[System.Serializable]
public struct News
{
    public Author author;
    public string body;
    public string title;
    public string createdAt;
    public string _id;
}

public class NewsManager : MonoBehaviour
{
    public UserManager m_UserManager;

    public delegate void NewsReceivedCallback(List<News> news);
    public delegate void NewsSentCallback();
    public delegate void NoConnectionCallback();
    public delegate void NewsReceivingError();

    private List<News> m_CachedNews;
    private float m_LastRefresh;

    void Start ()
    {
    }

    public News GetNewsItem(string id)
    {
        if (m_CachedNews == null || id == null) return default(News);

        for (int i = 0; i < m_CachedNews.Count; ++i)
        {
            string tmp = m_CachedNews[i]._id;
            if ( tmp != null && tmp.Equals(id) )
            {
                return m_CachedNews[i];
            }
        }

        return default(News);
    }

    public IEnumerator SendNewsCoroutine(string topic, string message, NewsSentCallback success, NoConnectionCallback noconnection)
    {
        WWWForm form = new WWWForm();
        form.AddField("title", topic);
        form.AddField("newsBody", message);

        UnityWebRequest request = UnityWebRequest.Post(Constants.serverAddress + "api/news", form);
        m_UserManager.SetCurrentUserAuthorization(request);

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot send news: " + request.error + ", Code = " + request.responseCode);
            if (noconnection != null) noconnection();
        }
        else if (request.isHttpError)
        {
            if (request.responseCode == 400)
            {
                Debug.Log("Http error: News data missing: " + request.error + ", Code = " + request.responseCode + " " + request.downloadHandler.text);
            }
            else if (request.responseCode == 500)
            {
                Debug.Log("Http error: Database search failed: " + request.error + ", Code = " + request.responseCode);
            }
        }
        else if (request.responseCode == 200)
        {
            if (success != null) success();
        }
    }

    public struct UpdatedNews
    {
        public string title;
        public string newsBody;
    }

    public IEnumerator EditNewsCoroutine(string topic, string message, string editId, NewsSentCallback success, NoConnectionCallback noconnection)
    {
        UpdatedNews un = new UpdatedNews
        {
            title = topic,
            newsBody = message
        };
        string json = JsonConvert.SerializeObject(un);
        UnityWebRequest request = UnityWebRequest.Put(Constants.serverAddress + "api/news/" + editId, Encoding.UTF8.GetBytes(json));

        request.SetRequestHeader("Content-Type", "application/json");
        m_UserManager.SetCurrentUserAuthorization(request);

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot edit news: " + request.error + ", Code = " + request.responseCode);
            if (noconnection != null) noconnection();
        }
        else if (request.isHttpError)
        {
            if (request.responseCode == 400)
            {
                Debug.Log("Http error: News data missing: " + request.error + ", Code = " + request.responseCode + " " + request.downloadHandler.text);
            }
            else if (request.responseCode == 500)
            {
                Debug.Log("Http error: Database search failed: " + request.error + ", Code = " + request.responseCode);
            }
        }
        else if (request.responseCode == 200)
        {
            if (success != null) success();
        }
    }

    public void SendNews(string topic, string message, string updateId, NewsSentCallback success, NoConnectionCallback noconnection)
    {
        if ( updateId == null )
        {
            StartCoroutine(SendNewsCoroutine(topic, message, success, noconnection));
        } else
        {
            StartCoroutine(EditNewsCoroutine(topic, message, updateId, success, noconnection));
        }
    }

    public IEnumerator GetNewsCoroutine(NewsReceivedCallback success, NoConnectionCallback noconnection, NewsReceivingError failure)
    {
        UnityWebRequest request;
        request = UnityWebRequest.Get(Constants.serverAddress + "api/news");

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot get news: " + request.error + ", Code = " + request.responseCode);
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
            //Debug.Log("News received: " + request.downloadHandler.text);

            Dictionary<string, News> newsData = null;
            try
            {
                newsData = JsonConvert.DeserializeObject<Dictionary<string, News>>(request.downloadHandler.text);
            } catch (Exception)
            {
                Debug.LogWarning("warning: Cannot deserialize news.");
            }

            if (newsData != null)
            {
                m_CachedNews = new List<News>();
                foreach (News n in newsData.Values)
                {
                    m_CachedNews.Add(n);
                }
                m_CachedNews.Sort((t1, t2) => { return MessageManager.ParseTimeStamp(t2.createdAt).CompareTo(MessageManager.ParseTimeStamp(t1.createdAt)); });

                if (success != null) success(m_CachedNews);
            }
        }
    }

    private bool IsTimeToReloadNews()
    {
        return Time.time > m_LastRefresh + 30f;
    }

    public void GetNews(bool forceReload, NewsReceivedCallback success, NoConnectionCallback noconnection, NewsReceivingError failure )
    {
        if (m_CachedNews == null || forceReload || IsTimeToReloadNews() )
        {
            m_LastRefresh = Time.time;
            StartCoroutine(GetNewsCoroutine(success, noconnection, failure));
        } else
        {
            if ( success != null ) success(m_CachedNews);
        }
    }
}
