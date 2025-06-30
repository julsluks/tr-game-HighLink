using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayButtonController : MonoBehaviour
{
    [Header("Scenes Configuration")]
    [SerializeField] private string sceneIfLoggedIn = "GameScene"; // Escena cuando hay sesión
    // [SerializeField] private string sceneIfNotLoggedIn = "LoginScene"; // Escena cuando no hay sesión

    [Header("UI Feedback")]
    [SerializeField] private Button playButton;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
    }

    private void OnPlayButtonClicked()
    {
        if (UserDataHolder.Instance != null && UserDataHolder.Instance.IsLoggedIn)
        {
            // Usuario logueado -> ir a juego
            SceneManager.LoadScene(sceneIfLoggedIn);
        }
        else
        {
            SceneManager.LoadScene("GameSceneOffline");
        }
    }
}