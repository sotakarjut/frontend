using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewsFeedScreen : UIScreen
{
    public NewsManager m_NewsManager;
    public UIManager m_UIManager;
    public UserManager m_UserManager;
    public EditNewsScreen m_EditScreen;

    public Image m_NewsPanel;
    public Transform m_NewsContentParent;
    public NewsTemplate m_NewsTemplate;
    public Image m_NewsSeparator;
    private List<NewsTemplate> m_NewsInstances;
    private List<Image> m_NewsSeparators;
    private int m_LastActiveNews;

    void Awake()
    {
        m_NewsInstances = new List<NewsTemplate>();
        m_NewsSeparators = new List<Image>();

        if (m_NewsTemplate)
        {
            m_NewsTemplate.gameObject.SetActive(false);
        }
        if (m_NewsSeparator)
        {
            m_NewsSeparator.gameObject.SetActive(false);
        }

        m_LastActiveNews = -1;
    }

    public override void Show()
    {
        base.Show();
        m_NewsPanel.gameObject.SetActive(false);
        m_NewsManager.GetNews(false, (news) => { NewsReceived(news); }, NoConnection, NewsFailed);
    }

    public void ForceRefresh()
    {
        m_NewsManager.GetNews(true, (news) => { NewsReceived(news); }, NoConnection, NewsFailed);
    }

    private void NoConnection()
    {
        m_UIManager.ShowNoConnection();
        m_UIManager.Logout();
    }

    private void NewsFailed()
    {
        m_UIManager.ShowNoConnection();
        m_UIManager.Logout();
    }

    private void NewsReceived(List<News> news)
    {
        if (news != null)
        {
            int i = 0;
            for (; i < news.Count; ++i)
            {
                if (m_NewsInstances.Count <= i)
                {
                    if (i > 0)
                    {
                        m_NewsSeparators.Add(Instantiate(m_NewsSeparator));
                        m_NewsSeparators[i - 1].transform.SetParent(m_NewsContentParent, false);
                    }

                    m_NewsInstances.Add(Instantiate<NewsTemplate>(m_NewsTemplate));
                    m_NewsInstances[i].transform.SetParent(m_NewsContentParent, false);
                }

                if (i > 0)
                {
                    m_NewsSeparators[i - 1].gameObject.SetActive(true);
                }

                bool editable = m_UserManager.CanCurrentUserImpersonate() ||
                    news[i].author._id != null && news[i].author._id.Equals(m_UserManager.CurrentUser);

                m_NewsInstances[i].SetData(news[i], editable, m_EditScreen);
                m_NewsInstances[i].gameObject.SetActive(true);
            }

            // disable the rest of the instances
            for (; i <= m_LastActiveNews; ++i)
            {
                if (i > 0)
                {
                    m_NewsSeparators[i - 1].gameObject.SetActive(false);
                }

                m_NewsInstances[i].gameObject.SetActive(false);
            }

            m_LastActiveNews = news.Count - 1;

            m_NewsPanel.gameObject.SetActive(true);
        }
    }
}

