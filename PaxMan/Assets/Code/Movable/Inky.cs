using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inky : Ghost
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
        if (IsPaxManInsideRadius(transform.position,12))//<---- Change this
        {
            ia.fsm.SendEvent((int)Flags.onSeePaxMan);
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), GetDestinationNode());
            ia.pathStepIndex = -1;
            return true;
        }
        return false;
    }

    public override void Chase()
    {
        if (!FindPaxMan())
        {
            ia.fsm.SendEvent((int)Flags.onStartPatrol);
            UpdatePath();
        }
    }

    private bool IsPaxManInsideRadius(Vector2 center, uint radius)
    {
        uint a = ia.pathFinding.ManhattanDistance(map.PositionToNode(center).Position, map.PositionToNode(GameManager.instance.GameData.paxManPosition).Position);
        if (a <= radius )
            return true;
        return false;
    }

    private Node GetDestinationNode()
    {
        float nodeDistance = 0.0f;
        Vector2 targetDirection = Vector2.zero;
        if (GameManager.instance.GameData.paxManDirection.x != 0.0f)
        {
            nodeDistance = map.horizontalNodeDistance * GameManager.instance.GameData.paxManDirection.x;
            targetDirection = Vector2.right;
        }
        else if (GameManager.instance.GameData.paxManDirection.y != 0.0f)
        {
            nodeDistance = map.verticalNodeDistance * GameManager.instance.GameData.paxManDirection.y;
            targetDirection = Vector2.up;
        }

        Vector2 paxmanPosition = GameManager.instance.GameData.paxManPosition;
        Vector2 middleDestination = map.PositionToNode(paxmanPosition + (targetDirection * nodeDistance * 2)).Position;//<--- Change this / dos al frente de pac man

        Vector2 blinkyNodePosition = map.PositionToNode(GameManager.instance.GameData.blinkyPosition).Position;

        Node target = map.PositionToNode((middleDestination - blinkyNodePosition) * 2);//<--- Change this / el doble de la distancia
        target = ia.pathFinding.GetNearestValidNode(target);
        //if (target.Index != map.PositionToNode(transform.position).Index)
        //{
            return target;
        //}
        //else
        //{

        //}
        //return null;
    }

}
