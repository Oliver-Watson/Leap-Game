using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private InputAction moveAction;

    private InputAction jumpAction;

    private Rigidbody rb;


    private Transform groundCheck;

    private LayerMask groundMask;

    public float groundDistance = 0.2f;

    private bool isGrounded;


    private float vertVelocity = 0;

    public float gravity = -9.81f;

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Move();
        Jump();
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundDistance, 1);

        if (isGrounded)
        {
            Debug.Log("Grounded");
            gravity = 0 * Time.deltaTime;
        }

        vertVelocity += gravity * Time.deltaTime;

        transform.position = new Vector3(transform.position.x, vertVelocity , transform.position.y);

        Debug.Log(transform.position);
    }

    private void Move()
    {

    }

    private void Jump()
    {

    }

}
