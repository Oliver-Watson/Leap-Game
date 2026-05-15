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
using static UnityEngine.Tilemaps.Tilemap;

public class PlatformBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    public float speed = 10f;

    private int direction = -1;

    private bool changeDirection = false;

    private bool changeDirectionLast = false;

    private Vector3 startDirection;
    private Vector3 startPosition;

    private Vector3 platformVelocity;

    [SerializeField] private float moveDistanceX;
    [SerializeField] private float moveDistanceY;
    [SerializeField] private float moveDistanceZ;
    private Vector3 moveAmount;

    private Vector3 endDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initialise start position
        startPosition = transform.position;


        // Move this distance in the direction the platform is facing
        moveAmount = (transform.right * moveDistanceX) + (transform.up * moveDistanceY) + (transform.forward * moveDistanceZ);

        //endPosition = transform.position.z + 10;
    }

    void Update()
    {
        // Call platform movement
        PlatformMovement();
    }

    private void PlatformMovement()
    {
        // If the player has fallen off the platform without jumping 
        if (PlayerMovement.coyoteTimeCounter < PlayerMovement.coyoteTime && !PlayerMovement.jumpedOffGround)
        {
            // Move the platform
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
        // Target vector point
        Vector3 targetPoint = startPosition + moveAmount;

        // Vector distance from the start position to the target end position
        endDistance = targetPoint - startPosition;

        // Initialising vector platVelocity as the transforms position
        Vector3 platVelocity = transform.position;

        // Change platforms position by the speed multiplied by the normalised vector to move the platform in the direction of the start position to target end position
        platVelocity.x += endDistance.normalized.x * speed * Time.deltaTime;

        platVelocity.y += endDistance.normalized.y * speed * Time.deltaTime;

        platVelocity.z += endDistance.normalized.z * speed * Time.deltaTime;

        // Assign the platforms position back to the updated velocity
        transform.position = platVelocity;

        // Vector distance of platform position to start and end respectively
        Vector3 platPosFromStart = transform.position - startPosition;
        Vector3 platPosFromEnd = transform.position - targetPoint;

        // If the platform position from the start exceeds the distance from the start or the end
        if (platPosFromStart.magnitude >= endDistance.magnitude || platPosFromEnd.magnitude >= endDistance.magnitude)
        {
            // Change platform direction condition is true
            changeDirection = true;
            Debug.Log("Position from start" + platPosFromStart);
            Debug.Log("Position from end" + platPosFromEnd);
        }

        else
        {
            // Continue in same direction 
            changeDirection = false;
        }

        // If the change direction condition is true and was not true the last frame 
        if (changeDirection && !changeDirectionLast)
        {
            // Change platform direction
            speed *= direction;
            Debug.Log("Change direction");
        }

        // Avoid platform getting stuck on continuous change direction state
        changeDirectionLast = changeDirection;
    }
}
