using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class GameManagerController : NetworkBehaviour
{
    public static GameManagerController Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private ServerConfig serverSettings;
    [SerializeField] private TMP_Text gameIdText;
    [SerializeField] private GameObject menuPanel; 

    // Sincronización automática de red
    private NetworkVariable<int> networkGameId = new NetworkVariable<int>(-1);
    private NetworkVariable<float> networkHeight = new NetworkVariable<float>(0f);

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn() {
        // Ejecutar inmediatamente para el cliente que se acaba de unir
        UpdateUI(networkGameId.Value);

        // Escuchar cambios futuros
        networkGameId.OnValueChanged += (oldV, newV) => { UpdateUI(newV); };

        networkHeight.OnValueChanged += (oldV, newV) => {
            if (HeightController.Instance != null)
                HeightController.Instance.UpdateHeight(newV);
        };

        if (IsServer) StartCoroutine(CreateGameOnServer());
    }

    private void UpdateUI(int id) {
        if (gameIdText != null && id != -1) {
            gameIdText.text = "Game ID: " + id;
        }
    }

    public void StartHostWithUI() {
        if (menuPanel != null) menuPanel.SetActive(false); 
        NetworkManager.Singleton.StartHost();
    }

    public void StartClientWithUI() {
        if (menuPanel != null) menuPanel.SetActive(false); 
        NetworkManager.Singleton.StartClient();
    }

    IEnumerator CreateGameOnServer() {
        if (serverSettings == null) yield break;
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(serverSettings.gameURL, "")) {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) {
                var data = JsonUtility.FromJson<NetworkGameData>(req.downloadHandler.text);
                networkGameId.Value = data.id;
            }
        }
    }

    public void UpdatePosition(Vector3 pos) {
        if (!IsServer) return;
        // Solo el host calcula la altura y la envía a la red
        networkHeight.Value = Mathf.Round((pos.y + 1.012f) * 10 * 100f) / 100f;
    }
}

[System.Serializable]
public class NetworkGameData { public int id; }