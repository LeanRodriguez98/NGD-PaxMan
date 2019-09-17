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
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), TileToGo(4));
            ia.pathStepIndex = -1;
            return true;
        }
        return false;
    }

    private Node TileToGo(uint _maxDistance)
    {
        float nodeDistance = 0.0f;
        if (GameManager.instance.player.Direction.x > 0.0f)
            nodeDistance = map.horizontalNodeDistance;
        else if (GameManager.instance.player.Direction.y > 0.0f)
            nodeDistance = map.verticalNodeDistance;

        for (uint i = _maxDistance; i > 0; i--)
        {
            Node n = map.PositionToNode(GameManager.instance.player.Position + (GameManager.instance.player.Direction * i * nodeDistance));
            if (!n.IsObstacle)
                return n;
        }
        return null;
    }

    public override void Chase()
    {
        if (!FindPaxMan())
        {
            ia.fsm.SendEvent((int)Flags.onStartPatrol);
            UpdatePath();
        }
    }
}
