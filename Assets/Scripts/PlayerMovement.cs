using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Controls")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float horizontalDashForce = 2f;
    [SerializeField] private float verticalDashForce = 2f;
    [SerializeField] private float dashDrag = 2f;
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sensitivity;

    [SerializeField] public static float coyoteTime = 4f;
    [SerializeField] private float jumpBuffer = 0.2f;

    [SerializeField] private bool normalisedDash;
    [SerializeField] private bool multiDirectionalDash;
    [SerializeField] private int allowedDash = 1;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    // Dynamic jump buffering and coyote time variables
    public static float coyoteTimeCounter;
    private float jumpBufferCounter;

    private Rigidbody rb;

    private float currentHorDashForce;
    private float lastHorDashForce;
    private float currentVertDashForce;
    //private float lastVertDashForce;

    private float targetMoveSpeed;
    private float lastSpeedX;
    private float lastSpeedZ;
    private float momentumCarry;

    // Ground Check
    private float groundDisplacement = 0.1f;
    public float groundDistance = 1f;

    private Vector2 moveAction;

    private Vector3 moveDirection;

    private Vector3 dashMoveDirection;
    private Vector3 lastMoveDirection;
    private Vector3 dashVelocity;

    private bool isGrounded = false;
    public static bool hasJumped = false;
    public static bool jumpedOffGround = false;
    public static bool hasDashed = false;
    private bool dashLastFrame = false;
    

    private int dashCounter = 0;

    private bool dashCD = true;
    private bool wasGrounded = false;
    
    private float dashTimeCounter;

    private float min = -1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHorDashForce = horizontalDashForce;
        currentVertDashForce = verticalDashForce;
        lastHorDashForce = currentHorDashForce;
        //lastVertDashForce = currentVertDashForce;
    }

    private void Update()
    {
        HandleMoveInput();

        isGrounded = IsGrounded();

        HandleJumpCondition();

        HandleDashCondition();

        //dashTimeCounter -= Time.deltaTime;
        //dashTimeCounter = Mathf.Max(dashTimeCounter, 0f);

        Debug.Log("Changing speed momentum + last speed" + rb.linearVelocity.magnitude);
        Debug.Log("Last speed" + momentumCarry);

        Debug.Log("Use gravity? " + rb.useGravity);

    }

    public enum MovementState
    {
        walking,
        dashing,
        falling,
        jumping
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();
    }

    private void HandleMoveInput()
    {
        if (dashTimeCounter <= 0)
        {
            moveDirection = orientation.forward * moveAction.y + orientation.right * moveAction.x;
            Vector3 velocity = rb.linearVelocity;
            velocity.x = moveDirection.normalized.x * speed;
            velocity.z = moveDirection.normalized.z * speed;
            rb.linearVelocity = velocity;
            Debug.Log("Move Direction: " + moveAction.y);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBuffer;
        }
    }

    private void PerformJump()
    {
        Vector3 jump = rb.linearVelocity;
        jump.y = jumpForce;
        rb.linearVelocity = jump;

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    private void HandleJumpCondition()
    {
        if (isGrounded && !wasGrounded)
        {
            coyoteTimeCounter = coyoteTime;

            hasJumped = false;
            jumpedOffGround = false;
            Debug.Log("Was not grounded last frame and is grounded (jump condition)");
        }
        else if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
            coyoteTimeCounter = Mathf.Max(coyoteTimeCounter, min);
            Debug.Log("Not grounded (jump condition)");
        }

        if (!hasJumped && coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            PerformJump();
            hasJumped = true;
            Debug.Log("Can jump");

            if (isGrounded)
            {
                jumpedOffGround = true;
            }
        }

        jumpBufferCounter -= Time.deltaTime;
        jumpBufferCounter = Mathf.Max(jumpBufferCounter, min);

        wasGrounded = isGrounded;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //dashCounter++;

            if (!hasDashed)
            {
                Debug.Log("Dash");

                dashTimeCounter = dashTime;

                GetDashDirection();

                PerformDash();

                //StartCoroutine(nameof(HandleDash));
            }
        }
    }

    private void GetDashDirection()
    {
        if (multiDirectionalDash)
        {
            if (moveAction.y == 0 && moveAction.x == 0)
            {
                dashMoveDirection = orientation.forward;
            }
            else
            {
                dashMoveDirection = orientation.forward * moveAction.y + orientation.right * moveAction.x;
            }
        }

        else if (normalisedDash)
        {
            dashMoveDirection = orientation.forward;
        }
    }

    private void PerformDash()
    {
        dashVelocity = rb.linearVelocity;
        dashVelocity.x = dashMoveDirection.x * currentHorDashForce;
        dashVelocity.y = dashMoveDirection.y * currentVertDashForce;
        dashVelocity.z = dashMoveDirection.z * currentHorDashForce;
        //dashVelocity = dashMoveDirection.normalized * currentHorDashForce;

        rb.linearVelocity = dashVelocity;

        currentHorDashForce -= dashDrag * Time.deltaTime;
        currentVertDashForce -= dashDrag * Time.deltaTime;

        currentHorDashForce = Mathf.Max(currentHorDashForce, 0f);
        currentVertDashForce = Mathf.Max(currentVertDashForce, 0f);

        //lastHorDashForce = currentHorDashForce;

        //lastSpeedX = rb.linearVelocity.x;
        //lastSpeedZ = rb.linearVelocity.z;

        //momentumCarry = rb.linearVelocity.magnitude;

        Debug.Log(currentHorDashForce + "Current dash force");
        Debug.Log("Dashing");
        Debug.Log("Dash Velocity" + rb.linearVelocity);
        Debug.Log("Dash velocity magnitude" + rb.linearVelocity.magnitude);
    }

    private void HandleDashCondition()
    {
        float differenceHor = lastHorDashForce - currentHorDashForce;
        //float differenceVert = lastVertDashForce = currentVertDashForce;

        if (differenceHor != 0)// && differenceVert != 0)
        {
            dashLastFrame = hasDashed;
            lastHorDashForce = currentHorDashForce;

            PerformDash();
            hasDashed = true;
            //dashCounter++;
            rb.useGravity = false;
            
        }
        else
        {
            rb.useGravity = true;
            //dashTimeCounter = 0f;
            Debug.Log("Smooth momentum");
        }
        if (dashTimeCounter <= 0f && isGrounded)
        {
            hasDashed = false;
            currentHorDashForce = horizontalDashForce;
            currentVertDashForce = verticalDashForce;
            dashCounter = 0;
        }

        Debug.Log("Difference " + differenceHor);

        dashTimeCounter -= Time.deltaTime;
        dashTimeCounter = Mathf.Max(dashTimeCounter, 0f);
    }

    private void SmoothDashMomentum()
    {

    }

    public bool IsGrounded()
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
