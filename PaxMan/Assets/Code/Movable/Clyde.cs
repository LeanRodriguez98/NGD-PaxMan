using UnityEngine;

public class Clyde : Ghost
{
    [Space(10)]
    public uint targetRadius;

    public override void Start()
    {
        base.Start();
    }

    public override bool FindPaxMan()
    {
        if (!IsPaxManInsideRadius(transform.position, targetRadius))
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
