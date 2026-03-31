using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class GameManagerController : NetworkBehaviour
{
    public static GameManagerController Instance { get; private set; }

    [Header("Configuración UI")]
    [SerializeField] private ServerConfig serverSettings;
    [SerializeField] private TMP_Text gameIdText;
    [SerializeField] private TMP_Text heightText; 
    [SerializeField] private GameObject menuPanel;

    private NetworkVariable<int> networkGameId = new NetworkVariable<int>(-1);
    private NetworkVariable<float> networkHeight = new NetworkVariable<float>(0f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        ActualizarTextoID(networkGameId.Value);
        networkGameId.OnValueChanged += (oldV, newV) => { ActualizarTextoID(newV); };

        // Sincronizar Altura en todos los clientes
        ActualizarTextoAltura(networkHeight.Value);
        networkHeight.OnValueChanged += (oldV, newV) => { ActualizarTextoAltura(newV); };

        if (IsServer) StartCoroutine(CreateGameOnServer());
    }

    private void ActualizarTextoID(int id)
    {
        if (gameIdText != null && id != -1) gameIdText.text = "Game ID: " + id;
    }

    private void ActualizarTextoAltura(float h)
    {
        if (heightText != null) heightText.text = "Height: " + h.ToString("F2");
    }

    public void StartHostWithUI()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        NetworkManager.Singleton.StartHost();
    }

    public void StartClientWithUI()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        NetworkManager.Singleton.StartClient();
    }

    IEnumerator CreateGameOnServer()
    {
        if (serverSettings == null) yield break;
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(serverSettings.gameURL, ""))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var data = JsonUtility.FromJson<NetworkGameData>(req.downloadHandler.text);
                networkGameId.Value = data.id;
            }
        }
    }

    public void UpdatePosition(Vector3 pos)
    {
        if (!IsServer) return;
        networkHeight.Value = Mathf.Round((pos.y + 1.012f) * 10 * 100f) / 100f;
    }
}

[System.Serializable] public class NetworkGameData { public int id; }