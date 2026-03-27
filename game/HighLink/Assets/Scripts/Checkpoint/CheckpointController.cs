using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class CheckpointController : NetworkBehaviour
{
    [SerializeField] private GameObject checkpointPrefab;
    private NetworkVariable<Vector3> cpPos1 = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> cpPos2 = new NetworkVariable<Vector3>();
    private GameObject[] visualFlags = new GameObject[2];

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCheckpointServerRpc() {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length < 2) return; // Necesitamos a ambos para guardar

        cpPos1.Value = players[0].transform.position;
        cpPos2.Value = players[1].transform.position;

        UpdateFlagsClientRpc(cpPos1.Value, cpPos2.Value);
    }

    [ClientRpc]
    private void UpdateFlagsClientRpc(Vector3 p1, Vector3 p2) {
        for (int i = 0; i < 2; i++) {
            if (visualFlags[i] == null) visualFlags[i] = Instantiate(checkpointPrefab);
            visualFlags[i].transform.position = (i == 0) ? p1 : p2;
            visualFlags[i].transform.position += new Vector3(0, 0.16f, 0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTeleportServerRpc() {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length < 2) return;

        // Teletransportamos a ambos en el servidor
        players[0].transform.position = cpPos1.Value;
        players[1].transform.position = cpPos2.Value;

        // Reset de velocidad
        foreach(var p in players) {
            var rb = p.GetComponent<Rigidbody2D>();
            if(rb != null) rb.linearVelocity = Vector2.zero;
        }

        // Forzamos la posición en los clientes para evitar el "atraviesa paredes"
        SyncPositionsClientRpc(cpPos1.Value, cpPos2.Value);
    }

    [ClientRpc]
    private void SyncPositionsClientRpc(Vector3 p1, Vector3 p2) {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length < 2) return;
        players[0].transform.position = p1;
        players[1].transform.position = p2;
    }
}