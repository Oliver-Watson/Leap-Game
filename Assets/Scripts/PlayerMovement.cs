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
    public float groundDistance = 0.5f;

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
        //ApplyGravity();

        isGrounded = IsGrounded();

        //Vector3 deltaDirection = new Vector3(moveAction.x * speed, vertPos, moveAction.y * speed);

        //rb.linearVelocity = deltaDirection;

        //Vector3 v = rb.linearVelocity;
        //v.y = vertPos;
        //rb.linearVelocity = v;

        //Debug.Log(transform.position);
        //Debug.Log(vertPos);

        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveAction.x * speed;
        velocity.z = moveAction.y * speed;
        rb.linearVelocity = velocity;

        //Debug.Log(coyoteTimeCounter + "Timer");
        //Debug.Log(jumpBufferCounter + "Jump Buffer");

        //if (jumpPerformed)
        //{
        //    jumpBufferCounter = jumpBuffer;
        //}
        //else
        //{
        //    jumpBufferCounter -= Time.deltaTime;
        //}

        //if (IsGrounded())
        //{ 
        //    coyoteTimeCounter = coyoteTime;
        //}
        //else
        //{
        //    if (jumpPerformed)
        //    {
        //        coyoteTimeCounter = 0f;

        //        jumpPerformed = false;

        //        Debug.Log("Jump");
        //    }
        //    else
        //    {
        //        coyoteTimeCounter -= Time.deltaTime;
        //    }
        //}
    }

    private void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();
        Debug.Log(moveAction + "Press");
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            //jumpPerformed = true;

            //vertPos = jumpForce;

            //Debug.Log(moveAction + "Press");
            //Debug.Log(coyoteTimeCounter + "Timer");

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    //private void ApplyGravity()
    //{
    //    if (IsGrounded() && vertPos < 0)
    //    {
    //        //Debug.Log("Grounded" + vertPos);
    //        vertPos = -2f;
    //    }

    //    else
    //    {
    //        vertPos += gravity * Time.deltaTime;
    //        //Debug.Log("Not Grounded!" + vertPos);
    //    }
    //}

    private bool IsGrounded()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance + groundDisplacement, groundMask);

        if (isGrounded)
        {
            return true;
        }

        else
        {
            return false;
        }
    }


    private void OnDrawGizmos()
    {
        // Gizmos.DrawSphere(groundCheck.position, groundDistance);
        Gizmos.DrawRay(new Vector3(groundCheck.position.x,groundCheck.position.y + groundDistance, groundCheck.position.z), Vector3.down);
    }

}
