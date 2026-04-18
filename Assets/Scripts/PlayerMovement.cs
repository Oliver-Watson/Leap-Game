using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float jumpForce = 5f;
    public float speed = 5f;
    public float groundDisplacement = 0.1f;

    private Vector2 moveAction;
    private Rigidbody rb;
    private float vertPos = 0;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    public float groundDistance = 1f;

    private bool isGrounded = false;
    private bool jumpPerformed = false;

    private float coyoteTime = 2f;
    private float coyoteTimeCounter;

    private float jumpBuffer = 2f;
    private float jumpBufferCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        isGrounded = IsGrounded();

        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveAction.x * speed;
        velocity.z = moveAction.y * speed;
        rb.linearVelocity = velocity;

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            if (jumpPerformed)
            {
                coyoteTimeCounter = 0f;
                jumpPerformed = false;
            }
            
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            if (jumpPerformed)
            {
                coyoteTimeCounter = 0f;
                jumpPerformed = false;
            }

            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();
        Debug.Log(moveAction + "Press");
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpPerformed = true;
        }
    }

    private bool IsGrounded()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance + groundDisplacement, groundMask);

        if (isGrounded)
        {
            Debug.Log("Grounded");
            return true;
        }

        else
        {
            Debug.Log("Not Grounded");
            return false;
        }
    }


    private void OnDrawGizmos()
    {
        // Gizmos.DrawSphere(groundCheck.position, groundDistance);
        Gizmos.DrawRay(new Vector3(groundCheck.position.x,groundCheck.position.y + groundDistance, groundCheck.position.z), Vector3.down);
    }

}
