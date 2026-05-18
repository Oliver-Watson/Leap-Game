using System;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float dashForce = 2f;
    private float currentDashForce;
    // [SerializeField] private float dashReset = 2f;
    [SerializeField] private float dashDrag = 2f;
    public float speed = 5f;
    public float groundDisplacement = 0.1f;

    [SerializeField] private float sensitivity;
    [SerializeField] private Transform orientation;
    private float mouseX;

    private Vector2 moveAction;
    private Vector2 mouseInput;

    private Vector3 moveDirection;

    private Vector3 dashMoveDirection;
    private Vector3 lastMoveDirection;
    private Vector3 dashVelocity;

    [SerializeField] private bool normalisedDash;
    [SerializeField] private bool multiDirectionalDash;

    private Rigidbody rb;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    public float groundDistance = 1f;

    private bool isGrounded = false;
    public static bool hasJumped = false;
    public static bool jumpedOffGround = false;
    public static bool hasDashed = false;

    [SerializeField] private int allowedDash = 1;
    private int dashCounter = 0;

    private bool dashCD = true;
    private bool wasGrounded = false;

    public static float coyoteTime = 4f;
    public static float coyoteTimeCounter;

    private float jumpBuffer = 0.2f;
    private float jumpBufferCounter;

    [SerializeField] private float dashTime = 0.5f;
    private float dashTimeCounter;

    private float min = -1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentDashForce = dashForce;
    }

    private void Update()
    {
        HandleMoveInput();

        isGrounded = IsGrounded();

        HandleJumpCondition();

        HandleDashCondition();

        Debug.Log(jumpBufferCounter + "Jump Buffer");
        Debug.Log(coyoteTimeCounter + "Coyote Time");
        Debug.Log(hasJumped + "Has Jumped");
        Debug.Log("Dash Counter: " + dashTimeCounter);
        Debug.Log("Dash Force: " + dashForce);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();
        Debug.Log(moveAction + "Press");
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBuffer;
        }
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
        //rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z);

        //rb.AddForce(orientation.forward * dashForce, ForceMode.Force);

        Vector3 dashVelocity = rb.linearVelocity;
        dashVelocity.x = dashMoveDirection.normalized.x * currentDashForce;
        dashVelocity.z = dashMoveDirection.normalized.z * currentDashForce;
        rb.linearVelocity = dashVelocity;
        currentDashForce -= dashDrag * Time.deltaTime;
        currentDashForce = Mathf.Max(currentDashForce, 0f);

        //lastMoveDirection = moveDirection;
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
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

    private void HandleJumpCondition()
    {
        if (isGrounded && !wasGrounded)
        {
            coyoteTimeCounter = coyoteTime;

            hasJumped = false;
            jumpedOffGround = false;
        }
        else if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
            coyoteTimeCounter = Mathf.Max(coyoteTimeCounter, min);
        }

        if (!hasJumped && coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            PerformJump();
            hasJumped = true;

            if (isGrounded)
            {
                jumpedOffGround = true;
            }
        }

        jumpBufferCounter -= Time.deltaTime;
        jumpBufferCounter = Mathf.Max(jumpBufferCounter, min);

        wasGrounded = isGrounded;
    }

    private void HandleDashCondition()
    {
        if (dashTimeCounter > 0)
        {
            PerformDash();
            //dashForce = dashReset;
            hasDashed = true;
            //dashCounter++;
        }
        if (dashTimeCounter <= 0f && isGrounded)
        {
            hasDashed = false;
            currentDashForce = dashForce;
            dashCounter = 0;
        }

        dashTimeCounter -= Time.deltaTime;
        dashTimeCounter = Mathf.Max(dashTimeCounter, 0f);
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
