using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaxMan : MobileEntity
{
    public uint lifes;
    public uint startNodeId;
    private Vector2 movement;
    private Vector2 previousMovement;
    private Animator animator;
    private Node currentNode;
    private Node destinationNode;
    private bool powered;
    private const string animationHorizontalTriggerName = "Horizontal";
    private const string animationVerticalTriggerName = "Vertical";
    private const string animationIdleTriggerName = "Idle";
    private const string animationDeadTriggerName = "Dead";
    private const string horizontalAxis = "Horizontal";
    private const string verticalAxis = "Vertical";
    private const string ghostTag = "Ghost";
    public Vector2 Direction
    {
        get { return movement.normalized; }
    }

    public Vector2 Position
    {
        get { return (Vector2)transform.position; }
    }

    public override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        InitMovement();
    }

    public void InitMovement()
    {
        movement = Vector2.left;
        previousMovement = movement;
        UpdateAnimations();
        currentNode = map.IdToNode(startNodeId);
        transform.position = currentNode.Position;
        destinationNode = map.GetNextNode(currentNode, movement);
        dead = false;
        powered = false;
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
                yield return new WaitForFixedUpdate();
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
                if (destinationNode == null)
                    yield return new WaitForFixedUpdate();
            } while (destinationNode == null);

            CheckWarpZone(currentNode);
        }
    }


    void Update()
    {
        InputMovement();
        if (Input.GetKeyDown(KeyCode.M))
        {
            EnablePower();
        }
    }

    public void EnablePower()
    {
        powered = true;
        CancelInvoke("DesablePower");
        Invoke("DesablePower", 10.0f);
    }

    public void DesablePower()
    {
        powered = false;
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
        animator.SetFloat(animationHorizontalTriggerName, movement.x);
        animator.SetFloat(animationVerticalTriggerName, movement.y);

        if (destinationNode == null)
            animator.SetBool(animationIdleTriggerName, true);
        else
            animator.SetBool(animationIdleTriggerName, false);
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!powered && !dead && collision.gameObject.CompareTag(ghostTag))
        {
            BoxCollider2D collider2D = collision.gameObject.GetComponent<BoxCollider2D>();
            if (collider2D.bounds.Contains(transform.position))
            {
                gameManager.StopAllGameCorrutines();
                animator.SetTrigger(animationDeadTriggerName);
                dead = true;
            }
        }
    }

    // This function are be called by a animation event.
    // AE = Animation Event
    public void AE_Dead()
    {
        lifes--;
        gameManager.OnDeadPaxMan();
        animator.SetTrigger("Restart");
    }
}