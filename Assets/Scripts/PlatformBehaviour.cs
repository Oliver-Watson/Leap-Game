using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlatformBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    public float speed = 10f;

    private int direction = -1;

    private bool changeDirection = false;

    private bool changeDirectionLast = false;

    private float startPosition;
    [SerializeField] private float endPosition = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        startPosition = transform.position.z;
        //endPosition = transform.position.z + 10;
    }

    void Update()
    {
        PlatformMovement();
    }

    private void PlatformMovement()
    {
        //Vector3 velocity = rb.linearVelocity;
        //velocity.z = speed;
        //rb.linearVelocity = velocity;

        if (PlayerMovement.coyoteTimeCounter < PlayerMovement.coyoteTime && !PlayerMovement.jumpedOffGround)
        {

            Debug.Log("Platform move");

            Vector3 velocity = transform.position;
            velocity.z = transform.position.z + speed * Time.deltaTime;
            transform.position = velocity;

            if (transform.position.z <= startPosition || transform.position.z >= endPosition)
            {
                changeDirection = true;
            }
            else
            {
                changeDirection = false;
            }

            if (changeDirection && !changeDirectionLast)
            {
                speed *= direction;
            }

            changeDirectionLast = changeDirection;

            Debug.Log(speed + "speed");

        }
        else
        {
            Debug.Log("Platform static");
        }


    }
}
