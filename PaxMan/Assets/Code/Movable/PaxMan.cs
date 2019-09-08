using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaxMan : MonoBehaviour
{
    public float speed;
    public uint lifes;
    private Vector2 movement;
    private Animator animator;
    private Map map;
    private Node currentNode;
    private Node destinationNode;
    private const string horizontalAxis = "Horizontal";
    private const string verticalAxis = "Vertical";
    private const string mapLayer = "Map";
    void Start()
    {
        animator = GetComponent<Animator>();
        movement = new Vector2(-1.0f, 0.0f);
        map = Map.instance;
        currentNode = map.PositionToNode(transform.position);
        transform.position = currentNode.Position;
        destinationNode = map.GetNextNode(currentNode, movement);
        StartCoroutine(Movement());

    }
    IEnumerator Movement()
    {
        Vector3 displacementDistance = (Vector3)(destinationNode.Position - currentNode.Position) / 10.0f;

        for (int i = 0; i < 10; i++)
        {
            transform.position += (Vector3)(destinationNode.Position - currentNode.Position) / 10.0f;
            yield return null;
        }

        currentNode = destinationNode;
        do
        {
            destinationNode = map.GetNextNode(currentNode, movement);
            yield return null;
            UpdateAnimations();
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
        {
            animator.SetBool("Idle", true);

        }
        else
        {
            animator.SetBool("Idle", false);

        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawLine(transform.position, transform.position + ((Vector3)movement / (1.0f / stopDistance)));
    }
}