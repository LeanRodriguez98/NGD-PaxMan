using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaxMan : MobileEntity
{
    public uint lifes;
    private Vector2 movement;
    private Vector2 previousMovement;
    private Animator animator;
    private Node currentNode;
    private Node destinationNode;
    private const string horizontalAxis = "Horizontal";
    private const string verticalAxis = "Vertical";

    public override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        movement = Vector2.left;
        previousMovement = movement;
        UpdateAnimations();
        currentNode = map.PositionToNode(transform.position);
        transform.position = currentNode.Position;
        destinationNode = map.GetNextNode(currentNode, movement);
        StartCoroutine(Movement());

    }
    private IEnumerator Movement()
    {
        float iterations;
        float currentSpeed;
        while (canMove)
        {
            Reset(out currentSpeed,out iterations);
            while (IsEqualToPosition(destinationNode.Position))
            {
                MoveOnTile(currentNode.Position, destinationNode.Position, iterations, currentSpeed);
                iterations++;
                yield return null;
            }
            currentNode = destinationNode;
            do
            {
                if (map.GetNextNode(currentNode, movement) != null)
                {
                    destinationNode = map.GetNextNode(currentNode, movement);
                    previousMovement = movement;
                }
                else
                {
                    destinationNode = map.GetNextNode(currentNode, previousMovement);
                    movement = previousMovement;
                }
                UpdateAnimations();
                yield return null;
            } while (destinationNode == null);

            CheckWarpZone(currentNode);
        }
    }


    void Update()
    {
        InputMovement();
    }

    private void InputMovement()
    {
        float horizontalMovement = Input.GetAxisRaw(horizontalAxis);
        float verticalMovement = Input.GetAxisRaw(verticalAxis);
        if (horizontalMovement != 0.0f)
        {
            movement.x = horizontalMovement;
            movement.y = 0;
        }
        if (verticalMovement != 0.0f)
        {
            movement.x = 0;
            movement.y = verticalMovement;
        }
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);

        if (destinationNode == null)
            animator.SetBool("Idle", true);
        else
            animator.SetBool("Idle", false);
    }

}