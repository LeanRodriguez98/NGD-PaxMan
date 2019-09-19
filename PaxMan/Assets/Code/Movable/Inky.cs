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
        return false;
    }

    public override void Chase()
    { }

    private bool IsPaxManInsideRadius(Vector2 center, uint radius)
    {
        if (ia.pathFinding.ManhattanDistance(map.PositionToNode(center).Position, map.PositionToNode(GameManager.instance.GameData.paxManPosition).Position) <= radius )
            return true;
        return false;
    }
}
