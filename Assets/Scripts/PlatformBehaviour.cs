using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class PlatformBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    public float speed = 10f;

    private int direction = -1;

    private bool changeDirection = false;

    private bool changeDirectionLast = false;

    private Vector3 startPosition;

    private Vector3 platformVelocity;

    private bool inPositiveDirection = true;

    [SerializeField] private float endPositionX;
    [SerializeField] private float endPositionY;
    [SerializeField] private float endPositionZ;

    [SerializeField] private bool axisX = false;
    [SerializeField] private bool axisY = false;
    [SerializeField] private bool axisZ = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        startPosition = transform.position;

        //endPosition = transform.position.z + 10;
    }

    void Update()
    {
        // Call platform movement
        PlatformMovement();
    }

    private void PlatformMovement()
    {
        // If the player has fallen of the platform without jumping 
        if (PlayerMovement.coyoteTimeCounter < PlayerMovement.coyoteTime && !PlayerMovement.jumpedOffGround)
        {
            HandleDirection();

            Debug.Log("Platform move");
        }
        else
        {
            Debug.Log("Platform static");
        }
    }

    private void HandleDirection()
    {
        for (int i = 0; i <= 2; i++)
        {
            // Store the transform, start position, end position, and axis conditions respectively
            float[] position = { transform.position.x, transform.position.y, transform.position.z };
            float[] startPos = { startPosition.x, startPosition.y, startPosition.z };
            float[] endPosition = { endPositionX, endPositionY, endPositionZ };
            bool[] moveThisAxis = { axisX, axisY, axisZ };

            // Initialise platform velocity as the transforms position
            platformVelocity = new Vector3(position[0], position[1], position[2]);

            // Store the platform velocity to be altered in the array
            float[] storePlatformVelocity = { platformVelocity.x, platformVelocity.y, platformVelocity.z };

            // If the end position is greater than the start position
            if (endPosition[i] >= startPos[i] && moveThisAxis[i])
            {
                // If if the x, y or z of the transform is less than or equal to the x, y or z of the start position and relative is to be moved and visa versa
                if (position[i] <= startPos[i] || position[i] >= endPosition[i])
                {
                    // Platform should change direction
                    changeDirection = true;
                }
                // Else if indexed axis is to be moved but should not change direction
                else
                {
                    changeDirection = false;
                }
            }
            // Opposite conditions
            else if (endPosition[i] <= startPos[i] && moveThisAxis[i])
            {
                if (position[i] >= startPos[i] || position[i] <= endPosition[i])
                {
                    changeDirection = true;
                }
                else
                {
                    changeDirection = false;
                }
            }

            // If the axis is to be moved
            if (moveThisAxis[i])
            {
                // The platform should move on the indexed axis according to the negative or positive speed
                storePlatformVelocity[i] += speed * Time.deltaTime;
            }

            // Reassign platformVelocity to the indexed updated platform velocity 
            platformVelocity = new Vector3(storePlatformVelocity[0], storePlatformVelocity[1], storePlatformVelocity[2]);
            transform.position = platformVelocity;

            // If the platform should change direction
            if (changeDirection && !changeDirectionLast)
            {
                // Return the negative speed to move platform in the opposite direction
                speed *= direction;
                i = 2;
                
            }

            Debug.Log(speed + "speed");
            Debug.Log(moveThisAxis[i] + "Move this axis");
            Debug.Log(startPos[i] + "Start array");
            Debug.Log(position[i] + "Position array");
            Debug.Log(i + "index");
        }

        changeDirectionLast = changeDirection; 
    }
}
