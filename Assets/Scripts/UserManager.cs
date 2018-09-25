using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;

public class UserManager : MonoBehaviour
{
    public string CurrentUser {get; private set; }
    public string CurrentUserName { get; private set; }
    public string CurrentUserClass { get; private set; }
    //public Sprite CurrentUserImage { get; private set; }
    public string CurrentUserBalance { get; private set; }
    public string CurrentUserRole { get; private set; }
    public string CurrentHackedUser { get { return m_HackedUser; } }

    //public Sprite ExampleImage;
    public Sprite NoProfileImage;

    public UIManager m_Manager;

    public delegate void UsersReceivedCallback(List<string> users);
    public delegate void UserProfileReceivedCallback(UserProfile userinfo);
    public delegate void ListsReadyCallback();
    public delegate void LoginSuccessfulCallback();
    public delegate void LoginFailedCallback();
    public delegate void NoConnectionCallback();
    public delegate void HackSuccessfulCallback(int duration);
    public delegate void HackFailedCallback();

    private delegate void UsersReadyCallback();
    private Dictionary<string, UserInfo> m_CachedUsers;
    private List<string> m_CachedUsernames;
    private Dictionary<string, ListInfo> m_CachedLists;
    private List<string> m_CachedListNames;

    private Dictionary<string, Role> m_CachedRoles;

    private string m_UserToken;
    private string m_HackedUser;

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

    public struct Role
    {
        public string _id;
        public string name;
        public bool canImpersonate;
        public bool canHack;
        public bool canBeHacked;
        public int hackerLevel;
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
        public string title;
    }

    [System.Serializable]
    public struct ListInfo
    {
        public string _id;
        public string name;
    }

    void Start ()
    {
	}

    private IEnumerator GetUserImageCoroutine(string user, Image target)
    {
        string url = m_CachedUsers[user].profile.picture;
        /*
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            url = "https://upload.wikimedia.org/wikipedia/commons/c/c6/Sierpinski_square.jpg";
        } else
        {
            url = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSfW_MAkXEukLt-FRNrb17d-vrHT1cNS9PxIJT8o5nMxYcocZVU";
        }*/


        using (WWW www = new WWW(url))
        {
            yield return www;
            //www.LoadImageIntoTexture(target);
            if (www.texture != null)
            {
                target.overrideSprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
            } else
            {
                target.overrideSprite = NoProfileImage;
            }
        }
    }

    public void GetUserImage(string user, Image target)
    {
        StartCoroutine(GetUserImageCoroutine(user, target));
        //return ExampleImage;
    }

