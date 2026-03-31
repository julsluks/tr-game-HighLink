using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class CheckpointController : NetworkBehaviour
{
    [SerializeField] private GameObject checkpointPrefab;
    private NetworkVariable<Vector3> cpPos1 = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> cpPos2 = new NetworkVariable<Vector3>();
    private GameObject flag1, flag2;

    void Update()
    {
        if (!IsSpawned || !IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q)) SpawnCheckpointServerRpc();
        if (Input.GetKeyDown(KeyCode.E)) RequestTeleportServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCheckpointServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length < 2) return;

        cpPos1.Value = players[0].transform.position;
        cpPos2.Value = players[1].transform.position;

        ActualizarBanderasClientRpc(cpPos1.Value, cpPos2.Value);
    }

    [ClientRpc]
    private void ActualizarBanderasClientRpc(Vector3 p1, Vector3 p2)
    {
        if (flag1 == null) flag1 = Instantiate(checkpointPrefab);
        if (flag2 == null) flag2 = Instantiate(checkpointPrefab);

        flag1.transform.position = p1 + new Vector3(0, 0.16f, 0);
        flag2.transform.position = p2 + new Vector3(0, 0.16f, 0);

        flag1.GetComponent<Animator>()?.SetTrigger("Spawn");
        flag2.GetComponent<Animator>()?.SetTrigger("Spawn");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTeleportServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length < 2) return;

        players[0].transform.position = cpPos1.Value;
        players[1].transform.position = cpPos2.Value;

        foreach (var p in players)
        {
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // Forzar posición en clientes para evitar que atraviesen el suelo al viajar
        SincronizarPosicionClientRpc(cpPos1.Value, cpPos2.Value);
    }

    [ClientRpc]
    private void SincronizarPosicionClientRpc(Vector3 p1, Vector3 p2)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length < 2) return;
        players[0].transform.position = p1;
        players[1].transform.position = p2;
    }
}