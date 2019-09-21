public class Blinky : Ghost
{
    public override void Start()
    {
        base.Start();
    }

    public override bool FindPaxMan()
    {
        if (map.IsPaxManVisible(transform.position))
        {
            ia.fsm.SendEvent((int)Flags.onSeePaxMan);
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), map.PositionToTile(gameManager.GameData.paxManPosition));
            ia.pathStepIndex = -1;
            return true;
        }
        return false;
    }

    
}
