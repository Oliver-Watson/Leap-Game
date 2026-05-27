using System;
using System.Collections;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
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
    [SerializeField] private float momentumCarryFactor;

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
    private float lastVertDashForce;
    private Vector3 lastVelocity;

    //private float targetMoveSpeed;
    //private float lastSpeedX;
    //private float lastSpeedZ;
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
    public static bool dashing = false;
    private bool dashLastFrame = false;
    private float smoothingTime = 1;
    

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
        lastVertDashForce = currentVertDashForce;
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

        Debug.Log("Is grounded " + isGrounded);

        Debug.Log("Dash time counter " + dashTimeCounter);

        Debug.Log("Has dashed " + dashing);

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
        if (isGrounded && !wasGrounded || isGrounded && dashing)
        {
            coyoteTimeCounter = coyoteTime;

            hasJumped = false;
            jumpedOffGround = false;
            //Debug.Log("Was not grounded last frame and is grounded (jump condition)");
        }
        else if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
            coyoteTimeCounter = Mathf.Max(coyoteTimeCounter, min);
            //Debug.Log("Not grounded (jump condition)");
        }

        if (!hasJumped && coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            PerformJump();
            hasJumped = true;
            //Debug.Log("Can jump (jump condition");

            if (isGrounded)
            {
                jumpedOffGround = true;
            }
        }

        Debug.Log("Has jumped" + hasJumped);
        Debug.Log("Was grounded " + wasGrounded);

        jumpBufferCounter -= Time.deltaTime;
        jumpBufferCounter = Mathf.Max(jumpBufferCounter, min);

        wasGrounded = isGrounded;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //dashCounter++;

            if (!dashing)
            {
                Debug.Log("Dash");

                dashTimeCounter = dashTime;

                GetDashDirection();

                // PerformDash();

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

        currentHorDashForce = Mathf.Max(currentHorDashForce, speed);
        currentVertDashForce = Mathf.Max(currentVertDashForce, 0f);

        //lastHorDashForce = currentHorDashForce;

        //lastSpeedX = rb.linearVelocity.x;
        //lastSpeedZ = rb.linearVelocity.z;

        //momentumCarry = rb.linearVelocity.magnitude;

        Debug.Log(currentHorDashForce + "Current dash force");
        Debug.Log("Dashing");
        Debug.Log("Dash Velocity" + rb.linearVelocity);
        Debug.Log("Dash velocity magnitude" + rb.linearVelocity.magnitude);
        Debug.Log("Dash velocity normalized" + dashMoveDirection.normalized * currentHorDashForce);
    }

    private void HandleDashCondition()
    {
        //float differenceHor = lastHorDashForce - currentHorDashForce;
        //float differenceVert = lastVertDashForce = currentVertDashForce;

        Vector3 difference = lastVelocity - rb.linearVelocity;
        lastVelocity = rb.linearVelocity;
        lastHorDashForce = currentHorDashForce;

        // Check if dashing last frame
        bool dashLast = dashing;

        if (dashTimeCounter > 0)
        {
            PerformDash();
            dashing = true;
            //dashCounter++;
            rb.useGravity = false;
        }

        // If player was dashing last frame
        if (dashLast)
        {
            // If speed has not changed since last frame 
            if (difference.x + difference.z == 0 || difference.y == 0 && !isGrounded)
            {
                // End the dash and carry over momentum
                dashing = false;

                rb.useGravity = true;

                dashTimeCounter = 0f;

                currentHorDashForce = horizontalDashForce;
                currentVertDashForce = verticalDashForce;

                SmoothDashMomentum();
                Debug.Log("Smooth momentum");
                Debug.Log("Last velocity " + dashMoveDirection.normalized * currentHorDashForce);
            }
        }

        Debug.Log("Difference " + difference);

        Debug.Log("Previous hor velocity - current " + difference.x + difference.z);
        Debug.Log("Previous vert velocity - current " + difference.y);

        dashTimeCounter -= Time.deltaTime;
        dashTimeCounter = Mathf.Max(dashTimeCounter, 0f);
    }

    private void SmoothDashMomentum()
    {
        Vector3 momentumVelocity = rb.linearVelocity;
        momentumVelocity.x = dashMoveDirection.x * 5;
        momentumVelocity.z = dashMoveDirection.z * 5;
        rb.linearVelocity = momentumVelocity;

        Debug.Log("Momentum velocity " + momentumVelocity);

        //if (currentHorDashForce > 0)
        //{
        //    currentHorDashForce = Mathf.Lerp(currentHorDashForce, 0, smoothingTime);

        //    smoothingTime += Time.deltaTime * momentumCarryFactor;

        //    smoothingTime = Mathf.Min(smoothingTime, 1);

        //    Debug.Log("Smoothing time (smoothing)" + smoothingTime);

        //    Debug.Log("Current horizontal dash force (smoothing) " + currentHorDashForce);
        //}
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
