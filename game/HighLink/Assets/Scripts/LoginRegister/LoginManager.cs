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
    [SerializeField] private ServerConfig serverSettings;
    [SerializeField] private string successScene = "MainScene";
    [SerializeField] private string registerScene = "RegisterScene";

    [Header("Colors")]
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color signingInColor = Color.yellow;

    private string searchLoginURL ()
    {
        return serverSettings.loginURL;
    }

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

        // Asegurar que UserDataHolder existe
        if (UserDataHolder.Instance == null)
        {
            GameObject userDataHolder = new GameObject("UserDataHolder");
            userDataHolder.AddComponent<UserDataHolder>();
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
            ShowFeedback("Email cannot be empty", errorColor);
            return false;
        }

        if (!IsValidEmail(email))
        {
            ShowFeedback("Invalid email format", errorColor);
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowFeedback("Password cannot be empty", errorColor);
            return false;
        }

        return true;
    }

    private IEnumerator LoginUser(string email, string password)
    {
        string loginURL = searchLoginURL();

        loginButton.interactable = false;
        ShowFeedback("Signing in...", signingInColor);
        StartCoroutine(HideFeedbackAfterDelay(5f));

        UserLoginData loginData = new UserLoginData
        {
            email = email,
            password = password
        };

        string jsonData = JsonUtility.ToJson(loginData);
        Debug.Log($"[LOGIN] Sending request to: {loginURL}");
        Debug.Log($"[LOGIN] Credentials: {jsonData}");

        UnityWebRequest request = new UnityWebRequest(loginURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        bool loginSuccess = false;
        string userName = "";

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log($"[LOGIN] Server response: {responseText}");

            try
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseText);

                if (response != null && response.user != null && !string.IsNullOrEmpty(response.token))
                {
                    UserDataHolder.Instance.SetUserData(response.user, response.token);
                    
                    Debug.Log($"[LOGIN] User logged in successfully: {response.user.email}");
                    loginSuccess = true;
                    userName = response.user.name;
                }
                else
                {
                    string errorMsg = "Server returned invalid response";
                    Debug.LogError($"[LOGIN] {errorMsg}");
                    ShowFeedback($"Error: {errorMsg}", errorColor);
                    StartCoroutine(HideFeedbackAfterDelay(5f));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LOGIN] Error parsing response: {ex.Message}");
                Debug.LogError($"[LOGIN] Response was: {responseText}");
                ShowFeedback($"Error: Invalid server response", errorColor);
                StartCoroutine(HideFeedbackAfterDelay(5f));
            }
        }
        else
        {
            string errorMsg = GetErrorMessage(request);
            Debug.LogError($"[LOGIN] {errorMsg}");
            ShowFeedback(errorMsg, errorColor);
            StartCoroutine(HideFeedbackAfterDelay(5f));
        }

        request.Dispose();

        // Si login fue exitoso, cambiar de escena después de esperar
        if (loginSuccess)
        {
            ShowFeedback($"Welcome {userName}!", successColor);
            StartCoroutine(HideFeedbackAfterDelay(5f));
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(successScene);
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
    }

    private IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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