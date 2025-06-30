using Unity.Netcode;
using UnityEngine;

public class HostPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;

    private GameObject[] playerInstances;

    private int playerIndex = 0;
    private ulong hostClientId;
    
    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            playerInstances = new GameObject[2];
            // Spawn host player (server + client)
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);
            
            // Handle other clients connecting
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (IsHost && clientId != NetworkManager.Singleton.LocalClientId)
        {
            SpawnPlayer(clientId);
        }
    }
    
    private void SpawnPlayer(ulong clientId)
    {
        // Your logic to choose prefab (could be based on clientId, player data, etc.)
        int prefabIndex = playerIndex == 0 ? 0 : 1;

        if (playerIndex == 0)
            hostClientId = clientId;

        playerIndex++;

        GameObject playerPrefab = playerPrefabs[prefabIndex];

        // Instantiate and spawn
        GameObject playerInstance = Instantiate(playerPrefab);

        playerInstances[prefabIndex] = playerInstance;
        
        NetworkObject playerNetworkObject = playerInstance.GetComponent<NetworkObject>();
        
        // Spawn with client ownership
        playerNetworkObject.SpawnWithOwnership(clientId);
        
        // Mark as player object
        playerNetworkObject.TrySetParent(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject);

        // Set up the rope controller
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}