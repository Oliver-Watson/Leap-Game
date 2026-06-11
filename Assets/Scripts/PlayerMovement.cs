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
    [Header("Player Controls")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float horizontalDashForce = 2f;
    [SerializeField] private float verticalDashForce = 2f;
    [SerializeField] private float endHorizontalDashForce = 5.0f;
    [SerializeField] private float endVerticalDashForce = 5.0f;

    [SerializeField] private float dashDrag = 2f;
    [SerializeField] private float decelDashRate = 1.0f;
    [SerializeField] private float decelDashGroundFactor = 1.0f;
    [SerializeField] private float decelMoveGroundFactor = 1.0f;
    [SerializeField] private float momentumCarryFactor;
    [SerializeField] private float forwardDragFactor;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float sensitivity;

    [SerializeField] public static float coyoteTime = 4f;
    [SerializeField] private float jumpBuffer = 0.2f;

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

    private float dashTimeCounter;
    private float dashSmoothCounter;

    // Carry move momentum
    private Vector3 moveVelocityAtJump;
    private float moveDragTime = 1.0f;
    private bool moving = false;
    private Vector3 lastMoveVelocity;
    private Vector2 lastMoveAction;
    private bool movingLastFrame = false;

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
    private bool useGravity = true;

    private Vector2 moveAction;

    private Vector3 moveDirection;

    private Vector3 dashMoveDirection;

    private bool isGrounded = false;
    public static bool hasJumped = false;
    public static bool jumpedOffGround = false;
    
    
    private int dashCounter = 0;

    private bool dashCD = true;
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

    private void Update()
    {
        HandleMoveInput();

        isGrounded = IsGrounded();

        HandleJumpCondition();

        HandleDashCondition();

        ApplyGravity();

        //HandleMomentumCarryOver();

        Debug.Log("Changing speed momentum + last speed" + rb.linearVelocity.magnitude);

        Debug.Log("Last speed" + momentumCarry);

        Debug.Log("Use gravity? " + rb.useGravity);

        Debug.Log("Is grounded " + isGrounded);

        Debug.Log("Dash time counter " + dashTimeCounter);

        Debug.Log("Has dashed " + dashing);

        Debug.Log("Use gravity " + useGravity);

        Debug.Log("Velocity " + rb.linearVelocity);

    }

    private void ApplyGravity() // how to get clean gravity
    {
        Vector3 updateGravity = rb.linearVelocity;

        if (useGravity && !isGrounded)
        {
            gravityFactor = gravity;// * Time.deltaTime;
            //gravityFactor = Mathf.Max(gravityFactor, gravity * 2.0f);
        }
        else
        {
            gravityFactor = 0;
        }

        updateGravity.y += gravityFactor * Time.deltaTime;
        //updateGravity.y = Mathf.Max(updateGravity.y, gravity * 0.8f);
        rb.linearVelocity = updateGravity;
    }

    //public enum MovementState
    //{
    //    walking,
    //    dashing,
    //    falling,
    //    jumping
    //}

    public void Move(InputAction.CallbackContext context)
    {
        moveAction = context.ReadValue<Vector2>();

        if (context.started)
        {
            Debug.Log("Move started (Move input action)");
            moveDragTime = 1.0f;
            moving = true;
        }

        else if (context.canceled)
        {
            Debug.Log("Move ended (Move input action");
            moving = false;
        }
    }

    private void HandleMoveInput() 
    {
        if (!dashing && !carryDashMomentum)
        {
            Debug.Log("Can move");
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
                //if (moveDragTime == 1.0f) // make public velocity variables, combine velocities // movingLastFrame
                //{
                //    carryMomentum = true;
                //}
                //else
                //{
                //    carryMomentum = false;
                //}

                moveDirection = moveOrientation.forward * lastMoveAction.y + moveOrientation.right * lastMoveAction.x;

                walkCarryOverVelocity = rb.linearVelocity;
                walkCarryOverVelocity.x = moveDirection.x * speed;
                walkCarryOverVelocity.z = moveDirection.z * speed;

                if (isGrounded && wasGrounded)
                {
                    moveDragTime -= forwardDragFactor * decelMoveGroundFactor * Time.deltaTime;
                    moveDragTime = Mathf.Max(0.0f, moveDragTime);
                }

                else
                {
                    moveDragTime -= forwardDragFactor * Time.deltaTime;
                    moveDragTime = Mathf.Max(0.0f, moveDragTime);
                }

                walkCarryOverVelocity.x = Mathf.Lerp(0.0f, walkVelocity.x, moveDragTime);
                walkCarryOverVelocity.z = Mathf.Lerp(0.0f, walkVelocity.z, moveDragTime);

                rb.linearVelocity = walkCarryOverVelocity;

                Debug.Log("Drag movement");
            }

            //walkVelocity.x = Mathf.Lerp(0.0f, walkVelocity.x, moveDragTime);
            //walkVelocity.z = Mathf.Lerp(0.0f, walkVelocity.z, moveDragTime);

            //rb.linearVelocity = walkVelocity;

            Debug.Log("Forward drag time " + moveDragTime);

            Debug.Log("Move Direction: " + moveAction.y);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveVelocityAtJump = rb.linearVelocity;
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
        gravityFactor = 0.0f;

        Debug.Log("Last hor move (jump d)");
        Debug.Log("Jump air drag time (jump d)" + moveDragTime);
        Debug.Log("Grounded (jump d)" + isGrounded);
    }

    private void HandleJumpCondition()
    {
        if (isGrounded && !wasGrounded) //|| isGrounded && dashing
        {
            coyoteTimeCounter = coyoteTime;

            hasJumped = false;
            jumpCancelDash = false;
            jumpedOffGround = false;
            //Debug.Log("Was not grounded last frame and is grounded (jump condition)");
        }
        else if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
            coyoteTimeCounter = Mathf.Max(coyoteTimeCounter, min);
            jumpCancelDash = false;
            //Debug.Log("Not grounded (jump condition)");
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

            Debug.Log("Can jump (jump condition");

            if (isGrounded)
            {
                jumpedOffGround = true;
            }
        }

        Debug.Log("Has jumped" + hasJumped);
        Debug.Log("Was grounded " + wasGrounded);
        Debug.Log("Coyote time counter " + coyoteTimeCounter);
        Debug.Log("Jump buffer counter " + jumpBufferCounter);

        jumpBufferCounter -= Time.deltaTime;
        jumpBufferCounter = Mathf.Max(jumpBufferCounter, min);

        wasGrounded = isGrounded;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //dashCounter++;

            if (dashAllowed) // !dashing
            {
                Debug.Log("Dash");

                dashing = true;

                GetDashDirection();

                PerformDash();

                useGravity = false;

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

        //lastHorDashForce = currentHorDashForce;

        //lastSpeedX = rb.linearVelocity.x;
        //lastSpeedZ = rb.linearVelocity.z;

        //momentumCarry = rb.linearVelocity.magnitude;

        Debug.Log(currentVertDashForce + "Current dash force");
        Debug.Log("Dashing");
        Debug.Log("Dash Velocity" + rb.linearVelocity);
        Debug.Log("Dash velocity magnitude" + rb.linearVelocity.magnitude);
        Debug.Log("Dash velocity normalized" + dashMoveDirection.normalized * currentHorDashForce);
    }

    private void HandleDashCondition()
    {
        if (dashing)
        {
            currentHorDashForce -= dashDrag * Time.deltaTime;
            currentVertDashForce -= dashDrag * Time.deltaTime;

            currentHorDashForce = Mathf.Max(currentHorDashForce, endHorizontalDashForce); 
            currentVertDashForce = Mathf.Max(currentVertDashForce, endVerticalDashForce);

            dashVelocity.x = dashMoveDirection.x * currentHorDashForce; 
            dashVelocity.y = dashMoveDirection.y * currentVertDashForce;
            dashVelocity.z = dashMoveDirection.z * currentHorDashForce;

            rb.linearVelocity = dashVelocity;

            completeDash = false;

            Debug.Log("Dash Velocity" + rb.linearVelocity);

            //dashAllowed = false;
        }

        Debug.Log("Dashing" + dashing);

        Vector3 difference = lastVelocity - rb.linearVelocity;
        lastVelocity = rb.linearVelocity;
        lastHorDashForce = currentHorDashForce;

        // Check if dashing last frame
        bool dashLast = dashing;

        // Only check next condition if player was dashing last frame
        if (dashLast)
        {
            // If player horizontal speed change has reached 0 or player has dashed off the ground and the vertical speed has reached 0 
            if (difference.x + difference.z == 0 || difference.y == 0 && !isGrounded || jumpCancelDash)
            {
                // End the dash and carry over momentum
                dashing = false; 

                completeDash = true;

                //rb.useGravity = true;

                useGravity = true;

                carryDashMomentum = true;

                momentumCarryH = currentHorDashForce;
                // momentumCarryV = currentVertDashForce; - Use if implementing vertical momentum carry

                dashMomentumDirection = orientation.forward;

                currentHorDashForce = horizontalDashForce;
                currentVertDashForce = verticalDashForce;

                //dashSmoothCounter = dashSmoothTime;

                //dashInterpolateTime = 0.0f;

                momentumCarry = rb.linearVelocity;

                if (!jumpCancelDash)
                {
                    Vector3 resetVelocity = rb.linearVelocity;
                    resetVelocity = new Vector3(momentumCarry.x, momentumCarry.y, momentumCarry.z);
                    rb.linearVelocity = resetVelocity;

                    dashInterpolateTime = 0.0f;

                    Debug.Log("Dash interploate time " + dashInterpolateTime);
                }
                else
                {
                    Vector3 resetVelocity = rb.linearVelocity;
                    resetVelocity = new Vector3(momentumCarry.x, jumpForce, momentumCarry.z);
                    rb.linearVelocity = resetVelocity;

                    dashInterpolateTime = 0.0f;
                }

                Debug.Log("Smooth momentum");
                Debug.Log("Last velocity " + dashMoveDirection.normalized * currentHorDashForce);
            }

            Debug.Log("Finish dash " + resetDash);
        }

        if (dashInterpolateTime < 1.0f && !dashing)
        {
            HandleDashMomentum();
        }

        dashAllowed = AllowedDash();

        Debug.Log("Dash allowed " + dashAllowed);

        Debug.Log("Difference " + difference);

        Debug.Log("Jump cancel " + jumpCancelDash);

        Debug.Log("Previous hor velocity - current " + difference.x + difference.z);
        Debug.Log("Previous vert velocity - current " + difference.y);
    }

    private void HandleDashMomentum()
    {
        moveDirection = moveOrientation.forward * moveAction.y + moveOrientation.right * moveAction.x;

        walkVelocity = rb.linearVelocity;
        walkVelocity.x = moveDirection.x * speed;
        walkVelocity.z = moveDirection.z * speed;

        dashCarryOverVelocity = rb.linearVelocity;
        dashCarryOverVelocity.x = Mathf.Lerp(momentumCarry.x, 0.0f, dashInterpolateTime); // + walkVelocity.x;
        //dashCarryOverVelocity.y = Mathf.Lerp(momentumCarry.y, 0.0f, dashInterpolateTime);
        dashCarryOverVelocity.z = Mathf.Lerp(momentumCarry.z, 0.0f, dashInterpolateTime); // + walkVelocity.z;

        rb.linearVelocity = dashCarryOverVelocity;

        if (!isGrounded)
        {
            dashInterpolateTime += decelDashRate * Time.deltaTime;
        }
        else
        {
            dashInterpolateTime += decelDashGroundFactor * decelDashRate * Time.deltaTime;
        }

        // If dash interpolate time has reached 1
        if (dashInterpolateTime >= 1.0f)
        {
            // End momentum carry
            carryDashMomentum = false;
            dashCarryOverVelocity = rb.linearVelocity;
            dashCarryOverVelocity.x = 0.0f;
            dashCarryOverVelocity.z = 0.0f;

            rb.linearVelocity = dashCarryOverVelocity + walkVelocity;
        }

        Debug.Log("Dash interploate time " + dashInterpolateTime);

        Debug.Log("Momentum velocity " + dashCarryOverVelocity);

        Debug.Log("Carry dash momentum " + carryDashMomentum);

        //Debug.Log("Dash smooth counter " + dashSmoothCounter);
        
    }

    private bool AllowedDash() // multiple dashes
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
        Gizmos.DrawRay(new Vector3(groundCheck.position.x, groundCheck.position.y + groundDistance, groundCheck.position.z), dashMomentumDirection);
    }

}
