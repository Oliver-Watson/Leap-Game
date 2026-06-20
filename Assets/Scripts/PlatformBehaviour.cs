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
    [Header("Platform Move Distance")]
    [SerializeField] private float moveDistanceX;
    [SerializeField] private float moveDistanceY;
    [SerializeField] private float moveDistanceZ;

    private Rigidbody rb;

    public float speed = 10f;

    private int direction = -1;

    private bool changeDirection = false;

    private bool changeDirectionLast = false;

    private Vector3 startPosition;
    private Vector3 moveAmount;
    private Vector3 endDistance;

    void Start()
    {
        // rb = GetComponent<Rigidbody>(); - used if manipulating position using rb.linearVelocity

        /// <summary>
        /// Initialise the start position 
        /// Vector move distance is initialised from the starting position of the platform
        /// </summary>
        /// 
        startPosition = transform.position;

        moveAmount = (transform.right * moveDistanceX) + (transform.up * moveDistanceY) + (transform.forward * moveDistanceZ);
    }

    void Update()
    {
        PlatformMovement();
    }

    private void PlatformMovement()
    {
        // If the player has fallen off the platform without jumping, the platform moves 
        if (PlayerMovement.coyoteTimeCounter < PlayerMovement.coyoteTime && !PlayerMovement.jumpedOffGround)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        /// <summary>
        /// End position is calculated as the start position plus the provided move amount
        /// Total distance is calculated as the end distance minus start position
        /// The platform moves back and forth between the two vector points 
        /// </summary>

        Vector3 targetPoint = startPosition + moveAmount;

        // Get vector distance to get a normalised direction
        endDistance = targetPoint - startPosition;

        Vector3 platVelocity = transform.position;

        platVelocity.x = endDistance.normalized.x * speed * Time.deltaTime;
        platVelocity.y = endDistance.normalized.y * speed * Time.deltaTime;
        platVelocity.z = endDistance.normalized.z * speed * Time.deltaTime;

        transform.position += platVelocity;

        // Vector distance of platform position to start and end respectively
        Vector3 platPosFromStart = transform.position - startPosition;
        Vector3 platPosFromEnd = transform.position - targetPoint;

        if (platPosFromStart.magnitude >= endDistance.magnitude || platPosFromEnd.magnitude >= endDistance.magnitude)
        {
            changeDirection = true;
        }

        else
        {
            changeDirection = false;
        }

        // Prevent the platform getting stuck changing direction 
        if (changeDirection && !changeDirectionLast)
        {
            // Platform moves in opposite direction
            speed *= direction;
        }

        changeDirectionLast = changeDirection;
    }
}
