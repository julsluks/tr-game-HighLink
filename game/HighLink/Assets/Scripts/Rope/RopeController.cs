using UnityEngine;

public class RopeConstraint2D : MonoBehaviour
{
    public Transform player1; // Reference to the first player
    public Transform player2; // Reference to the second player
    public float maxDistance = 10f; // Maximum distance before the rope pulls them back
    public float springForce = 5f; // Force applied by the spring joint
    public float damping = 0.2f; // Damping to reduce oscillations

    private SpringJoint2D springJoint;

    void Start()
    {
        // Check if player1 and player2 are assigned
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Player1 or Player2 is not assigned in the Inspector!");
            return;
        }

        // Check if player1 and player2 have Rigidbody2D components
        if (player1.GetComponent<Rigidbody2D>() == null || player2.GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError("Player1 or Player2 is missing a Rigidbody2D component!");
            return;
        }

        // Create a SpringJoint2D component on player1
        springJoint = player1.gameObject.AddComponent<SpringJoint2D>();
        springJoint.connectedBody = player2.GetComponent<Rigidbody2D>();

        // Configure the SpringJoint2D
        springJoint.distance = maxDistance; // Set the maximum distance
        springJoint.frequency = springForce; // Set the spring force
        springJoint.dampingRatio = damping; // Set the damping
        springJoint.autoConfigureDistance = false; // Disable auto-configuration
    }

    void Update()
    {
        // Check if player1 and player2 are assigned
        if (player1 == null || player2 == null)
        {
            return;
        }

        // Calculate the distance between the two players
        float distance = Vector2.Distance(player1.position, player2.position);

        // If the distance exceeds the maximum, enable the spring force
        if (distance > maxDistance)
        {
            springJoint.enabled = true;
        }
        else
        {
            // Disable the spring force when players are close enough
            springJoint.enabled = false;
        }
    }
}