using UnityEngine;

public class CameraFollowPlayers : MonoBehaviour
{
    public Transform player1; // Reference to Player 1
    public Transform player2; // Reference to Player 2

    [SerializeField] private float camModifier = 0.9f; // Camera modifier to adjust the position
    
    void Update() 
    {
        if (player1 == null || player2 == null)
        {
            return;
        }
        
        Vector3 midpoint = (player1.position + player2.position) * camModifier / 2f;
        transform.position = new Vector3(midpoint.x, midpoint.y, transform.position.z);

        GameController.Instance.UpdatePosition(transform.position);
    }
}