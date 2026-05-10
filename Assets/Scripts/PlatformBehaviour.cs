using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;

public class PlatformBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    public float speed = 10f;

    private int direction = -1;

    private bool changeDirection = false;

    private bool changeDirectionLast = false;

    private Vector3 startPosition;

    private Vector3 platformVelocity;

    [SerializeField] private float endPositionX;
    [SerializeField] private float endPositionY;
    [SerializeField] private float endPositionZ;
    private float endPosition;

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
            HandleMovement();

            Debug.Log("Platform move");
        }
        else
        {
            Debug.Log("Platform static");
        }
    }

    private void HandleMovement()
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

        // loop for xyz axis
        for (int i = 0; i <= 2; i++)
        {
            // If the end position is greater than the start position and respective axis is to be moved
            if (endPosition[i] >= startPos[i] && moveThisAxis[i])
            {
                // If if the x, y or z of the transform is less than or equal to the x, y or z of the start position and visa versa 
                if (position[i] <= startPos[i] || position[i] >= endPosition[i])
                {
                    // Platform should change direction
                    changeDirection = true;
                }
                // Else if the respective axis is to be moved but should not change direction
                else
                {
                    changeDirection = false;
                }
            }
            // Repeated logic for when the platform end position is less than the start position
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

            // Reassign platformVelocity to the updated storedPlatformVelocity array
            platformVelocity = new Vector3(storePlatformVelocity[0], storePlatformVelocity[1], storePlatformVelocity[2]);
            // Update the transforms position
            transform.position = platformVelocity;

            Debug.Log(speed + "speed");
            Debug.Log(moveThisAxis[i] + "Move this axis");
            Debug.Log(startPos[i] + "Start array");
            Debug.Log(position[i] + "Position array");
            Debug.Log(i + "index");
        }

        // If the platform should change direction
        if (changeDirection && !changeDirectionLast)
        {
            // Return the negative speed to move platform in the opposite direction
            speed *= direction;
            Debug.Log("Change direction");
        }

        changeDirectionLast = changeDirection;
    }
}
