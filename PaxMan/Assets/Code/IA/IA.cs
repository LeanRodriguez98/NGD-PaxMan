using System.Collections.Generic;
using UnityEngine;

public class IA
{
    public IA(int _states, int _flags)
    {
        fsm = new FSM(_states, _flags);
        pathFinding = new PathFinding();
        pathStepIndex = 0;
        currentPath = new List<Vector2>();
    }
    public FSM fsm;
    public PathFinding pathFinding;
    public int pathStepIndex;
    public Vector2 currentPosition;
    public Vector2 destinationPosition;
    public List<Vector2> currentPath;
}
