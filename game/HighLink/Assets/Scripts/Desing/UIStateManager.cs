using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStateManager : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private TMP_Text welcomeText;

    private void Start()
    {
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogout);
        UpdateUI();
        
        if (UserDataHolder.Instance != null)
        {
            UserDataHolder.Instance.OnSessionChanged += UpdateUI;
        }
    }

    private void UpdateUI()
    {
        bool isLoggedIn = UserDataHolder.Instance != null && UserDataHolder.Instance.IsLoggedIn;

        if (logoutButton != null) logoutButton.gameObject.SetActive(isLoggedIn);
        if (loginButton != null) loginButton.gameObject.SetActive(!isLoggedIn);
        if (registerButton != null) registerButton.gameObject.SetActive(!isLoggedIn);

        if (welcomeText != null)
        {
            welcomeText.gameObject.SetActive(isLoggedIn);
            if (isLoggedIn) welcomeText.text = $"Welcome, {UserDataHolder.Instance.CurrentUser.name}!";
        }
    }

    private void OnLogout()
    {
        if (UserDataHolder.Instance != null)
        {
            UserDataHolder.Instance.ClearSession();
        }
        UpdateUI();
    }
}