    private IEnumerator GetMailingListsCoroutine(ListsReadyCallback success, NoConnectionCallback noConnection)
    {
        UnityWebRequest request = UnityWebRequest.Get(Constants.serverAddress + "/api/mailinglists");
        request.chunkedTransfer = false;

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot get lists: " + request.error + ", Code = " + request.responseCode);
            noConnection();
        }
        else if (request.isHttpError)
        {
            Debug.Log("Http error: Cannot get lists: " + request.error + ", Code = " + request.responseCode);
            noConnection();
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            Dictionary<string, ListInfo> lists = JsonConvert.DeserializeObject<Dictionary<string, ListInfo>>(request.downloadHandler.text);
            m_CachedListNames = new List<string>();
            m_CachedLists = new Dictionary<string, ListInfo>();

            foreach (var list in lists)
            {
                Debug.Log(list.Value.name + ": " + list.Value._id);
                m_CachedListNames.Add(list.Value.name);
                m_CachedLists[list.Value._id] = list.Value;
            }

            if (success != null)
            {
                success();
            }
        }
    }

    public List<string> GetMailingListNames()
    {
        if (m_CachedListNames != null)
        {
            return m_CachedListNames;
        }
        else
        {
            return null;
        }
    }

    private IEnumerator GetUsersCoroutine(UsersReadyCallback callback, NoConnectionCallback noconnectionCallback)
    {
        UnityWebRequest request = UnityWebRequest.Get(Constants.serverAddress + "/api/users");
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

            while ( m_CachedLists == null || m_CachedRoles == null )
            {
                yield return new WaitForEndOfFrame();
            }

            if ( callback != null )
            {
                callback();
            }
        }
    }

    public string GetListIdByName(string name)
    {
        if (m_CachedLists == null) return null;

        foreach (ListInfo li in m_CachedLists.Values)
        {
            if ( name.Equals(li.name))
            {
                return li._id;
            }
        }
        return null;
    }

    public string GetUserIdByName(string name)
    {
        if (m_CachedUsers == null) return null;

        foreach (UserInfo ui in m_CachedUsers.Values)
        {
            if (name.Equals(ui.username) )
            {
                return ui._id;
            }
        }
        return null;
    }

    private IEnumerator GetRolesCoroutine()
    {
        UnityWebRequest request = UnityWebRequest.Get(Constants.serverAddress + "/api/roles");
        request.chunkedTransfer = false;

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot get roles: " + request.error + ", Code = " + request.responseCode);
        }
        else if (request.isHttpError)
        {
            Debug.Log("Http error: Cannot get roles: " + request.error + ", Code = " + request.responseCode);
        }
        else
        {
            Dictionary<string, Role> roles = JsonConvert.DeserializeObject<Dictionary<string, Role>>(request.downloadHandler.text);
            m_CachedRoles = new Dictionary<string, Role>();

            foreach (Role r in roles.Values)
            {
                m_CachedRoles.Add(r._id, r);
            }
        }
    }

    public bool CanCurrentUserHack()
    {
        string role = m_CachedUsers[CurrentUser].profile.role;
        return m_CachedRoles[role].canHack;
    }

    public bool CanCurrentUserImpersonate()
    {
        string role = m_CachedUsers[CurrentUser].profile.role;
        
        return m_CachedRoles[role].canImpersonate;
    }

    public bool CanImpersonate(string user)
    {
        string role = m_CachedUsers[user].profile.role;
        return m_CachedRoles[role].canImpersonate;
    }

    public bool CanBeHacked(string user)
    {
        string role = m_CachedUsers[user].profile.role;
        return m_CachedRoles[role].canBeHacked;
    }

    public void GetUsers(UsersReceivedCallback callback, NoConnectionCallback failCallback)
    {
        if (m_CachedUsernames != null)
        {
            callback(m_CachedUsernames);
        }
        else
        {
            StartCoroutine(GetRolesCoroutine());
            StartCoroutine(GetMailingListsCoroutine( null, failCallback ));
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

    public string GetUserByIndex(int index)
    {
        if ( m_CachedUsers == null )
        {
            return null;
        }

        string name = GetUsernameByIndex(index);
        foreach (UserInfo u in m_CachedUsers.Values)
        {
            if (name.Equals(u.username)) return u._id;
        }
        return null;
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
            if (m_HackedUser != null)
            {
                return m_CachedUsers[m_HackedUser].profile;
            }
            else
            {
                return m_CachedUsers[CurrentUser].profile;
            }
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
            StartCoroutine(GetRolesCoroutine());
            StartCoroutine(GetMailingListsCoroutine(null, failCallback));
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
        UnityWebRequest request = UnityWebRequest.Post(Constants.serverAddress + "api/login", form);
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
            //CurrentUserImage = GetUserImage(CurrentUserName);
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
        UnityWebRequest request = UnityWebRequest.Get(Constants.serverAddress + "api/testauth");
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
    }
    
    public void Logout()
    {
        m_HackedUser = null;
        CurrentUser = null;
        CurrentUserName = null;
    }

    private string GetIP()
    {
        try
        {

        string hostName = System.Net.Dns.GetHostName();
        return System.Net.Dns.GetHostEntry(hostName).AddressList[0].ToString();
        } catch (Exception e)
        {
            return "Unknown terminal: " + e.Message;
        }
    }

    private struct Duration
    {
        public int hackingDuration;
    }

    private IEnumerator TryHackCoroutine(string target, HackSuccessfulCallback success, HackFailedCallback fail, NoConnectionCallback noconnection)
    {
        WWWForm form = new WWWForm();
        form.AddField("targetId", target);
        form.AddField("terminalId", GetIP() );

        UnityWebRequest request = UnityWebRequest.Post(Constants.serverAddress + "/api/hack/intiate", form);
        SetCurrentUserAuthorization(request);
        request.chunkedTransfer = false;

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot hack: " + request.error + ", Code = " + request.responseCode);
            noconnection();
        }
        else if (request.isHttpError)
        {
            if ( request.responseCode == 400)
            {
                Debug.Log("Http error: Missing data in hack: " + request.error + ", Code = " + request.responseCode);
            }
            else if (request.responseCode == 403)
            {
                Debug.Log("Http error: Not allowed to hack: " + request.error + ", Code = " + request.responseCode);
            }
            else if (request.responseCode == 404)
            {
                Debug.Log("Http error: Hack target does not exist: " + request.error + ", Code = " + request.responseCode);
            }
            else if (request.responseCode == 500)
            {
                Debug.Log("Http error: Internal database error: " + request.error + ", Code = " + request.responseCode);
            }
            fail();
        }
        else
        {
            Duration duration = JsonConvert.DeserializeObject<Duration>(request.downloadHandler.text);

            if (success != null)
            {
                success(duration.hackingDuration/10);
            }
        }
    }


    public void Hack(string target, HackSuccessfulCallback success, HackFailedCallback fail, NoConnectionCallback noconnection)
    {
        StartCoroutine(TryHackCoroutine(target, success, fail, noconnection));
    }

    public void SetHackedUser(string target)
    {
        m_HackedUser = target;
    }
}

