using UnityEngine;

public class Pinky : Ghost
{
    [Space(10)]
    public uint paxManOffsetToCalcuclateChase;

    public override void Start()
    {
        base.Start();
    }
   

    public override bool FindPaxMan()
    {
        if (map.IsPaxManVisible(transform.position))
        {
            ia.fsm.SendEvent((int)Flags.onSeePaxMan);
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), GetDestinationTile(paxManOffsetToCalcuclateChase));
            ia.pathStepIndex = -1;
            return true;
        }
        return false;
    }

    private Tile GetDestinationTile(uint _maxDistance)
    {
        float tileDistance = 0.0f;
        Vector2 targetDirection = Vector2.zero;
        if (gameManager.GameData.paxManDirection.x != 0.0f)
        {
            tileDistance = map.HorizontalTileDistance * gameManager.GameData.paxManDirection.x;
            targetDirection = Vector2.right;
        }
        else if (gameManager.GameData.paxManDirection.y != 0.0f)
        {
            tileDistance = map.VerticalTileDistance * gameManager.GameData.paxManDirection.y;
            targetDirection = Vector2.up;
        }
        for (uint i = _maxDistance; i > 0; i--)
        {
            Tile n = map.PositionToTile(gameManager.GameData.paxManPosition + (targetDirection * i * tileDistance));

            if (!n.IsObstacle && n.Index != map.PositionToTile(transform.position).Index)
                return n;
        }
        return null;
    }

    
}
