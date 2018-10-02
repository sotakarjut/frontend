using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine.UI;

public class UserManager : MonoBehaviour
{
    public string CurrentUser {get; private set; }
    public string CurrentUserName { get; private set; }
    public string CurrentUserClass { get; private set; }
    public string CurrentUserBalance { get; private set; }
    public string CurrentUserRole { get; private set; }
    public string CurrentHackedUser { get { return m_HackedUser; } }

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
    private string m_TerminalName;

    [System.Serializable]
    public struct UserInfo
    {
        public string _id;
        public string username;
        public UserProfile profile;
    }

    public List<string> GetNPCRoles()
    {
        List<string> results = new List<string>();
        if (m_CachedRoles != null)
        {
            foreach (Role r in m_CachedRoles.Values)
            {
                if ( !r.canBeHacked && !r.canImpersonate )
                {
                    results.Add(r.name);
                }
            }
        }
        return results;
    }

    public List<string> GetCharacterRoles()
    {
        List<string> results = new List<string>();
        if (m_CachedRoles != null)
        {
            foreach (Role r in m_CachedRoles.Values)
            {
                if (r.canBeHacked && !r.canImpersonate)
                {
                    results.Add(r.name);
                }
            }
        }
        return results;
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

    [Serializable]
    public struct ConfigData
    {
        public string terminalName;
        public string serverAddress;
    }

    void Awake()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");
        string name;
        try
        {
            if (File.Exists(configPath))
            {
                string rawdata = File.ReadAllText(configPath);
                ConfigData data = JsonUtility.FromJson<ConfigData>(rawdata);
                name = data.terminalName;

                if ( data.serverAddress != null && data.serverAddress.Length > 0 )
                {
                    Debug.Log("Overriding server address with [" + data.serverAddress + "]");
                    Constants.serverAddress = data.serverAddress;
                }
            }
            else
            {
                name = "Unknown";
            }
        } catch (Exception)
        {
            name = "Unknown";
        }

        string mac;
        try
        {
            mac = GetMAC();
        } catch (Exception)
        {
            mac = "Unknown";
        }

        m_TerminalName = " { \"terminal\" : { \"mac\" : \"" + mac + "\", \"name\" : \"" + name + "\" } }";
        Debug.Log("Terminal name " + m_TerminalName);
	}

    private IEnumerator GetUserImageCoroutine(string user, Image target)
    {
        if (target != null && m_CachedUsers != null && m_CachedUsers.ContainsKey(user))
        {
            string url = null;
            try
            {
                url = m_CachedUsers[user].profile.picture;
            }
            catch (Exception)
            {
                target.overrideSprite = NoProfileImage;
            }

            using (WWW www = new WWW(url))
            {
                yield return www;
                if (www.texture != null)
                {
                    try
                    {
                        target.overrideSprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
                    } catch (Exception)
                    {
                        target.overrideSprite = NoProfileImage;
                    }
                }
                else
                {
                    target.overrideSprite = NoProfileImage;
                }
            }
        } else
        {
            if ( target != null ) target.overrideSprite = NoProfileImage;
        }
    }

    public void GetUserImage(string user, Image target)
    {
        StartCoroutine(GetUserImageCoroutine(user, target));
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
            if (noConnection != null) noConnection();
        }
        else if (request.isHttpError)
        {
            Debug.Log("Http error: Cannot get lists: " + request.error + ", Code = " + request.responseCode);
            if (noConnection != null) noConnection();
        }
        else
        {
            try
            {
                Dictionary<string, ListInfo> lists = JsonConvert.DeserializeObject<Dictionary<string, ListInfo>>(request.downloadHandler.text);
                m_CachedListNames = new List<string>();
                m_CachedLists = new Dictionary<string, ListInfo>();

                foreach (var list in lists)
                {
                    //Debug.Log(list.Value.name + ": " + list.Value._id);
                    m_CachedListNames.Add(list.Value.name);
                    m_CachedLists[list.Value._id] = list.Value;
                }

                if (success != null)
                {
                    success();
                }
            } catch (Exception)
            {
                Debug.LogWarning("Warning: Getting mailing lists failed.");
                m_CachedListNames = new List<string>();
                m_CachedLists = new Dictionary<string, ListInfo>();
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
            if ( noconnectionCallback != null) noconnectionCallback();
        }
        else if (request.isHttpError)
        {
            Debug.Log("Http error: Cannot get users: " + request.error + ", Code = " + request.responseCode);
            if (noconnectionCallback != null) noconnectionCallback();
        }
        else
        {
            Dictionary<string, UserInfo> users = null;
            try
            {
                users = JsonConvert.DeserializeObject<Dictionary<string, UserInfo>>(request.downloadHandler.text);
                m_CachedUsernames = new List<string>();
                m_CachedUsers = new Dictionary<string, UserInfo>();
            }
            catch (Exception)
            {
                Debug.LogError("Error: cannot deserialize user data");
                m_CachedUsernames = null;
                m_CachedUsers = null;
            }

            if (users != null)
            {
                foreach (var user in users)
                {
                    //Debug.Log(user.Value.username + ": " + user.Value._id);
                    m_CachedUsernames.Add(user.Value.username);
                    m_CachedUsers[user.Value._id] = user.Value;
                }

                int loops = 0;
                while ( (m_CachedLists == null || m_CachedRoles == null) && loops < 500)
                {
                    ++loops;
                    yield return new WaitForEndOfFrame();
                }

                if (callback != null)
                {
                    callback();
                }
            }
        }
    }

