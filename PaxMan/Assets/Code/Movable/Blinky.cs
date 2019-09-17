using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinky : Ghost
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
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), map.PositionToNode(GameManager.instance.player.Position));
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
}
