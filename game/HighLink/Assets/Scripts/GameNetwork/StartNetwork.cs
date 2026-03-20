using UnityEngine;
using Unity.Netcode;

public class StartNetwork : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;

    public void StartServer()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            HideUI();
        }
    }
    
    public void StartClient()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.StartClient())
        {
            HideUI();
        }
    }
    
    public void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            HideUI();
        }
    }

    private void HideUI()
    {
        if (menuUI != null)
        {
            menuUI.SetActive(false); 
        }
    }
}