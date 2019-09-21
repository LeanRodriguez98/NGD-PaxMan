using System.Collections;
using UnityEngine;

public class PaxMan : MobileEntity
{
    [System.Serializable]
    public struct PowerSettings
    {
        public float powerDuration;
        [Range(0.0f, 1.0f)] public float poweredPorcentualSpeed;
    }
    [Space(10)]
    public PowerSettings powerSettings;
    [Space(10)]
    public uint lifes;
    [Space(10)]
    public uint startTileId;

    private Vector2 movement;
    private Vector2 previousMovement;
    private Animator animator;
    private Tile currentTile;
    private Tile destinationTile;
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
        currentTile = map.IdToTile(startTileId);
        transform.position = currentTile.Position;
        destinationTile = map.GetNextTile(currentTile, movement);
        dead = false;
        powered = false;
        StartCoroutine(Movement());
    }

    private IEnumerator Movement()
    {
        float iterations;
        float currentSpeed;
        while (!gameManager.gameOver)
        {
            if (canMove)
            {
                SetMovementSettings(out currentSpeed, out iterations);
                while (IsEqualToPosition(destinationTile.Position))
                {
                    MoveOnTile(currentTile.Position, destinationTile.Position, iterations, currentSpeed);
                    iterations++;
                    yield return new WaitForFixedUpdate();
                }
                currentTile = destinationTile;
                do
                {
                    if (map.GetNextTile(currentTile, movement) != null)
                    {
                        destinationTile = map.GetNextTile(currentTile, movement);
                        previousMovement = movement;
                    }
                    else
                    {
                        destinationTile = map.GetNextTile(currentTile, previousMovement);
                        movement = previousMovement;
                    }
                    UpdateAnimations();
                    if (destinationTile == null)
                        yield return new WaitForFixedUpdate();
                } while (destinationTile == null);

                CheckWarpZone(currentTile); 
            }
            else
            {
                yield return new WaitForFixedUpdate();
            }
        }
    }


    void Update()
    {
        InputMovement();
    }

    public void EnablePower()
    {
        powered = true;
        CancelInvoke("DesablePower");
        Invoke("DesablePower", powerSettings.powerDuration);
        movementSettings.porcentualSpeed = powerSettings.poweredPorcentualSpeed;
    }

    public void DesablePower()
    {
        powered = false;
        movementSettings.porcentualSpeed = defaultPorcentuslSpeed;
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

        if (destinationTile == null)
            animator.SetBool(animationIdleTriggerName, true);
        else
            animator.SetBool(animationIdleTriggerName, false);
    }


    private void OnTriggerStay2D(Collider2D _collision)
    {
        if (!powered && !dead && _collision.gameObject.CompareTag(ghostTag))
        {
            BoxCollider2D collider2D = _collision.gameObject.GetComponent<BoxCollider2D>();
            if (collider2D.bounds.Contains(transform.position))
            {
                gameManager.StopAllGameCorrutines();
                animator.SetTrigger(animationDeadTriggerName);
                dead = true;
            }
        }
    }

    // This function are be called by a animation event in the dead animation of PaxMan
    // AE = Animation Event
    public void AE_Dead()
    {
        lifes--;
        gameManager.OnDeadPaxMan();
        animator.SetTrigger("Restart");
    }
}