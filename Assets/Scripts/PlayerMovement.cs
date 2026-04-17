using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float gravity = -9.81f;
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

    private float coyoteTime = 0.8f;
    private float coyoteTimeCounter; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void Update()
    {
        ApplyGravity();

        Vector3 deltaDirection = new Vector3(moveAction.x * speed, vertPos, moveAction.y * speed);

        rb.linearVelocity = deltaDirection;

        //Vector3 v = rb.linearVelocity;
        //v.y = vertPos;
        //rb.linearVelocity = v;

        Debug.Log(transform.position);
        Debug.Log(vertPos);

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; 
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();
        Debug.Log(moveAction + "Press");
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && coyoteTimeCounter > 0f)
        {
            coyoteTimeCounter = 0f;

            vertPos = jumpForce;

            Debug.Log(moveAction + "Press");
            Debug.Log(coyoteTimeCounter + "Timer");
        }
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && vertPos < 0)
        {
            Debug.Log("Grounded" + vertPos);
            vertPos = -2f;
        }

        else
        {
            vertPos += gravity * Time.deltaTime;
            Debug.Log("Not Grounded!" + vertPos);
        }
    }

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
        Gizmos.DrawRay(new Vector3(groundCheck.position.x,groundCheck.position.y + groundDistance,groundCheck.position.x), Vector3.down);
    }

}
