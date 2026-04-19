using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float jumpForce = 5f;
    public float speed = 5f;
    public float groundDisplacement = 0.1f;

    private Vector2 moveAction;
    private Rigidbody rb;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    public float groundDistance = 1f;

    private bool isGrounded = false;
    private bool jumpIntent;
    private int jumpTracker = 0;

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

        //jumpBufferCounter -= Time.deltaTime;

        if (jumpIntent)
        {
            jumpTracker++;
        }

        if (isGrounded)
        {
            if (jumpTracker <= 1 && jumpTracker > 0)
            {
                jumpBufferCounter = jumpBuffer;
            }

            if (jumpBufferCounter > 0f && jumpTracker >= 1)
            {
                PerformJump();
            }

            coyoteTimeCounter = coyoteTime;
            jumpBufferCounter = 0;

            jumpTracker = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            if (jumpTracker <= 1 && jumpTracker > 0)
            {
                jumpBufferCounter = jumpBuffer;
                //jumpPerformed = false;
            }   
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
            {
                PerformJump();

                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
                //jumpPerformed = false;

            }

        }

        jumpIntent = false;
        jumpBufferCounter -= Time.deltaTime;

        Debug.Log(jumpBufferCounter + "Jump Buffer");
        Debug.Log(coyoteTimeCounter + "Coyote Time");
        Debug.Log(jumpIntent + "Jump Intent");
        Debug.Log(jumpTracker + "Jump Tracker");
    }

    private void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();
        Debug.Log(moveAction + "Press");
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //jumpBufferCounter = jumpBuffer;
            jumpIntent = true;
        }
        else
        {
            jumpIntent = false;
        }
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;

        //jumpPerformed = false;
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
