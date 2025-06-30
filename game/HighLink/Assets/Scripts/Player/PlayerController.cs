using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 12f;
    private Rigidbody2D body;
    private Animator anim;
    private bool grounded;
    // [SerializeField] private float charSize = 0.03f;
    [SerializeField] private float jumpMultiplier = 1f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float airMoveModifier = 0.3f;

    private HashSet<Collider2D> groundContacts = new HashSet<Collider2D>();


    private bool jumpIntent = false;

    public KeyCode JumpKey = KeyCode.UpArrow; // public KeyCode JumpKey 
    public KeyCode LeftKey = KeyCode.LeftArrow; // public KeyCode LeftKey
    public KeyCode RightKey = KeyCode.RightArrow; // public KeyCode RightKey

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("I am the owner of this player object.");
            base.OnNetworkSpawn();
            Initialize();
        }
        else
        {
            Debug.Log("I am not the owner of this player object.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Initialize()
    {
        body = GetComponent<Rigidbody2D>();
        body.linearVelocity = new Vector2(0, 0);
        body.gravityScale = 1;
        body.freezeRotation = true; 

        anim = GetComponent<Animator>();
        grounded = true;

        
    }

    void Update()
    {
        if(!IsOwner)
        {
            return;
        }
        anim.SetBool("Grounded", grounded);

        if (body.linearVelocity.y < -0.15f)
        {
            // grounded = false;
            anim.SetBool("Falling", true);  // Trigger fall animation
        }

        if (body.linearVelocity.y >= -0.15f) 
        {
            anim.SetBool("Falling", false);  // Stop fall animation
        }

        if (Input.GetKeyDown(JumpKey) && grounded)
        {
            jumpIntent = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!IsOwner)
        {
            return;
        }

        float moveHorizontal = 0f;
        
        if (Input.GetKey(LeftKey))
        {
            if(!grounded)
            {
                moveHorizontal = airMoveModifier * -1f;
            }
            else
            {
                moveHorizontal = -1f;
            }
        }
        if (Input.GetKey(RightKey))
        {
            if(!grounded)
            {
                moveHorizontal = airMoveModifier * 1f;
            }
            else
            {
                moveHorizontal = 1f;
            }
        }

        // body.linearVelocity = new Vector2(moveHorizontal * speed, body.linearVelocity.y);
        body.AddForce(new Vector2(moveHorizontal * speed * Time.deltaTime, 0), ForceMode2D.Impulse);

        // Flip the sprite
        if(moveHorizontal > 0.01f)
        {
            this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if(moveHorizontal < -0.01f)
        {
            this.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (jumpIntent)
        {
            Jump();
            jumpIntent = false;
        }

        anim.SetBool("Walking", moveHorizontal != 0);
        

        if(body.linearVelocity.magnitude > maxSpeed)
        {
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, maxSpeed);
        }
    }

    private void Jump()
    {

        // body.linearVelocity = new Vector2(body.linearVelocity.x, speed * jumpMultiplier);
        body.AddForce(new Vector2(0, speed * jumpMultiplier), ForceMode2D.Impulse);
        anim.SetTrigger("Jump");
        grounded = false;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundTag(collision.gameObject.tag))
        {
            groundContacts.Add(collision.collider);
            grounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundTag(collision.gameObject.tag))
        {
            groundContacts.Remove(collision.collider);
        }
    }

    public bool IsGrounded()
    {
        return groundContacts.Count > 0;
    }

    private bool IsGroundTag(string tag)
    {
        return tag == "Grounded" || tag == "Grounded2" || tag == "Grounded3" || tag == "ground";
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger zone.");
            grounded = true;
        }
        
        
    }
}