    public string GetListIdByName(string name)
    {
        if (m_CachedLists == null || name == null) return null;

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
        if (m_CachedUsers == null || name == null) return null;

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
            Dictionary<string, Role> roles = null;
            try
            {
                roles = JsonConvert.DeserializeObject<Dictionary<string, Role>>(request.downloadHandler.text);
            } catch (Exception)
            {
                Debug.LogError("Error: Can't deserialize role data");
            }

            if (roles != null)
            {
                m_CachedRoles = new Dictionary<string, Role>();

                foreach (Role r in roles.Values)
                {
                    m_CachedRoles.Add(r._id, r);
                }
            }
        }
    }

    public int GetCurrentUserHackerLevel()
    {
        if (m_CachedUsers == null || CurrentUser == null || !m_CachedUsers.ContainsKey(CurrentUser)) return 0;

        string role = m_CachedUsers[CurrentUser].profile.role;

        if (m_CachedRoles != null && m_CachedRoles.ContainsKey(role))
        {
            return m_CachedRoles[role].hackerLevel;
        } else
        {
            return 0;
        }
    }

    public bool CanCurrentUserHack()
    {
        if (m_CachedUsers == null || CurrentUser == null || !m_CachedUsers.ContainsKey(CurrentUser)) return false;

        string role = m_CachedUsers[CurrentUser].profile.role;
        if (m_CachedRoles != null && m_CachedRoles.ContainsKey(role))
        {
            return m_CachedRoles[role].canHack;
        } else
        {
            return false;
        }
    }

    public bool CanCurrentUserImpersonate()
    {
        if (m_CachedUsers == null || CurrentUser == null || !m_CachedUsers.ContainsKey(CurrentUser)) return false;

        string role = m_CachedUsers[CurrentUser].profile.role;
        if (m_CachedRoles != null && m_CachedRoles.ContainsKey(role))
        {
            return m_CachedRoles[role].canImpersonate;
        } else
        {
            return false;
        }
    }

    public bool CanImpersonate(string user)
    {
        if (m_CachedUsers == null || user == null || !m_CachedUsers.ContainsKey(user)) return false;

        string role = m_CachedUsers[user].profile.role;
        if (m_CachedRoles != null && m_CachedRoles.ContainsKey(role))
        {
            return m_CachedRoles[role].canImpersonate;
        } else
        {
            return false;
        }
    }

    public bool CanBeHacked(string user)
    {
        if (m_CachedUsers == null || user == null || !m_CachedUsers.ContainsKey(user)) return true;

        string role = m_CachedUsers[user].profile.role;
        if (m_CachedRoles != null && m_CachedRoles.ContainsKey(role))
        {
            return m_CachedRoles[role].canBeHacked;
        } else
        {
            return true;
        }
    }

    public void GetUsers(UsersReceivedCallback callback, NoConnectionCallback failCallback)
    {
        if (m_CachedUsernames != null && callback != null)
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
        if (m_CachedUsers != null && m_CachedUsers.ContainsKey(id))
        {
            return m_CachedUsers[id].profile.name;
        } else
        {
            return null;
        }
    }

    public string GetUsernameByIndex(int index)
    {
        if (m_CachedUsernames != null && index >= 0 && index < m_CachedUsernames.Count)
        {
            return m_CachedUsernames[index];
        } else
        {
            return null;
        }
    }

    public string GetUserByIndex(int index)
    {
        if ( m_CachedUsers == null )
        {
            return null;
        }

        string name = GetUsernameByIndex(index);
        if (name != null)
        {
            foreach (UserInfo u in m_CachedUsers.Values)
            {
                if (name.Equals(u.username)) return u._id;
            }
        }
        return null;
    }

    public int GetUserIndex(string username)
    {
        if (username == null || m_CachedUsernames == null) return -1;

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
        if (username == null || m_CachedUsers == null) return null;

        foreach (KeyValuePair<string, UserInfo> pair in m_CachedUsers)
        {
            if ( pair.Value.username != null && pair.Value.username.Equals(username) )
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
            if (m_HackedUser != null && m_CachedUsers.ContainsKey(m_HackedUser) )
            {
                return m_CachedUsers[m_HackedUser].profile;
            }
            else if (m_CachedUsers.ContainsKey(CurrentUser))
            {
                return m_CachedUsers[CurrentUser].profile;
            }
            else
            {
                return default(UserProfile);
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
            if (callback != null)
            {
                callback(InternalGetCurrentUserProfile());
            }
        } else
        {
            StartCoroutine(GetRolesCoroutine());
            StartCoroutine(GetMailingListsCoroutine(null, failCallback));
            StartCoroutine(GetUsersCoroutine( () => { callback(InternalGetCurrentUserProfile()); }, failCallback ));
        }
    }

    public string GetUserName(string id)
    {
        if (m_CachedUsers != null && m_CachedUsers.ContainsKey(id) )
        {
            return m_CachedUsers[id].username;
        }
        else
        {
            return "DefaultUser";
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

        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (request.isNetworkError)
        {
            Debug.Log("Network error: Cannot login: " + request.error + ", Code = " + request.responseCode);
            if ( noconnection != null) noconnection();
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

            if ( failure != null) failure();
        }
        else
        {
            LoginData logindata = default(LoginData);

            try
            {
                logindata = JsonConvert.DeserializeObject<LoginData>(request.downloadHandler.text);
            } catch (Exception)
            {
                Debug.LogError("Error: Cannot deserialize login data");
            }

            if (logindata.token != null)
            {
                //Debug.Log(logindata.user.username + ": " + logindata.token);

                CurrentUser = logindata.user._id;
                CurrentUserName = logindata.user.username;
                CurrentUserClass = logindata.user.profile.role + " " + logindata.user.profile.group;
                CurrentUserBalance = logindata.user.profile.balance.ToString();
                CurrentUserRole = logindata.user.profile.role;
                m_UserToken = logindata.token;

                if ( success != null) success();
            } else
            {
                if (failure != null) failure();
            }
        }
    }

    public void SetCurrentUserAuthorization(UnityWebRequest request)
    {
        if (request != null)
        {
            request.SetRequestHeader("Authorization", "Bearer " + m_UserToken);
        }
    }

    public void Login(string username, string pin, LoginSuccessfulCallback successCallback, LoginFailedCallback loginFailCallback, NoConnectionCallback failCallback)
    {
        StartCoroutine(TryLoginCoroutine(username, pin, successCallback, loginFailCallback, failCallback));
    }
    
    public void Logout()
    {
        CurrentUser = null;
        CurrentUserName = null;
        CurrentUserClass = null;
        CurrentUserRole = null;
        CurrentUserBalance = "0";
        m_HackedUser = null;
        m_UserToken = null;
    }

    private string GetMAC()
    {
        try
        {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet && adapter.OperationalStatus == OperationalStatus.Up)
                {
                    PhysicalAddress address = adapter.GetPhysicalAddress();
                    byte[] bytes = address.GetAddressBytes();
                    string mac = null;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                        if (i != bytes.Length - 1)
                        {
                            mac = string.Concat(mac + "-");
                        }
                    }
                    //Debug.Log(mac);
                    return mac;
                }
            }

            Debug.LogWarning("Cannot find active ethernet for MAC");
            return "Unknown";
            //string hostName = System.Net.Dns.GetHostName();
            //return System.Net.Dns.GetHostEntry(hostName).AddressList[0].ToString();
        } catch (Exception e)
        {
            Debug.LogWarning("Cannot get MAC: " + e.Message);
            return "Unknown";
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
        form.AddField("terminalId", m_TerminalName );

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
            if (noconnection != null) noconnection();
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
            if ( fail != null) fail();
        }
        else
        {
            Duration duration = default(Duration);
            try
            {
                duration = JsonConvert.DeserializeObject<Duration>(request.downloadHandler.text);
            } catch (Exception)
            {
                Debug.LogError("Error: Cannot deserialize hacking duration");
            }

            if (success != null)
            {
                success(duration.hackingDuration);
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

