using System;
using UnityEngine;

public class PaxMan : MonoBehaviour
{
    public float speed;
    public uint lifes;
    [Range(0.1f, 1.0f)] public float stopDistance;
    private Vector2 movement;
    private Rigidbody2D rb2D;
    private Animator animator;
    private const string horizontalAxis = "Horizontal"; 
    private const string verticalAxis = "Vertical";
    private const string mapLayer = "Map";
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        movement = new Vector2(-1.0f, 0.0f);
    }

    void Update()
    {
        Movement();
    }

    void FixedUpdate()
    {
        rb2D.MovePosition(rb2D.position + movement * speed * Time.fixedDeltaTime);
        UpdateAnimations();

    }

    private void Movement()
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
        RaycastHit2D raycastHit2D;
        raycastHit2D = Physics2D.Raycast(transform.position, movement, stopDistance, 1 << LayerMask.NameToLayer(mapLayer));

        if (raycastHit2D.collider == null)
        {
            animator.SetBool("Idle", false);
        }
        else if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer(mapLayer))
        {
            animator.SetBool("Idle", true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + ((Vector3)movement / (1.0f / stopDistance)));
    }
}