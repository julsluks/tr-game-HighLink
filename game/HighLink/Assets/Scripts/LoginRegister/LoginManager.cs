using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button goToRegisterButton;

    [Header("Settings")]
    [SerializeField] private string loginEndpoint = "http://localhost:4000/api/users/login";
    [SerializeField] private string successScene = "MainScene";
    [SerializeField] private string registerScene = "RegisterScene";

    private void Start()
    {
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        
        if (goToRegisterButton != null)
        {
            goToRegisterButton.onClick.AddListener(() => 
            {
                SceneManager.LoadScene(registerScene);
            });
        }
    }

    private void OnLoginButtonClicked()
    {
        string email = emailInputField.text.Trim();
        string password = passwordInputField.text;

        if (!ValidateInputs(email, password))
            return;

        StartCoroutine(LoginUser(email, password));
    }

    private bool ValidateInputs(string email, string password)
    {
        if (string.IsNullOrEmpty(email))
        {
            ShowFeedback("Email cannot be empty", Color.red);
            return false;
        }

        if (!IsValidEmail(email))
        {
            ShowFeedback("Invalid email format", Color.red);
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowFeedback("Password cannot be empty", Color.red);
            return false;
        }

        return true;
    }

    private IEnumerator LoginUser(string email, string password)
    {
        loginButton.interactable = false;
        ShowFeedback("Signing in...", Color.yellow);

        UserLoginData loginData = new UserLoginData
        {
            email = email,
            password = password
        };

        string jsonData = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest(loginEndpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                
                if (response != null && response.user != null && !string.IsNullOrEmpty(response.token))
                {
                    UserDataHolder.Instance.SetUserData(response.user, response.token);
                    ShowFeedback($"Welcome {response.user.name}!", Color.green);
                    yield return new WaitForSeconds(1f); // Pequeña pausa
                    SceneManager.LoadScene(successScene);
                }
                else
                {
                    ShowFeedback("Invalid server response", Color.red);
                }
            }
            else
            {
                ShowFeedback(GetErrorMessage(request), Color.red);
            }
        }

        loginButton.interactable = true;
    }

    private string GetErrorMessage(UnityWebRequest request)
    {
        return request.responseCode switch
        {
            400 => "Invalid email or password",
            401 => "Unauthorized access",
            _ => $"Error: {request.error}"
        };
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
        Invoke(nameof(HideFeedback), 3f);
    }

    private void HideFeedback()
    {
        feedbackText.gameObject.SetActive(false);
    }

    [System.Serializable]
    private class UserLoginData
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    private class LoginResponse
    {
        public UserData user;
        public string token;
    }
}