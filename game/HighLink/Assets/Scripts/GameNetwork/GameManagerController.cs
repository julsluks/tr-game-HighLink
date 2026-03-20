using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class GameManagerController : NetworkBehaviour
{
    public static GameManagerController Instance { get; private set; }

    [Header("Configuración del Servidor")]
    [SerializeField] private ServerConfig serverSettings;
    [SerializeField] private TMP_Text gameIdText;

    // Variables de estado
    private NetworkVariable<int> networkGameId = new NetworkVariable<int>(-1);
    private float heightToShow = 0f;
    private bool statsOnline = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        // Solo el Host se encarga de la base de datos y estadísticas
        if (IsServer)
        {
            StartCoroutine(CreateGameOnServer());
        }

        // Todos (Host y Cliente) actualizan su UI cuando el ID cambia
        networkGameId.OnValueChanged += (oldValue, newValue) => {
            if (gameIdText != null)
            {
                gameIdText.text = "Game ID: " + newValue;
            }
        };
    }

    // --- LÓGICA DE SERVIDOR / ESTADÍSTICAS ---

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
                
                StartCoroutine(CheckStatsService());
            }
            else
            {
                Debug.LogError("Error creando juego: " + req.error);
            }
        }
    }

    IEnumerator CheckStatsService()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(serverSettings.checkStatsURL))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var stats = JsonUtility.FromJson<NetworkStatsStatus>(req.downloadHandler.text);
                if (stats.state == "running")
                {
                    statsOnline = true;
                    InvokeRepeating(nameof(SendStatsTick), 1f, 2.5f);
                }
            }
        }
    }

    private void SendStatsTick()
    {
        if (statsOnline && networkGameId.Value != -1)
        {
            StartCoroutine(SendStatsCoroutine());
        }
    }

    IEnumerator SendStatsCoroutine()
    {
        string url = serverSettings.statsAPIURL + $"?game_id={networkGameId.Value}&height={heightToShow}";
        
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error enviando stats: " + req.error);
            }
        }
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        if (!IsServer) return;

        heightToShow = Mathf.Round((newPosition.y + 1.012f) * 10 * 100f) / 100f;

        if (HeightController.Instance != null)
        {
            HeightController.Instance.UpdateHeight(heightToShow);
        }
    }
}

// DEFINICIONES NECESARIAS PARA QUE EL SCRIPT COMPILE
[System.Serializable]
public class NetworkGameData { public int id; }

[System.Serializable]
public class NetworkStatsStatus { public string state; }