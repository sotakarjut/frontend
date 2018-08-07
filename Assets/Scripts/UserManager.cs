using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

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

    public delegate void UsersReceivedCallback(List<string> users);

    private List<UserInfo> m_CachedUsers;

    [System.Serializable]
    public struct UserInfo
    {
        public string _id;
        public string username;
    }

    void Start ()
    {
        m_CachedUsers = new List<UserInfo>();
	}

    public Sprite GetUserImage(string username)
    {
        return ExampleImage;
    }

    private IEnumerator GetUsersCoroutine(UsersReceivedCallback callback)
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
        }
        else if (request.isHttpError)
        {
            Debug.Log("Http error: Cannot get users: " + request.error + ", Code = " + request.responseCode);
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            Dictionary<string, UserInfo> users = JsonConvert.DeserializeObject<Dictionary<string, UserInfo>>(request.downloadHandler.text);
            List<string> usernames = new List<string>();
            foreach (var user in users)
            {
                Debug.Log(user.Value.username + ": " + user.Value._id);
                usernames.Add(user.Value.username);
                m_CachedUsers.Add(user.Value);
            }

            if ( callback != null )
            {
                callback(usernames);
            }
        }
    }

    public List<string> GetUsers(UsersReceivedCallback callback)
    {
        StartCoroutine(GetUsersCoroutine(callback));

        return ExampleUsers;
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

    public string GetUserByIndex(int index)
    {
        return m_CachedUsers[index].username;
    }

    public bool Login(string username, string pin)
    {
        // TODO: check login
        CurrentUserName = username;
        CurrentUserClass = ExampleClass;
        CurrentUserImage = GetUserImage(username);
        CurrentUserBalance = ExampleBalance;

        return true;
    }
    
    public void Logout()
    {
        CurrentUserName = null;
    }
	
}
