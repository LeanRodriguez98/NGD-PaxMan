using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinky : Ghost
{
    public override void Start()
    {
        base.Start();
    }
    public override void Update()
    {
        base.Update();
    }

    public override bool FindPaxMan()
    {
        if (map.IsPaxManVisible(transform.position))
        {
            ia.fsm.SendEvent((int)Flags.onSeePaxMan);
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position),TileToGo(4));//<---- Change this
            ia.pathStepIndex = -1;
            return true;
        }
        return false;
    }

    private Node TileToGo(uint _maxDistance)
    {
        float nodeDistance = 0.0f;
        Vector2 targetDirection = Vector2.zero;
        if (gameManager.GameData.paxManDirection.x != 0.0f)
        {
            nodeDistance = map.horizontalNodeDistance * gameManager.GameData.paxManDirection.x;
            targetDirection = Vector2.right;
        }
        else if (gameManager.GameData.paxManDirection.y != 0.0f)
        {
            nodeDistance = map.verticalNodeDistance * gameManager.GameData.paxManDirection.y;
            targetDirection = Vector2.up;
        }
        for (uint i = _maxDistance; i > 0; i--)
        {
            Node n = map.PositionToNode(gameManager.GameData.paxManPosition + (targetDirection * i * nodeDistance));

            if (!n.IsObstacle && n.Index != map.PositionToNode(transform.position).Index)
                return n;
        }
        return null;
    }

    
}
