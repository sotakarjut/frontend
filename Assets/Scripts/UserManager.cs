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
    public string CurrentUserRole { get; private set; }

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

    private string m_UserToken;

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
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/api/users");
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
        if (m_CachedUsers != null)
        {
            for (int i = 0; i < m_CachedUsers.Count; ++i)
            {
                if (user.Equals(m_CachedUsers[i]))
                {
                    return i;
                }
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
        if (m_CachedUsers != null)
        {
            return m_CachedUsers[index].username;
        }
        else
        {
            return "NoConnectionUser";
        }
    }

    private struct LoginData
    {
        public UserInfo user;
        public string token;
    }

    private IEnumerator TryLoginCoroutine(string username, string password, LoginSuccessfulCallback success, LoginFailedCallback failure, NoConnectionCallback noconnection)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        UnityWebRequest request = UnityWebRequest.Post("http://localhost:3000/api/login", form);
        //request.chunkedTransfer = false;

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot login: " + request.error + ", Code = " + request.responseCode);
            noconnection();
        }
        else if (request.isHttpError)
        {
            if (request.responseCode == 404)
            {
                Debug.Log("Http error: User not found: " + request.error + ", Code = " + request.responseCode);
            } else if ( request.responseCode == 401)
            {
                Debug.Log("Http error: Wrong password: " + request.error + ", Code = " + request.responseCode);
            }

            failure();
        }
        else
        {
            LoginData logindata = JsonConvert.DeserializeObject<LoginData>(request.downloadHandler.text);
            
            Debug.Log(logindata.user.username + ": " + logindata.token);

            CurrentUserName = logindata.user.username;
            CurrentUserClass = logindata.user.profile.@class;
            CurrentUserImage = GetUserImage(CurrentUserName);
            CurrentUserBalance = logindata.user.profile.balance.ToString();
            CurrentUserRole = logindata.user.profile.role;
            m_UserToken = logindata.token;

            success();
        }
    }

    private IEnumerator TestAuthorizationCoroutine()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/api/testauth");
        request.SetRequestHeader("Authorization", "Bearer " + m_UserToken);
        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: " + request.error + ", Code = " + request.responseCode);
        }
        else if (request.isHttpError)
        {
            Debug.Log("HTTP error: " + request.error + ", Code = " + request.responseCode);
        } else
        {
            Debug.Log("Authorization success!");
            Debug.Log(request.downloadHandler.text);
        }

    }

    public void TestAuthorization()
    {
        StartCoroutine(TestAuthorizationCoroutine());
    }

    public void Login(string username, string pin, LoginSuccessfulCallback successCallback, LoginFailedCallback loginFailCallback, NoConnectionCallback failCallback)
    {
        StartCoroutine(TryLoginCoroutine(username, pin, successCallback, loginFailCallback, failCallback));


        /*
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
        */
    }
    
    public void Logout()
    {
        CurrentUserName = null;
    }
	
}
