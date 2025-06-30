using UnityEngine;
using System.Collections; // Required for coroutines


public class CheckpointController : MonoBehaviour
{
    [SerializeField] private GameObject checkpointPrefab;
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    private GameObject[] checkpoints = new GameObject[2];
    private Vector3[] checkpointPosition = new Vector3[2];

    private bool ReadyToSpawn = true;

    [SerializeField] private RopeCreator ropeCreator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnCheckpoint();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnCheckpoint();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(checkpoints[0] == null || checkpoints[1] == null)
            {
                return;
            }
            MovePlayersToCheckpoint();
            ResetRope();
        }
    }

    private void SpawnCheckpoint()
    {
        if (!ReadyToSpawn)
        {
            return;
        }
        if(!player1.GetComponent<PlayerControllerOffline>().grounded || !player2.GetComponent<PlayerControllerOffline>().grounded)
        {
            return;
        }
        ReadyToSpawn = false;
        StartCoroutine("CoolDownSpawnCheckpoint");
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null)
            {
                checkpoints[i] = Instantiate(checkpointPrefab);
            }
            checkpointPosition[i] = (i == 0) ? player1.position : player2.position;            
            var positionToPlaceFlag = checkpointPosition[i];
            positionToPlaceFlag.y += 0.16f; // Additional height added to the checkpoint
            checkpoints[i].transform.position = positionToPlaceFlag;
        }
        foreach (var checkpoint in checkpoints)
        {
            Animator animator = checkpoint.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Spawn");

                StartCoroutine(ResetToIdle(animator, 0.5f));

               
            }
        }
    }

    private IEnumerator CoolDownSpawnCheckpoint()
    {
        yield return new WaitForSeconds(30f); // Cooldown duration
        ReadyToSpawn = true;
    }

    private IEnumerator ResetToIdle(Animator animator, float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the animation duration
        animator.SetTrigger("Idle");
    }

    private void MovePlayersToCheckpoint()
    {
            player1.position = checkpointPosition[0];
            player1.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            player2.position = checkpointPosition[1];
            player2.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

    }

    private void ResetRope()
    {
        if (ropeCreator != null)
        {
            ropeCreator.ResetRopePositions();
        }
        else
        {
            Debug.LogWarning("RopeCreator reference is null. Cannot reset rope positions.");
        }
    }

}
