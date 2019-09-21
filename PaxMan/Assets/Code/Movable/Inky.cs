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
        if (IsPaxManInsideRadius(transform.position, 12))//<---- Change this
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
        Vector2 middleDestination = map.PositionToTile(paxmanPosition + (targetDirection * tileDistance * 2)).Position;//<--- Change this / dos al frente de pac man

        Vector2 blinkyTilePosition = map.PositionToTile(gameManager.GameData.blinkyPosition).Position;

        Tile target = map.PositionToTile((middleDestination - blinkyTilePosition) * 2);//<--- Change this / el doble de la distancia
        target = ia.pathFinding.GetNearestValidTile(target, transform.position);
        return target;
    }
}
