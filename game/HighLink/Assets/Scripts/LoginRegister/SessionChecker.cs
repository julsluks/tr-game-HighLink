using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionChecker : MonoBehaviour
{
    [SerializeField] private string loginScene = "LoginScene";
    [SerializeField] private string mainScene = "MainScene";
    [SerializeField] private float checkDelay = 0.1f;

    private void Start()
    {
        Invoke(nameof(CheckSession), checkDelay);
    }

    private void CheckSession()
    {
        if (UserDataHolder.Instance != null && UserDataHolder.Instance.IsLoggedIn)
        {
            SceneManager.LoadScene(mainScene);
        }
        else
        {
            SceneManager.LoadScene(loginScene);
        }
    }
}