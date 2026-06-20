using System;
using System.Collections;
using System.Runtime.CompilerServices;
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
    [Header("Controls")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float horizontalDashForce = 2f;
    [SerializeField] private float verticalDashForce = 2f;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float sensitivity;
    [SerializeField] private float decelMoveGroundFactor = 1.0f;

    [SerializeField] public static float coyoteTime = 4f;
    [SerializeField] private float jumpBuffer = 0.2f;

    [Header("Dashing")]
    [SerializeField] private float dashDrag = 2f;
    [SerializeField] private float decelDashRate = 1.0f;
    [SerializeField] private float decelDashGroundFactor = 1.0f;
    [SerializeField] private float momentumCarryFactor;
    [SerializeField] private float forwardDragFactor;
    [SerializeField] private float endHorizontalDashForce;
    [SerializeField] private float endVerticalDashForce;
    [SerializeField] private bool normalisedDash;
    [SerializeField] private bool multiDirectionalDash;
    [SerializeField] private int allowedDash = 1;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform moveOrientation;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    // Movement variables
    private Vector3 walkVelocity;
    private Vector3 dashVelocity;
    private Vector3 dashCarryOverVelocity;
    private Vector3 walkCarryOverVelocity;

    private Vector2 moveAction;

    private Vector3 moveDirection;

    // Dynamic jump buffering and coyote time variables
    public static float coyoteTimeCounter;
    private float jumpBufferCounter;

    private Rigidbody rb;

    private float currentHorDashForce;
    private float lastHorDashForce;
    private float currentVertDashForce;
    private float lastVertDashForce;
    private Vector3 lastVelocity;

    // Carry dash momentum
    private Vector3 momentumCarry;
    private float momentumCarryH;
    private float momentumCarryV;
    private Vector3 dashMomentumDirection;
    private float dashInterpolateTime = 1.0f;
    private bool carryDashMomentum = false;
    private float dashDecayFloat = 0.25f;

    private Vector3 dashMoveDirection;

    // Carry move momentum
    private Vector3 moveVelocityAtJump;
    private float moveDragTime = 1.0f;
    private bool moving = false;
    private Vector3 lastMoveVelocity;
    private Vector2 lastMoveAction;
    private Vector2 lastDashInput;

    // Dash conditions
    private bool jumpCancelDash = false;
    private bool resetDash = true;
    private bool completeDash = true;
    private bool dashAllowed = true;
    public static bool dashing = false;

    // Ground Check
    private float groundDisplacement = 0.1f;
    public float groundDistance = 1f;

    // Gravity 
    private float gravityFactor;
    private Vector3 updateGravity;
    private bool useGravity = true;
    private float gravityCap = 1.5f;

    private bool isGrounded = false;
    public static bool hasJumped = false;
    public static bool jumpedOffGround = false;

    private bool wasGrounded = false;
    private float min = -1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHorDashForce = horizontalDashForce;
        currentVertDashForce = verticalDashForce;

        lastHorDashForce = currentHorDashForce;
        lastVertDashForce = currentVertDashForce;
    }

    private void FixedUpdate()
    {
        HandleMoveInput();

        isGrounded = IsGrounded();

        HandleJumpCondition();

        HandleDashCondition();

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        updateGravity = rb.linearVelocity;

        if (useGravity && !isGrounded)
        {
            gravityFactor = gravity;
        }
        else
        {
            gravityFactor = 0;
        }

        updateGravity.y += gravityFactor * Time.fixedDeltaTime;

        // Gravity capped as fall velocity can become too great during long air time
        updateGravity.y = Mathf.Max(updateGravity.y, gravity * gravityCap);
        rb.linearVelocity = updateGravity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();

        if (context.started)
        {
            moveDragTime = 1.0f;
            moving = true;
        }

        else if (context.canceled)
        {
            moving = false;
        }
    }

    private void HandleMoveInput() 
    {
        /// <summary>
        /// Only applies movement logic if the dash state is not active 
        /// If movement has been input during current frame, velocity is set in the normalised player direction plus input at the given speed
        /// Movement input is taken from last frame to carry momentum shortly if player stops moving. Momentum carry changes depending on grounded state
        /// </summary>

        if (!dashing && !carryDashMomentum)
        {
            if (moving)
            {
                moveDirection = moveOrientation.forward * moveAction.y + moveOrientation.right * moveAction.x;

                walkVelocity = rb.linearVelocity;
                walkVelocity.x = moveDirection.x * speed;
                walkVelocity.z = moveDirection.z * speed;

                rb.linearVelocity = walkVelocity;

                lastMoveVelocity = rb.linearVelocity;

                lastMoveAction = moveAction;
            }
            else
            {
                moveDirection = moveOrientation.forward * lastMoveAction.y + moveOrientation.right * lastMoveAction.x;

                walkCarryOverVelocity = rb.linearVelocity;
                walkCarryOverVelocity.x = moveDirection.x * speed;
                walkCarryOverVelocity.z = moveDirection.z * speed;

                if (isGrounded && wasGrounded)
                {
                    moveDragTime -= forwardDragFactor * decelMoveGroundFactor * Time.fixedDeltaTime;
                    moveDragTime = Mathf.Max(0.0f, moveDragTime);
                }

                else
                {
                    moveDragTime -= forwardDragFactor * Time.fixedDeltaTime;
                    moveDragTime = Mathf.Max(0.0f, moveDragTime);
                }

                walkCarryOverVelocity.x = Mathf.Lerp(0.0f, walkVelocity.x, moveDragTime);
                walkCarryOverVelocity.z = Mathf.Lerp(0.0f, walkVelocity.z, moveDragTime);

                rb.linearVelocity = walkCarryOverVelocity;
            }
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Reset jump buffer
            jumpBufferCounter = jumpBuffer;
        }
    }

    private void PerformJump()
    {
        Vector3 jump = rb.linearVelocity;

        jump.y = jumpForce;
        rb.linearVelocity = jump;

        // Player cannot jump until jump buffer and coyote time are reset 
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;

        // Set gravity factor to 0 to avoid gravity absorbing jump velocity if jump was performed while falling
        gravityFactor = 0.0f;
    }

    private void HandleJumpCondition()
    {
        // Prevent being stuck on jump reset
        if (isGrounded && !wasGrounded) 
        {
            // Reset coyote time 
            coyoteTimeCounter = coyoteTime;

            hasJumped = false;
            jumpCancelDash = false;
            jumpedOffGround = false;
        }
        else if (!isGrounded)
        {
            // Countdown before player loses jump starts
            coyoteTimeCounter -= Time.fixedDeltaTime;
            coyoteTimeCounter = Mathf.Max(coyoteTimeCounter, min);
            jumpCancelDash = false;
        }

        if (!hasJumped && coyoteTimeCounter > 0 && jumpBufferCounter > 0) 
        {
            PerformJump();
            hasJumped = true;

            if (dashing)
            {
                jumpCancelDash = true;
            }
            if (carryDashMomentum)
            {
                dashInterpolateTime = 0.0f;
            }

            // Accessed by platform script so platform will not move if air time is caused by player jumping off the ground
            if (isGrounded)
            {
                jumpedOffGround = true;
            }
        }

        jumpBufferCounter -= Time.fixedDeltaTime;
        jumpBufferCounter = Mathf.Max(jumpBufferCounter, min);

        wasGrounded = isGrounded;
    }

    /// <summary>
    /// Set the initial velocity and dash direction when dash key is input
    /// </summary>
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (dashAllowed) 
            {
                dashing = true;

                GetDashDirection();
                PerformDash();

                useGravity = false;
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
        // Assign dash velocity
        dashVelocity = rb.linearVelocity;
        dashVelocity.x = dashMoveDirection.x * currentHorDashForce;
        dashVelocity.y = dashMoveDirection.y * currentVertDashForce;
        dashVelocity.z = dashMoveDirection.z * currentHorDashForce;

        // Check which move input player made when performing dash to get the relative dash direction
        lastDashInput = moveAction;

        // If no movement input is received 
        if (moveAction.x == 0 && moveAction.y == 0)
        {
            // Move input is taken as the forward input
            lastDashInput.y = 1;
        }

        rb.linearVelocity = dashVelocity;
    }

    private void HandleDashCondition()
    {
        /// <summary>
        /// After initial dash velocity is set, dash velocity decays over time each frame
        /// </summary>
        if (dashing)
        {
            currentHorDashForce -= dashDrag * Time.fixedDeltaTime;
            currentVertDashForce -= dashDrag * Time.fixedDeltaTime;

            currentHorDashForce = Mathf.Max(currentHorDashForce, speed); 
            currentVertDashForce = Mathf.Max(currentVertDashForce, gravity);

            dashVelocity.x = dashMoveDirection.x * currentHorDashForce; 
            dashVelocity.y = dashMoveDirection.y * currentVertDashForce;
            dashVelocity.z = dashMoveDirection.z * currentHorDashForce;

            rb.linearVelocity = dashVelocity;

            completeDash = false;
        }

        Vector3 difference = lastVelocity - rb.linearVelocity;
        lastVelocity = rb.linearVelocity;
        lastHorDashForce = currentHorDashForce;

        // Prevents dash instantly being cancelled if the player was stationary 
        bool dashLast = dashing;

        if (dashLast)
        {
            /// <summary>
            /// If horizontal or vertical speed difference has reached 0, or player has jump cancelled, the dash state ends
            /// The current velocity is stored to carry over momentum
            /// </summary>
            if (difference.x + difference.z == 0 || difference.y == 0 && !isGrounded || jumpCancelDash)
            {
                // End the dash and carry over momentum
                dashing = false; 

                completeDash = true;

                // rb.useGravity = true; - used if using unity gravity

                useGravity = true;

                carryDashMomentum = true;

                momentumCarryH = currentHorDashForce;

                dashMomentumDirection = orientation.forward;

                currentHorDashForce = horizontalDashForce;
                currentVertDashForce = verticalDashForce;

                momentumCarry = rb.linearVelocity;

                if (!jumpCancelDash)
                {
                    Vector3 resetVelocity = rb.linearVelocity;
                    resetVelocity = new Vector3(momentumCarry.x, momentumCarry.y, momentumCarry.z);
                    rb.linearVelocity = resetVelocity;

                    dashInterpolateTime = 0.0f;
                }

                else
                {
                    // Set current velocity to the last velocity when dashing plus the jump force
                    Vector3 resetVelocity = rb.linearVelocity;
                    resetVelocity = new Vector3(momentumCarry.x, jumpForce, momentumCarry.z);
                    rb.linearVelocity = resetVelocity;

                    dashInterpolateTime = 0.0f;
                }
            }
        }

        // Only set to 0 when dash completes
        if (dashInterpolateTime < 1.0f && !dashing)
        {
            HandleDashMomentum();
        }

        dashAllowed = AllowedDash();
    }

    /// <summary>
    /// Smoothly lerp dash velocity while accounting for player move input
    /// Dash decays faster if player is moving against momentum
    /// </summary>
    private void HandleDashMomentum()
    {
        moveDirection = moveOrientation.forward * moveAction.y + moveOrientation.right * moveAction.x;

        walkVelocity.x = moveDirection.x * speed;
        walkVelocity.z = moveDirection.z * speed;

        dashCarryOverVelocity = rb.linearVelocity;
        dashCarryOverVelocity.x = Mathf.Lerp(momentumCarry.x, 0.0f, dashInterpolateTime) + walkVelocity.x;
        dashCarryOverVelocity.z = Mathf.Lerp(momentumCarry.z, 0.0f, dashInterpolateTime) + walkVelocity.z;

        rb.linearVelocity = dashCarryOverVelocity;

        // Increase dash interpolate time by different factors depending on whether in the air or grounded
        if (!isGrounded)
        {
            dashInterpolateTime += decelDashRate * Time.fixedDeltaTime;
        }
        else
        {
            dashInterpolateTime += decelDashGroundFactor * decelDashRate * Time.fixedDeltaTime;
        }

        // If previously received move input at dash is going against current direction
        if (moveAction.x * lastDashInput.x == -1 || moveAction.y * lastDashInput.y == -1)
        {
            dashInterpolateTime *= (momentumCarryH + (speed * dashDecayFloat)) / momentumCarryH;
        }

        if (dashInterpolateTime >= 1.0f)
        {
            carryDashMomentum = false;

            dashCarryOverVelocity = rb.linearVelocity;
            dashCarryOverVelocity.x = 0.0f;
            dashCarryOverVelocity.z = 0.0f;

            rb.linearVelocity = dashCarryOverVelocity + walkVelocity;
        }
    }

    // Dash condition
    private bool AllowedDash() 
    {
        if (completeDash)
        {
            if (isGrounded)
            {
                resetDash = true;
                completeDash = false;
            }
            else
            {
                resetDash = false;
            }
        }

        if (resetDash)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsGrounded()
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
        Gizmos.DrawRay(new Vector3(groundCheck.position.x,groundCheck.position.y + groundDistance, groundCheck.position.z), Vector3.down);
        // Check direction dash should be facing
        Gizmos.DrawRay(new Vector3(groundCheck.position.x, groundCheck.position.y + groundDistance, groundCheck.position.z), dashMomentumDirection);
    }

}
