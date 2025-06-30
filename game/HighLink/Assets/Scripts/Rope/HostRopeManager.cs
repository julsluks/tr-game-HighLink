using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class HostRopeManager : NetworkBehaviour
{
    [SerializeField] private GameObject ropePrefab;
    [SerializeField] private GameObject cameraPrefab;
    
    // Add these new variables
    private RopeCreator currentRope;
    private NetworkVariable<RopeState> ropeState = new NetworkVariable<RopeState>();
    private float lastSyncTime;
    [SerializeField] private float syncInterval = 0.1f;

    private struct RopeState : INetworkSerializable
    {
        public Vector3[] fragmentPositions;
        public Quaternion[] fragmentRotations;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int length = fragmentPositions?.Length ?? 0;
            serializer.SerializeValue(ref length);
            
            if (serializer.IsReader)
            {
                fragmentPositions = new Vector3[length];
                fragmentRotations = new Quaternion[length];
            }

            for (int i = 0; i < length; i++)
            {
                serializer.SerializeValue(ref fragmentPositions[i]);
                serializer.SerializeValue(ref fragmentRotations[i]);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
        
        // Add this for client-side updates
        ropeState.OnValueChanged += OnRopeStateChanged;
    }

    private void Update()
    {
        // Add this new update logic
        if (IsHost && currentRope != null && Time.time - lastSyncTime >= syncInterval)
        {
            UpdateRopeState();
            lastSyncTime = Time.time;
        }
    }

    private void UpdateRopeState()
    {
        if (currentRope == null || currentRope.ropeFragments == null) return;

        var newState = new RopeState
        {
            fragmentPositions = new Vector3[currentRope.ropeFragments.Length],
            fragmentRotations = new Quaternion[currentRope.ropeFragments.Length]
        };

        for (int i = 0; i < currentRope.ropeFragments.Length; i++)
        {
            if (currentRope.ropeFragments[i] != null)
            {
                newState.fragmentPositions[i] = currentRope.ropeFragments[i].transform.position;
                newState.fragmentRotations[i] = currentRope.ropeFragments[i].transform.rotation;
            }
        }

        ropeState.Value = newState;
    }

    private void OnRopeStateChanged(RopeState previous, RopeState current)
    {
        if (currentRope == null || currentRope.ropeFragments == null) return;

        for (int i = 0; i < Mathf.Min(currentRope.ropeFragments.Length, current.fragmentPositions.Length); i++)
        {
            if (currentRope.ropeFragments[i] != null)
            {
                currentRope.ropeFragments[i].transform.position = Vector3.Lerp(
                    currentRope.ropeFragments[i].transform.position,
                    current.fragmentPositions[i],
                    Time.deltaTime * (1f/syncInterval) * 2f);

                currentRope.ropeFragments[i].transform.rotation = Quaternion.Slerp(
                    currentRope.ropeFragments[i].transform.rotation,
                    current.fragmentRotations[i],
                    Time.deltaTime * (1f/syncInterval) * 2f);
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.ConnectedClients.Count == 2)
        {
            GameObject[] players = new GameObject[2];
            players[0] = GameObject.Find("RedPlayer(Clone)");
            players[1] = GameObject.Find("YellowPlayer(Clone)");

            if (players[0] != null && players[1] != null)
            {
                SpawnRopeClientRpc(
                    new NetworkObjectReference(players[0]),
                    new NetworkObjectReference(players[1])
                );
            }
        }
    }

    [ClientRpc]
    private void SpawnRopeClientRpc(NetworkObjectReference player1Ref, NetworkObjectReference player2Ref)
    {
        if (player1Ref.TryGet(out NetworkObject player1) && 
            player2Ref.TryGet(out NetworkObject player2))
        {
            InstantiateRope(player1.gameObject, player2.gameObject);
            InstantiateCamera(player1.gameObject, player2.gameObject);
        }
    }

    private void InstantiateRope(GameObject player1, GameObject player2)
    {
        GameObject rope = Instantiate(ropePrefab);
        currentRope = rope.GetComponent<RopeCreator>(); // Store reference to current rope

        currentRope.player1 = player1.transform;
        currentRope.player2 = player2.transform;

        var ropeController = rope.GetComponent<RopeConstraint2D>();
        ropeController.player1 = player1.transform;
        ropeController.player2 = player2.transform;
    }

    private void InstantiateCamera(GameObject player1, GameObject player2)
    {
        GameObject camera = Instantiate(cameraPrefab);
        var CameraFollowPlayers = camera.GetComponentInChildren<CameraFollowPlayers>();

        if (CameraFollowPlayers != null)
        {
            CameraFollowPlayers.player1 = player1.transform;
            CameraFollowPlayers.player2 = player2.transform;
        }
    }

    public override void OnDestroy()
    {
        if (IsHost)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
        ropeState.OnValueChanged -= OnRopeStateChanged;
    }
}