using UnityEngine;

public class RopeCreator : MonoBehaviour
{
    public Transform player1; // Reference to the first player
    public Transform player2; // Reference to the second player
    public GameObject ropeFragmentPrefab; // Prefab for each rope fragment
    public int fragmentCount = 10; // Number of rope fragments
    public float ropeLength = 10f; // Total length of the rope
    public float fragmentSpacing = 0.5f; // Spacing between fragments

    public GameObject[] ropeFragments; // Array to store rope fragments


    void Start()
    {
        // Check if player1 and player2 are assigned
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Player1 or Player2 is not assigned in the Inspector!");
            return;
        }

        // Check if the rope fragment prefab is assigned
        if (ropeFragmentPrefab == null)
        {
            Debug.LogError("RopeFragmentPrefab is not assigned in the Inspector!");
            return;
        }

        // Initialize the rope fragments
        ropeFragments = new GameObject[fragmentCount];
        Vector2 ropeDirection = (player2.position - player1.position).normalized;
        Vector2 spawnPosition = player1.position;

        for (int i = 0; i < fragmentCount; i++)
        {
            // Instantiate a rope fragment
            ropeFragments[i] = Instantiate(ropeFragmentPrefab, spawnPosition, Quaternion.identity);
            spawnPosition += (Vector2)(ropeDirection * fragmentSpacing);

            // Add Rigidbody2D and Collider2D to the fragment (if not already on the prefab)
            Rigidbody2D rb = ropeFragments[i].GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = ropeFragments[i].AddComponent<Rigidbody2D>();
            }
            // rb.gravityScale = 0.05f; // Disable gravity for the rope fragments

            Collider2D collider = ropeFragments[i].GetComponent<Collider2D>();
            if (collider == null)
            {
                collider = ropeFragments[i].AddComponent<CircleCollider2D>();
            }

            // Connect the fragment to the previous fragment (or player1 for the first fragment)
            if (i == 0)
            {
                ConnectFragments(player1.gameObject, ropeFragments[i]);
            }
            else
            {
                ConnectFragments(ropeFragments[i - 1], ropeFragments[i]);
            }
        }

        // Connect the last fragment to player2
        ConnectFragments(ropeFragments[fragmentCount - 1], player2.gameObject);

        // Ignore collisions between the first two and last two fragments and players
        IgnorePlayerCollisions();
    }

    void Update()
    {
        // Reset the positions of the rope fragments based on the current player positions
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetRopePositions();
        };
    }

    void ConnectFragments(GameObject startObject, GameObject endObject)
    {
        // Add a DistanceJoint2D to connect the fragments
        DistanceJoint2D joint = startObject.AddComponent<DistanceJoint2D>();
        joint.connectedBody = endObject.GetComponent<Rigidbody2D>();
        joint.distance = fragmentSpacing; // Set the distance between fragments
        joint.autoConfigureDistance = false;
        joint.maxDistanceOnly = true; // Allow stretching but not compressing
    }

     void IgnorePlayerCollisions()
    {
        // Get all GameObjects with the "Player" tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Ignore collisions for the first two and last two fragments
        for (int i = 0; i < 2; i++)
        {
            foreach (GameObject player in players)
            {
                Collider2D playerCollider = player.GetComponent<Collider2D>();
                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(ropeFragments[i].GetComponent<Collider2D>(), playerCollider);
                    Physics2D.IgnoreCollision(ropeFragments[ropeFragments.Length - 1 - i].GetComponent<Collider2D>(), playerCollider);
                }
            }
        }
    }

    public void ResetRopePositions()
    {
        if (player1 == null || player2 == null || ropeFragments == null || ropeFragments.Length == 0)
            return;

        Vector3 startPos = player1.position;
        Vector3 endPos = player2.position;
        float totalDistance = Vector3.Distance(startPos, endPos);
        Vector3 direction = (endPos - startPos).normalized;

        // Calculate spacing based on current distance between players
        float spacing = totalDistance / (fragmentCount + 1);

        for (int i = 0; i < fragmentCount; i++)
        {
            if (ropeFragments[i] != null)
            {
                // Calculate position along the line between players
                Vector3 newPos = startPos + direction * (spacing * (i + 1));
                
                // Set position and rotation directly
                Rigidbody2D rb = ropeFragments[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.transform.position = newPos;  // Using transform.position instead
                    rb.rotation = 0f;
                }
                else
                {
                    ropeFragments[i].transform.position = newPos;
                    ropeFragments[i].transform.rotation = Quaternion.identity;
                }
            }
        }

        // Force physics update to prevent weird behavior
        Physics2D.SyncTransforms();
    }
}