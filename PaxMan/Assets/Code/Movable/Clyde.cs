using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clyde : Ghost
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
        if (!IsPaxManInsideRadius(transform.position, 8))//<---- change this
        {
            if (map.IsPaxManVisible(transform.position))
            {
                ia.fsm.SendEvent((int)Flags.onSeePaxMan);
                ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), map.PositionToTile(gameManager.GameData.paxManPosition));
                ia.pathStepIndex = -1;
                return true;
            }
        }

        return false;
    }
}
