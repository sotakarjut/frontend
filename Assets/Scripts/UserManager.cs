using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class UserManager : MonoBehaviour
{
    public string CurrentUser {get; private set; }
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
    private Dictionary<string, UserInfo> m_CachedUsers;
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
        public long balance;
        public string group;
        public string name;
        public string picture;
        public string role;
        public int security_level;
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
            m_CachedUsers = new Dictionary<string, UserInfo>();

            foreach (var user in users)
            {
                Debug.Log(user.Value.username + ": " + user.Value._id);
                m_CachedUsernames.Add(user.Value.username);
                m_CachedUsers[user.Value._id] = user.Value;
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

    public string GetUserRealName(string id)
    {
        return m_CachedUsers[id].profile.name;
    }

    public string GetUsernameByIndex(int index)
    {
        return m_CachedUsernames[index];
    }

    public int GetUserIndex(string username)
    {
        if (m_CachedUsernames != null)
        {
            for (int i = 0; i < m_CachedUsernames.Count; ++i)
            {
                if (username.Equals(m_CachedUsernames[i]))
                {
                    return i;
                }
            }
        } 

        return -1;
    }

    private string GetUserIdByUsername(string username)
    {
        foreach (KeyValuePair<string, UserInfo> pair in m_CachedUsers)
        {
            if ( pair.Value.username.Equals(username) )
            {
                return pair.Key;
            }
        }

        return null;
    }

    private UserProfile InternalGetCurrentUserProfile()
    {
        if (m_CachedUsers != null)
        {
            return m_CachedUsers[CurrentUser].profile;
        }
        else
        {
            return default(UserProfile);
        }
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

    public string GetUserName(string id)
    {
        if (m_CachedUsers != null)
        {
            return m_CachedUsers[id].username;
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

            CurrentUser = logindata.user._id;
            CurrentUserName = logindata.user.username;
            CurrentUserClass = logindata.user.profile.role + " " + logindata.user.profile.group;
            CurrentUserImage = GetUserImage(CurrentUserName);
            CurrentUserBalance = logindata.user.profile.balance.ToString();
            CurrentUserRole = logindata.user.profile.role;
            m_UserToken = logindata.token;

            success();
        }
    }

    public void SetCurrentUserAuthorization(UnityWebRequest request)
    {
        request.SetRequestHeader("Authorization", "Bearer " + m_UserToken);
    }

    /*
    private IEnumerator TestAuthorizationCoroutine()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/api/testauth");
        SetCurrentUserAuthorization(request);
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
    }*/

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
        CurrentUser = null;
        CurrentUserName = null;
    }
	
}
