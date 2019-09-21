using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inky : Ghost
{

    public uint targetRadius;
    public uint paxManOffsetToCalcuclateChase;
    public int distanceMuliplier;
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
        if (IsPaxManInsideRadius(transform.position, targetRadius))
        {
            ia.fsm.SendEvent((int)Flags.onSeePaxMan);
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), GetDestinationTile());
            ia.pathStepIndex = -1;
            return true;
        }
        return false;
    }

    private Tile GetDestinationTile()
    {
        float tileDistance = 0.0f;
        Vector2 targetDirection = Vector2.zero;
        if (gameManager.GameData.paxManDirection.x != 0.0f)
        {
            tileDistance = map.horizontalTileDistance * gameManager.GameData.paxManDirection.x;
            targetDirection = Vector2.right;
        }
        else if (gameManager.GameData.paxManDirection.y != 0.0f)
        {
            tileDistance = map.verticalTileDistance * gameManager.GameData.paxManDirection.y;
            targetDirection = Vector2.up;
        }

        Vector2 paxmanPosition = gameManager.GameData.paxManPosition;
        Vector2 middleDestination = map.PositionToTile(paxmanPosition + (targetDirection * tileDistance * paxManOffsetToCalcuclateChase)).Position;

        Vector2 blinkyTilePosition = map.PositionToTile(gameManager.GameData.blinkyPosition).Position;

        Tile target = map.PositionToTile((middleDestination - blinkyTilePosition) * distanceMuliplier);
        target = ia.pathFinding.GetNearestValidTile(target, transform.position);
        return target;
    }
}
