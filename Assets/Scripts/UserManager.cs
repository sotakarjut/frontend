using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class UserManager : MonoBehaviour
{
    public string CurrentUserName { get; private set; }
    public string CurrentUserClass { get; private set; }
    public Sprite CurrentUserImage { get; private set; }
    public string CurrentUserBalance { get; private set; }

    public string ExampleClass;
    public Sprite ExampleImage;
    public string ExampleBalance;
    public List<string> ExampleUsers;

    public UIManager m_Manager;

    public delegate void UsersReceivedCallback(List<string> users);
    public delegate void UserProfileReceivedCallback(UserProfile userinfo);
    public delegate void LoginSuccessfulCallback();
    public delegate void LoginFailedCallback();
    public delegate void NoConnectionCallback();

    private delegate void UsersReadyCallback();
    private List<UserInfo> m_CachedUsers;
    private List<string> m_CachedUsernames;

    [System.Serializable]
    public struct UserInfo
    {
        public string _id;
        public string username;
        public UserProfile profile;
    }

    public void NoConnection()
    {
        m_Manager.Logout();
        m_Manager.ShowNoConnection();
    }

    [System.Serializable]
    public struct UserProfile
    {
        public string @class;
        public string name;
        public string picture;
        public string role;
        public int security_level;
        public int balance;
    }

    void Start ()
    {
	}

    public Sprite GetUserImage(string username)
    {
        return ExampleImage;
    }

    private IEnumerator GetUsersCoroutine(UsersReadyCallback callback, NoConnectionCallback noconnectionCallback)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/users");
        request.chunkedTransfer = false;

        yield return request.SendWebRequest();

        while ( !request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if ( request.isNetworkError)
        {
            Debug.Log("Network error: Cannot get users: " + request.error + ", Code = " + request.responseCode);
            noconnectionCallback();
        }
        else if (request.isHttpError)
        {
            Debug.Log("Http error: Cannot get users: " + request.error + ", Code = " + request.responseCode);
            noconnectionCallback();
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            Dictionary<string, UserInfo> users = JsonConvert.DeserializeObject<Dictionary<string, UserInfo>>(request.downloadHandler.text);
            m_CachedUsernames = new List<string>();
            m_CachedUsers = new List<UserInfo>();

            foreach (var user in users)
            {
                Debug.Log(user.Value.username + ": " + user.Value._id);
                m_CachedUsernames.Add(user.Value.username);
                m_CachedUsers.Add(user.Value);
            }

            if ( callback != null )
            {
                callback();
            }
        }
    }

    public void GetUsers(UsersReceivedCallback callback, NoConnectionCallback failCallback)
    {
        if (m_CachedUsernames != null)
        {
            callback(m_CachedUsernames);
        }
        else
        {
            StartCoroutine(GetUsersCoroutine( () => { callback(m_CachedUsernames); }, failCallback ));
        }
    }

    public int GetUserIndex(string user)
    {
        for (int i = 0; i < m_CachedUsers.Count; ++i)
        {
            if ( user.Equals(m_CachedUsers[i]))
            {
                return i;
            }
        }
        return -1;
    }

    private UserProfile InternalGetCurrentUserProfile()
    {
        for (int i = 0; i < m_CachedUsers.Count; ++i)
        {
            if (m_CachedUsers[i].username != null && m_CachedUsers[i].username.Equals(CurrentUserName))
            {
                return m_CachedUsers[i].profile;
            }
        }
        return default(UserProfile);
    }

    public void GetCurrentUserInfo(UserProfileReceivedCallback callback, NoConnectionCallback failCallback)
    {
        if (m_CachedUsers != null)
        {
            callback(InternalGetCurrentUserProfile());
        } else
        {
            StartCoroutine(GetUsersCoroutine( () => { callback(InternalGetCurrentUserProfile()); }, failCallback ));
        }
    }

    public string GetUserNameByIndex(int index)
    {
        return m_CachedUsers[index].username;
    }

    public void Login(string username, string pin, LoginSuccessfulCallback successCallback, LoginFailedCallback loginFailCallback, NoConnectionCallback failCallback)
    {
        StartCoroutine(GetUsersCoroutine(() =>
        {
            if (m_CachedUsernames != null)
            {
                CurrentUserName = username;
                CurrentUserClass = ExampleClass;
                CurrentUserImage = GetUserImage(username);
                CurrentUserBalance = ExampleBalance;

                successCallback();
            }
            else
            {
                loginFailCallback();
            }
        }, failCallback ));
    }
    
    public void Logout()
    {
        CurrentUserName = null;
    }
	
}
