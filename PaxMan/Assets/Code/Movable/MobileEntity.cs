using System.Collections;
using System.Collections.Generic;
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
    protected bool canMove = true;

    public virtual void Start()
    {
        map = Map.instance;
        gameManager = GameManager.instance;
    }

    protected void Reset(out float _currentSpeed, out float _itarations)
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

    protected void CheckWarpZone(Node _currentNode)
    {
        if (map.IsWarpZone(_currentNode))
        {
            transform.position = map.GetWarpDestination(_currentNode);
        }
    }
}
