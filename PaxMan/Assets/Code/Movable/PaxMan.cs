using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaxMan : MonoBehaviour
{
    public float maxSpeed;
    public float speed;
    [Range(0.0f, 1.0f)] public float porcentualSpeed;
    public uint lifes;
    private Vector2 movement;
    private Vector2 previousMovement;
    private Animator animator;
    private Map map;
    private Node currentNode;
    private Node destinationNode;
    private const string horizontalAxis = "Horizontal";
    private const string verticalAxis = "Vertical";
    void Start()
    {
        animator = GetComponent<Animator>();
        movement = new Vector2(-1.0f, 0.0f);
        previousMovement = movement;
        UpdateAnimations();
        map = Map.instance;
        currentNode = map.PositionToNode(transform.position);
        transform.position = currentNode.Position;
        destinationNode = map.GetNextNode(currentNode, movement);
        StartCoroutine(Movement());

    }
    IEnumerator Movement()
    {
        if (speed > maxSpeed)
            speed = maxSpeed;
        float i = 0.0f;
        float currentSpeed = maxSpeed - (speed * porcentualSpeed);
        while (transform.position != (Vector3)destinationNode.Position)
        {
            if (i > currentSpeed)
                i = currentSpeed;
            transform.position = Vector3.Lerp(currentNode.Position, destinationNode.Position, i / currentSpeed);
            i++;
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
        
        StartCoroutine(Movement());
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
            movement.y = verticalMovement;
            movement.x = 0;
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