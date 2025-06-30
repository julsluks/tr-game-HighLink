using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField confirmPasswordInputField;

    [Header("Buttons")]
    [SerializeField] private Button registerButton;

    [Header("UI Feedback")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color successColor = Color.green;

    // URL of your registration endpoint on the server
    [Header("Server Settings")]
    [SerializeField] private string registerEndpoint = "http://localhost:4000/api/users";

    private void Start()
    {
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        confirmPasswordInputField.contentType = TMP_InputField.ContentType.Password;
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
    }

    private void OnRegisterButtonClicked()
    {
        string name = nameInputField.text.Trim();
        string email = emailInputField.text.Trim();
        string password = passwordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;

        // Validations
        if (string.IsNullOrEmpty(name))
        {
            ShowFeedback("Error: Name field cannot be empty", errorColor);
            StartCoroutine(HideFeedbackAfterDelay(7f));
            Debug.LogError("Error: Name field cannot be empty");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowFeedback("Error: Email field cannot be empty", errorColor);
            StartCoroutine(HideFeedbackAfterDelay(7f));
            Debug.LogError("Error: Email field cannot be empty");
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowFeedback("Error: Email format is not valid", errorColor);
            StartCoroutine(HideFeedbackAfterDelay(7f));
            Debug.LogError("Error: Email format is not valid");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowFeedback("Error: Password field cannot be empty", errorColor);
            StartCoroutine(HideFeedbackAfterDelay(7f));
            Debug.LogError("Error: Password field cannot be empty");
            return;
        }

        if (password.Length < 8)
        {
            ShowFeedback("Error: Password must be at least 8 characters long", errorColor);
            StartCoroutine(HideFeedbackAfterDelay(7f));
            Debug.LogError("Error: Password must be at least 8 characters long");
            return;
        }

        if (password != confirmPassword)
        {
            ShowFeedback("Error: Passwords do not match", errorColor);
            StartCoroutine(HideFeedbackAfterDelay(7f));
            Debug.LogError("Error: Passwords do not match");
            return;
        }

        // If everything is valid, proceed with registration
        StartCoroutine(RegisterUser(name, email, password));
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

    private IEnumerator RegisterUser(string nameCli, string email, string password)
    {
        registerButton.interactable = false;
        ShowFeedback("Registering...", Color.yellow);
        StartCoroutine(HideFeedbackAfterDelay(7f));

        UserData userData = new UserData
        {
            name = nameCli,
            email = email,
            password = password
        };

        // Convert to JSON
        string jsonData = JsonUtility.ToJson(userData);

        // Create the request
        UnityWebRequest request = new UnityWebRequest(registerEndpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for response
        yield return request.SendWebRequest();

        // Handle the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            ShowFeedback("Registration successful!", successColor);
            StartCoroutine(HideFeedbackAfterDelay(7f));
            Debug.Log("Registration successful!");
            Debug.Log("Server response: " + request.downloadHandler.text);

            yield return new WaitForSeconds(10f);
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            if (request.responseCode == 400) // Bad Request (invalid data)
            {
                ShowFeedback("Error: " + request.downloadHandler.text, errorColor);
                StartCoroutine(HideFeedbackAfterDelay(7f));
            }
            else if (request.responseCode == 409) // Conflict (user/email already exists)
            {
                ShowFeedback("Error: Email is already registered", errorColor);
                StartCoroutine(HideFeedbackAfterDelay(7f));
            }
            else
            {
                ShowFeedback("Registration error: " + request.error, errorColor);
                StartCoroutine(HideFeedbackAfterDelay(7f));
                Debug.LogError("Registration error: " + request.error);
                Debug.LogError("Server response: " + request.downloadHandler.text);
            }
        }

        registerButton.interactable = true;
    }

    private void ShowFeedback(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true); // In case it was hidden
    }

    private IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.gameObject.SetActive(false);
    }

    // Class to serialize user data
    [System.Serializable]
    private class UserData
    {
        public string name;
        public string email;
        public string password;
    }
}