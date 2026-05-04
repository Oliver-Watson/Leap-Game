using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    public float jumpForce = 10f;
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
    private bool wasGrounded = false;

    public static float coyoteTime = 4f;
    public static float coyoteTimeCounter;

    private float jumpBuffer = 0.2f;
    private float jumpBufferCounter;

    private float min = -1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleMoveInput();

        isGrounded = IsGrounded();

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

        Debug.Log(jumpBufferCounter + "Jump Buffer");
        Debug.Log(coyoteTimeCounter + "Coyote Time");
        Debug.Log(hasJumped + "Has Jumped");
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

    private void HandleMoveInput()
    {
        moveDirection = orientation.forward * moveAction.y + orientation.right * moveAction.x;
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDirection.normalized.x * speed;
        velocity.z = moveDirection.normalized.z * speed;
        rb.linearVelocity = velocity;
        Debug.Log("Move Direction: " + moveDirection.normalized);
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
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
