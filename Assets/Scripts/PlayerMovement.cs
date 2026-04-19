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
    private bool hasJumped = false;
    private bool wasGrounded = false;

    private float coyoteTime = 2f;
    private float coyoteTimeCounter;

    private float jumpBuffer = 2.2f;
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

        if (isGrounded && !wasGrounded)
        {
            coyoteTimeCounter = coyoteTime;

            hasJumped = false;
        }
        else if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //if (jumpIntent)
        //{
        //    jumpBufferCounter = jumpBuffer;
        //}
        //else
        //{
        //    jumpBufferCounter -= Time.deltaTime;
        //}
        jumpBufferCounter -= Time.deltaTime;

        if (!hasJumped && coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            PerformJump();
            hasJumped = true;
        }

        //if (isGrounded && !wasGrounded)
        //{
        //    coyoteTimeCounter = coyoteTime;

        //    hasJumped = false;
        //}
        //else if (!isGrounded)
        //{
        //    coyoteTimeCounter -= Time.deltaTime;
        //}

        wasGrounded = isGrounded;

        Debug.Log(jumpBufferCounter + "Jump Buffer");
        Debug.Log(coyoteTimeCounter + "Coyote Time");
        Debug.Log(hasJumped + "Has Jumped");
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
            //jumpIntent = true;
            jumpBufferCounter = jumpBuffer;
        }
        //else
        //{
        //    jumpIntent = false;
        //}
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
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
