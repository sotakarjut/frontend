using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start ()
    {
	}

    public Sprite GetUserImage(string username)
    {
        return ExampleImage;
    }

    public List<string> GetUsers()
    {
        return ExampleUsers;
    }

    public int GetUserIndex(string user)
    {
        for (int i = 0; i < ExampleUsers.Count; ++i)
        {
            if ( user.Equals(ExampleUsers[i]))
            {
                return i;
            }
        }
        return -1;
    }

    public string GetUserByIndex(int index)
    {
        return ExampleUsers[index];
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
