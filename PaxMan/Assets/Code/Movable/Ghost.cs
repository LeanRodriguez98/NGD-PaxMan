using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public enum States
    {
        idle = 0,
        leaveingHome = 1,
        patrol = 2,
        goToScatter = 3,
        scatter = 4,
        chase = 5,
        panic = 6,
        goToHome = 7,
        _count
    }

    public enum Flags
    {
        onStartGame = 0,
        onIdle = 1,
        onOpenDoor = 2,
        onStartPatrol = 3,
        onGoToScatter = 4,
        onScatter = 5,
        onSeePaxMan = 6,
        OnPanic = 7,
        _count
    }

    [System.Serializable]
    public struct OnHomePatron
    {
        [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
        public uint startPositionNodeID;
        [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
        public uint endPositionNodeID;
    }
    [System.Serializable]
    public struct PatrolPatron
    {
        public int[] countOfNodesConectionPosibleTarget;
        [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
        public int[] excludedNodesID;
    }

    [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
    public uint StartPositionnodeID;
    private Map map;
    private FSM fsm;
    private PathFinding pathFinding;
    public OnHomePatron homePatron;
    public PatrolPatron patrolPatron;
    public List<Vector2> currentPath = new List<Vector2>();// Make private leater
    public int pathStepIndex = 0;                          // Make private leater
    private bool canMove = true;
    private void Start()
    {
        map = Map.instance;
        fsm = new FSM((int)States._count, (int)Flags._count);
        fsm.SetState((int)States.idle);
        fsm.SetRelation((int)States.idle, (int)Flags.onOpenDoor, (int)States.leaveingHome);
        fsm.SetRelation((int)States.leaveingHome, (int)Flags.onStartPatrol, (int)States.patrol);
        transform.position = map.IdToNode(StartPositionnodeID).Position;
        pathFinding = new PathFinding();
        currentPath = pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(homePatron.startPositionNodeID));
        StartCoroutine(Movement());
    }

    public IEnumerator Movement()
    {
        while (canMove)
        {
            Vector2 currentPosition = currentPath[pathStepIndex];
            Vector2 destinationPosition = currentPath[pathStepIndex + 1];
            float i = 0.0f;
            while (transform.position != (Vector3)destinationPosition)
            {
                if (i > 10.0f)
                    i = 10.0f;
                transform.position = Vector3.Lerp(currentPosition, destinationPosition, i / 10.0f);
                i++;
                yield return null;
            }
            if (destinationPosition == currentPath[currentPath.Count - 1])
                UpdatePath();
            else
                pathStepIndex++;
        }

    }

    private void UpdatePath()
    {
        switch (fsm.GetState())
        {
            case (int)States.idle:
                currentPath = pathFinding.GetPath(map.IdToNode(homePatron.startPositionNodeID), map.IdToNode(homePatron.endPositionNodeID));
                uint auxID = homePatron.startPositionNodeID;
                homePatron.startPositionNodeID = homePatron.endPositionNodeID;
                homePatron.endPositionNodeID = auxID;
                break;
            case (int)States.leaveingHome:
                currentPath = pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(272));
                fsm.SendEvent((int)Flags.onStartPatrol);
                break;
            case (int)States.patrol:
                Node destinationNode = null;
                List<Node> posibleDestinations = new List<Node>();
                bool addNode = true;
                foreach (Node node in map.GetAllNodesOfConectionsNumber(patrolPatron.countOfNodesConectionPosibleTarget))
                {
                    if (node != map.PositionToNode(transform.position))
                    {

                        for (int i = 0; i < patrolPatron.excludedNodesID.Length; i++)
                        {
                            if (node == map.IdToNode((uint)patrolPatron.excludedNodesID[i]))
                            {
                                addNode = false;
                            }
                        }
                        if (addNode)
                        {
                            posibleDestinations.Add(node);
                        }
                        addNode = true;
                    }
                }
                destinationNode = posibleDestinations[UnityEngine.Random.Range(0, (posibleDestinations.Count))];
                pathFinding.IgnoreNode(map.PositionToNode(currentPath[pathStepIndex])); //ignore the previous node because the ghosts can't go backwards 
                currentPath = pathFinding.GetPath(map.PositionToNode(transform.position), destinationNode);
                pathStepIndex = 0;
                break;
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fsm.SendEvent((int)Flags.onOpenDoor);
        }
    }
}
