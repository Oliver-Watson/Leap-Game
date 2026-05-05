using System;
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
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float dashForce = 2f;
    [SerializeField] private float dashReset = 2f;
    [SerializeField] private float dashDrag = 2f;
    public float speed = 5f;
    public float groundDisplacement = 0.1f;

    [SerializeField] private float sensitivity;
    [SerializeField] private Transform orientation;
    private float mouseX;

    private Vector2 moveAction;
    private Vector2 mouseInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    public float groundDistance = 1f;

    private bool isGrounded = false;
    public static bool hasJumped = false;
    public static bool jumpedOffGround = false;
    public static bool hasDashed = false;
    private bool dashCD = true;
    private bool wasGrounded = false;

    public static float coyoteTime = 4f;
    public static float coyoteTimeCounter;

    private float jumpBuffer = 0.2f;
    private float jumpBufferCounter;

    private float dashTime = 0.5f;
    private float dashTimeCounter;

    private float min = -1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
            if (!hasDashed)
            {
                Debug.Log("Dash");

                dashTimeCounter = dashTime;
            }
        }
    }

    private void PerformDash()
    {
        //rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z);

        //rb.AddForce(orientation.forward * dashForce, ForceMode.Force);

        moveDirection = orientation.forward;
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDirection.normalized.x * dashForce;
        velocity.z = moveDirection.normalized.z * dashForce;
        rb.linearVelocity = velocity;
        dashForce -= dashDrag * Time.deltaTime;
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    private void HandleMoveInput()
    {
        moveDirection = orientation.forward * moveAction.y + orientation.right * moveAction.x;
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDirection.normalized.x * speed;
        velocity.z = moveDirection.normalized.z * speed;
        rb.linearVelocity = velocity;
        Debug.Log("Move Direction: " + moveAction.y);
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
        }
        if (dashTimeCounter == 0 && isGrounded)
        {
            hasDashed = false;
            dashForce = dashReset;
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
