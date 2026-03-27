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
    [SerializeField] private GameObject menuPanel; // AQUÍ ARRASTRA SOLO EL PANEL DE BOTONES

    private NetworkVariable<int> networkGameId = new NetworkVariable<int>(-1);
    private NetworkVariable<float> networkHeight = new NetworkVariable<float>(0f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        networkGameId.OnValueChanged += (oldV, newV) =>
        {
            if (gameIdText != null) gameIdText.text = "Game ID: " + newV;
        };

        networkHeight.OnValueChanged += (oldV, newV) =>
        {
            if (HeightController.Instance != null)
                HeightController.Instance.UpdateHeight(newV);
        };

        if (IsServer) StartCoroutine(CreateGameOnServer());
    }

    public void StartHostWithUI()
    {
        if (menuPanel != null) menuPanel.SetActive(false); // Solo oculta botones
        NetworkManager.Singleton.StartHost();
    }

    public void StartClientWithUI()
    {
        if (menuPanel != null) menuPanel.SetActive(false); // Solo oculta botones
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