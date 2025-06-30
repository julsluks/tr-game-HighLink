using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // Añade esta línea

[RequireComponent(typeof(Button))]
public class LogoutButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string loginSceneName = "LoginScene";
    [SerializeField] private float sceneChangeDelay = 0.5f;
    [SerializeField] private GameObject loginRegisterPanel;

    private Button logoutButton;

    private void Awake()
    {
        logoutButton = GetComponent<Button>();
        logoutButton.onClick.AddListener(OnLogoutClicked);
        
        if (UserDataHolder.Instance != null)
        {
            UserDataHolder.Instance.OnSessionChanged += UpdateVisibility;
        }
        
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        if (UserDataHolder.Instance != null)
        {
            UserDataHolder.Instance.OnSessionChanged -= UpdateVisibility;
        }
    }

    private void UpdateVisibility()
    {
        bool shouldShow = UserDataHolder.Instance != null && UserDataHolder.Instance.IsLoggedIn;
        
        logoutButton.gameObject.SetActive(shouldShow);
        
        if (loginRegisterPanel != null)
        {
            loginRegisterPanel.SetActive(!shouldShow);
        }
    }

    private void OnLogoutClicked()
    {
        logoutButton.interactable = false;
        
        if (UserDataHolder.Instance != null)
        {
            UserDataHolder.Instance.ClearSession();
        }

        if (loginRegisterPanel != null)
        {
            loginRegisterPanel.SetActive(true);
        }

        StartCoroutine(LoadSceneAfterDelay(loginSceneName, sceneChangeDelay));
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}