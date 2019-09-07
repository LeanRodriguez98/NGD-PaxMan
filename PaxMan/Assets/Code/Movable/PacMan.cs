using System;
using UnityEngine;

public class PacMan : MonoBehaviour
{
    public float speed;
    public uint lifes;

    private Vector2 movement;
    private Rigidbody2D rb2D;
    private const string horizontalAxis = "Horizontal"; 
    private const string verticalAxis = "Verical";
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Movement();
    }

    void FixedUpdate()
    {
        rb2D.MovePosition(rb2D.position + movement * speed * Time.fixedDeltaTime);
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
}