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
    private Vector3 endPosition;

    private Vector3 endDistance;
    private Vector3 startDistance;

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
            // HandleMovement();
            PlatPosition();

            Debug.Log("Platform move");
        }
        else
        {
            Debug.Log("Platform static");
        }
    }

    private void PlatPosition()
    {
        endPosition = new Vector3(endPositionX, endPositionY, endPositionZ);

        endDistance = endPosition - startPosition;

        Vector3 platVelocity = transform.position;

        platVelocity.x += endDistance.normalized.x * speed * Time.deltaTime;

        platVelocity.y += endDistance.normalized.y * speed * Time.deltaTime;

        platVelocity.z += endDistance.normalized.z * speed * Time.deltaTime;

        transform.position = platVelocity;

        Vector3 platPosFromStart = transform.position - startPosition;
        Vector3 platPosFromEnd = transform.position - endPosition;

        if (platPosFromStart.magnitude >= endDistance.magnitude || platPosFromEnd.magnitude >= endDistance.magnitude)
        {
            changeDirection = true;
            Debug.Log("Position from start" + platPosFromStart);
            Debug.Log("Position from end" + platPosFromEnd);
        }

        else
        {
            changeDirection = false;
        }


        if (changeDirection && !changeDirectionLast)
        {
            speed *= direction;
            Debug.Log("Change direction");
        }

        changeDirectionLast = changeDirection;
    }
}
