using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

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
}

public class NewsManager : MonoBehaviour
{
    public delegate void NewsReceivedCallback(List<News> news);
    public delegate void NoConnectionCallback();
    public delegate void NewsReceivingError();

    private List<News> m_CachedNews;
    private float m_LastRefresh;

    void Start ()
    {
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
            Debug.Log("News received: " + request.downloadHandler.text);

            Dictionary<string, News> newsData = JsonConvert.DeserializeObject<Dictionary<string, News>>(request.downloadHandler.text);

            m_CachedNews = new List<News>();
            foreach (News n in newsData.Values)
            {
                m_CachedNews.Add(n);
            }
            m_CachedNews.Sort((t1, t2) => { return MessageManager.ParseTimeStamp(t2.createdAt).CompareTo(MessageManager.ParseTimeStamp(t1.createdAt)); });

            if ( success != null) success(m_CachedNews);
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
