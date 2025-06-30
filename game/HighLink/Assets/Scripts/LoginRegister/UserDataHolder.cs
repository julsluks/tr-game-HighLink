using UnityEngine;
using System;

[System.Serializable]
public class UserData 
{
    public string id;
    public string name;
    public string email;
    public bool admin;
    public string createdAt;
    public string updatedAt;
}

public class UserDataHolder : MonoBehaviour
{
    public static UserDataHolder Instance;
    
    public UserData CurrentUser { get; private set; }
    public string AuthToken { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(AuthToken);

    public event Action OnSessionChanged;

    private const string TOKEN_KEY = "auth_token";
    private const string USER_DATA_KEY = "user_data";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetUserData(UserData user, string token)
    {
        CurrentUser = user;
        AuthToken = token;
        
        PlayerPrefs.SetString(TOKEN_KEY, token);
        PlayerPrefs.SetString(USER_DATA_KEY, JsonUtility.ToJson(user));
        PlayerPrefs.Save();
        
        OnSessionChanged?.Invoke();
        Debug.Log($"Session started for: {user.email}");
    }

    private void LoadSession()
    {
        if (PlayerPrefs.HasKey(TOKEN_KEY))
        {
            AuthToken = PlayerPrefs.GetString(TOKEN_KEY);
            CurrentUser = JsonUtility.FromJson<UserData>(PlayerPrefs.GetString(USER_DATA_KEY));
            Debug.Log($"Session loaded for: {CurrentUser.email}");
        }
    }

    public void ClearSession()
    {
        CurrentUser = null;
        AuthToken = null;
        PlayerPrefs.DeleteKey(TOKEN_KEY);
        PlayerPrefs.DeleteKey(USER_DATA_KEY);
        PlayerPrefs.Save();
        
        OnSessionChanged?.Invoke();
        Debug.Log("Session cleared");
    }
}