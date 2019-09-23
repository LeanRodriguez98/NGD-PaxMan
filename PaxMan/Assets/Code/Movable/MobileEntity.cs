using UnityEngine;
public class MobileEntity : MonoBehaviour
{
    [System.Serializable]
    public struct MovementSettings
    {
        public float maxSpeed;
        public float speed;
        [Range(0.0f, 1.0f)] public float porcentualSpeed;
    }

    public MovementSettings movementSettings;
    protected Map map;
    protected GameManager gameManager;
    protected bool canMove;
    protected bool dead;
    protected float defaultPorcentuslSpeed;
    private SpriteRenderer spriteRenderer;

    public virtual void Start()
    {
        map = Map.instance;
        gameManager = GameManager.instance;
        defaultPorcentuslSpeed = movementSettings.porcentualSpeed;
        canMove = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected void SetMovementSettings(out float _currentSpeed, out float _itarations)
    {
        if (movementSettings.speed > movementSettings.maxSpeed)
            movementSettings.speed = movementSettings.maxSpeed;
        _itarations = 0.0f;
        _currentSpeed = movementSettings.maxSpeed - (movementSettings.speed * movementSettings.porcentualSpeed);
    }

    protected void MoveOnTile(Vector2 _startPosition, Vector2 _destinationPosition, float _iteration, float _speed)
    {
        if (_iteration > _speed)
            _iteration = _speed;
        transform.position = Vector3.Lerp(_startPosition, _destinationPosition, _iteration / _speed);
    }

    protected bool IsEqualToPosition(Vector2 _positionToComparate)
    {
        return (transform.position != (Vector3)_positionToComparate);
    }

    protected void CheckWarpZone(Tile _currentTile)
    {
        if (map.IsWarpZone(_currentTile))
        {
            transform.position = map.GetWarpDestination(_currentTile);
        }
    }

    public void PauseMovement()
    {
        canMove = false;
    }
    public void PauseMovement(float _duration)
    {
        canMove = false;
        CancelInvoke("ReasumeMovement");
        Invoke("ReasumeMovement", _duration);
    }

    public void ReasumeMovement()
    {
        canMove = true;
    }

    public void TurnOffSprite()
    {
        spriteRenderer.enabled = false;
    }
    public void TurnOffSprite(float _duration)
    {
        spriteRenderer.enabled = false;
        CancelInvoke("TurnOnSprite");
        Invoke("TurnOnSprite", _duration);
    }

    private void TurnOnSprite()
    {
        spriteRenderer.enabled = true;
    }
}
