using UnityEngine;
using Unity.Netcode;

public class GameManagerController : NetworkBehaviour
{

    public static GameManagerController Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // [SerializeField] private RopeController ropeControllerPrefab;
    // private RopeController ropeController;
    
    private NetworkVariable<int> playersConnected = new NetworkVariable<int>(0);
    private NetworkList<ulong> playerClientIds;

    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        playerClientIds = new NetworkList<ulong>();
    }


    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsHost) return;
        
        playersConnected.Value++;
        playerClientIds.Add(clientId);
        
        if (playersConnected.Value == 2)
        {
            SpawnRope();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsHost) return;
        
        playersConnected.Value--;
        playerClientIds.Remove(clientId);
        
        // if (ropeController != null)
        // {
        //     DestroyRope();
        // }
    }

    private void SpawnRope()
    {
        if (!IsHost) return;
        
        // Find both players (host and client)
        NetworkObject[] players = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        Transform hostPlayer = null;
        Transform clientPlayer = null;
        
        foreach (var player in players)
        {
            if (player.IsPlayerObject)
            {
                if (player.OwnerClientId == NetworkManager.ServerClientId)
                    hostPlayer = player.transform;
                else
                    clientPlayer = player.transform;
            }
        }
        
        if (hostPlayer != null && clientPlayer != null)
        {
            // // Spawn the rope controller on network
            // ropeController = Instantiate(ropeControllerPrefab);
            // ropeController.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.ServerClientId);
            
            // // Initialize the rope between players
            // ropeController.InitializeRope(hostPlayer, clientPlayer);
        }
    }

    private void DestroyRope()
    {
        // if (ropeController != null)
        // {
        //     ropeController.GetComponent<NetworkObject>().Despawn();
        //     Destroy(ropeController.gameObject);
        //     ropeController = null;
        // }
    }
}
