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

    private float platformPositionx;
    private float platformPositiony;
    private float platformPositionz;
    private Vector3 platformPosition;

    [SerializeField] private bool axisX = false;
    [SerializeField] private bool axisY = false;
    [SerializeField] private bool axisZ = false;

    [SerializeField] private bool right = false;
    [SerializeField] private bool up = false;
    [SerializeField] private bool forwards = false;

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
        // Determine if platform can move or is fixed to the start position
        // GetPlatformPosition();

        // If the player has fallen of the platform without jumping 
        if (PlayerMovement.coyoteTimeCounter < PlayerMovement.coyoteTime && !PlayerMovement.jumpedOffGround)
        {
            //GetPlatformVelocity();

            Debug.Log("Platform move");

            HandleDirection();
        }
        else
        {
            Debug.Log("Platform static");
        }
    }

    //private void GetPlatformPosition()
    //{
    //    if (axisX)
    //    {
    //        platformPositionx = transform.position.x;
    //    }
    //    else
    //    {
    //        platformPositionx = startPosition.x;
    //    }

    //    if (axisY)
    //    {
    //        platformPositiony = transform.position.y;
    //    }
    //    else
    //    {
    //        platformPositiony = startPosition.y;
    //    }

    //    if (axisZ)
    //    {
    //        platformPositionz = transform.position.z;
    //    }
    //    else
    //    {
    //        platformPositionz = startPosition.z;
    //    }

    //    // Get final platform position where vectors xyz can either be moved or are fixed to the start position 
    //    platformPosition = new Vector3(platformPositionx, platformPositiony, platformPositionz);
    //}

    private void GetPlatformVelocity()
    {
        platformVelocity = transform.position;

        if (axisX)
        {
            if (right)
            {
                platformVelocity.x += speed * Time.deltaTime;
            }
            else
            {
                platformVelocity.x -= speed * Time.deltaTime;
            }
        }

        if (axisY)
        {
            if (up)
            {
                platformVelocity.y += speed * Time.deltaTime;
            }
            else
            {
                platformVelocity.y -= speed * Time.deltaTime;
            }
        }

        if (axisZ)
        {
            if (forwards)
            {
                platformVelocity.z += speed * Time.deltaTime;
            }
            else
            {
                platformVelocity.z -= speed * Time.deltaTime;
            }
        }

        transform.position = platformVelocity;
    }

    private void MovePlatform(bool inPositiveDirection)
    {
        platformVelocity = transform.position;

        if (inPositiveDirection)
        {
            platformVelocity.x += speed * Time.deltaTime;
            platformVelocity.z += speed * Time.deltaTime;
            platformVelocity.y += speed * Time.deltaTime;
        }
        else
        {
            platformVelocity.x -= speed * Time.deltaTime;
            platformVelocity.z -= speed * Time.deltaTime;
            platformVelocity.y -= speed * Time.deltaTime;
        }

        transform.position = platformVelocity;
    }

    private void HandleDirection()
    {
        for (int i = 0; i <= 2; i++)
        {
            float[] position = { platformPosition.x, platformPosition.y, platformPosition.z };
            float[] startPos = { startPosition.x, startPosition.y, startPosition.z };
            float[] endPosition = { endPositionX, endPositionY, endPositionZ };
            bool[] moveThisAxis = { axisX, axisY, axisZ };

            if (endPosition[i] >= startPos[i])
            {
                if (position[i] <= startPos[i] && moveThisAxis[i] || position[i] >= endPosition[i] && moveThisAxis[i])
                {
                    changeDirection = true;
                }
                else
                {
                    changeDirection = false;
                }

                MovePlatform(!inPositiveDirection);
            }
            else if (endPosition[i] <= startPos[i])
            {
                if (position[i] >= startPos[i] && moveThisAxis[i] || position[i] <= endPosition[i] && moveThisAxis[i])
                {
                    changeDirection = true;
                }
                else
                {
                    changeDirection = false;
                }

                MovePlatform(inPositiveDirection);
            }

            if (changeDirection && !changeDirectionLast)
            {
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
