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

    [System.Serializable]
    public struct ScatterPatron
    {
        [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
        public uint[] scatterPosibleStartNodeID;
        [Range(0, 100)] public float goToScattProbability;
        [Range(0, 100)] public float leaveScattProbability;
        [HideInInspector] public uint iterations;
    }

    [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
    public uint StartPositionNodeID;
    private Map map;
    private FSM fsm;
    private PathFinding pathFinding;
    public OnHomePatron homePatron;
    public PatrolPatron patrolPatron;
    public ScatterPatron scatterPatron;
    public int pathStepIndex = 0;                          // Make private leater
    public List<Vector2> currentPath = new List<Vector2>();// Make private leater
    private bool canMove = true;

    private void Start()
    {
        map = Map.instance;
        SetFSM();
        transform.position = map.IdToNode(StartPositionNodeID).Position;
        pathFinding = new PathFinding();
        currentPath = pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(homePatron.startPositionNodeID));
        StartCoroutine(Movement());
    }

    private void SetFSM()
    {
        fsm = new FSM((int)States._count, (int)Flags._count);
        fsm.SetState((int)States.idle);
        fsm.SetRelation((int)States.idle, (int)Flags.onOpenDoor, (int)States.leaveingHome);
        fsm.SetRelation((int)States.leaveingHome, (int)Flags.onStartPatrol, (int)States.patrol);
        fsm.SetRelation((int)States.patrol, (int)Flags.onGoToScatter, (int)States.goToScatter);
        fsm.SetRelation((int)States.goToScatter, (int)Flags.onScatter, (int)States.scatter);
        fsm.SetRelation((int)States.scatter, (int)Flags.onStartPatrol, (int)States.patrol);

        fsm.SetRelation((int)States.patrol, (int)Flags.OnPanic, (int)States.panic);
        fsm.SetRelation((int)States.goToScatter, (int)Flags.OnPanic, (int)States.panic);
        fsm.SetRelation((int)States.scatter, (int)Flags.OnPanic, (int)States.panic);


        fsm.SetRelation((int)States.panic, (int)Flags.onStartPatrol, (int)States.patrol);



    }

    Vector2 auxvec;
    Vector2 currentPosition;
    Vector2 destinationPosition;
    public IEnumerator Movement()
    {
        while (canMove)
        {
            if (currentPath.Count >= 2)
            {
                currentPosition = currentPath[pathStepIndex];
                destinationPosition = currentPath[pathStepIndex + 1];
                auxvec = destinationPosition - currentPosition;
                float i = 0.0f;
                while (transform.position != (Vector3)destinationPosition)
                {
                    if (i > 10.0f)
                        i = 10.0f;
                    transform.position = Vector3.Lerp(currentPosition, destinationPosition, i / 10.0f);
                    if (fsm.GetState() != (int)States.panic)
                    {
                        i++;
                    }
                    else
                    {
                        if ((Vector2)transform.position == currentPosition)
                        {
                            UpdatePath();
                            break;
                        }
                        i--;
                    }
                    yield return null;
                }
            }

            NewTileVerifications();
            if (destinationPosition == currentPath[currentPath.Count - 1])
                UpdatePath();
            else
                pathStepIndex++;
        }
    }

    private void NewTileVerifications()
    {
        switch (fsm.GetState())
        {
            case (int)States.goToScatter:
                for (int i = 0; i < scatterPatron.scatterPosibleStartNodeID.Length; i++)
                {
                    if (map.PositionToNode(transform.position) == map.IdToNode(scatterPatron.scatterPosibleStartNodeID[i]))
                    {
                        fsm.SendEvent((int)Flags.onScatter);
                        UpdatePath();
                    }
                }
                break;


        }
    }

    private void UpdatePath()
    {
        switch (fsm.GetState())
        {
            case (int)States.idle:
                Idle();
                break;
            case (int)States.leaveingHome:
                LeavingHome();
                break;
            case (int)States.patrol:
                Patrol();
                break;
            case (int)States.goToScatter:
                GoToScatter();
                break;
            case (int)States.scatter:
                Scatter();
                break;
            case (int)States.panic:
                pathFinding.IgnoreNode(map.PositionToNode(transform.position + (Vector3)(auxvec)));
                currentPath = pathFinding.GetPath(map.PositionToNode(transform.position + (Vector3)(auxvec)), map.PositionToNode(currentPath[0]));
                pathStepIndex = -1;
                fsm.SendEvent((int)Flags.onStartPatrol);
                break;

        }
        pathStepIndex = 0;
    }

    private void Idle()
    {
        currentPath = pathFinding.GetPath(map.IdToNode(homePatron.startPositionNodeID), map.IdToNode(homePatron.endPositionNodeID));
        uint auxID = homePatron.startPositionNodeID;
        homePatron.startPositionNodeID = homePatron.endPositionNodeID;
        homePatron.endPositionNodeID = auxID;
    }

    private void LeavingHome()
    {
        currentPath = pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(272));
        fsm.SendEvent((int)Flags.onStartPatrol);
    }

    private void Patrol()
    {
        if (UnityEngine.Random.Range(0, 101) < scatterPatron.goToScattProbability)
        {
            fsm.SendEvent((int)Flags.onGoToScatter);
            UpdatePath();
        }
        else
        {
            Node destinationNode = null;
            List<Node> posibleDestinations = new List<Node>();
            bool addNode = true;
            foreach (Node node in map.GetAllNodesOfConectionsNumber(patrolPatron.countOfNodesConectionPosibleTarget))
            {
                if (node != map.PositionToNode(transform.position))
                {
                    for (int i = 0; i < patrolPatron.excludedNodesID.Length; i++)
                        if (node == map.IdToNode((uint)patrolPatron.excludedNodesID[i]))
                            addNode = false;
                    if (addNode)
                        posibleDestinations.Add(node);
                    addNode = true;
                }

            }
            //posibleDestinations.Remove(map.PositionToNode(transform.position));
            //Debug.Log("Same eliminated: " + (Vector2)transform.position + " - " + map.PositionToNode(transform.position).Position);

            destinationNode = posibleDestinations[UnityEngine.Random.Range(0, (posibleDestinations.Count))];
            LockPreviousPosition();
            currentPath = pathFinding.GetPath(map.PositionToNode(transform.position), destinationNode);
            posibleDestinations.Clear();

        }
    }

    private void GoToScatter()
    {
        LockPreviousPosition();
        List<Node> scatterPosibleStartNodes = new List<Node>();
        for (int i = 0; i < scatterPatron.scatterPosibleStartNodeID.Length; i++)
            scatterPosibleStartNodes.Add(map.IdToNode(scatterPatron.scatterPosibleStartNodeID[i]));
        Node destinationNode = pathFinding.GetNearestNode(map.PositionToNode(transform.position), scatterPosibleStartNodes);
        scatterPatron.iterations = 0;
        fsm.SendEvent((int)Flags.onScatter);
        if (destinationNode != map.PositionToNode(transform.position))
            currentPath = pathFinding.GetPath(map.PositionToNode(transform.position), destinationNode);
        else
            UpdatePath();
    }

    private void Scatter()
    {
        LockPreviousPosition();
        int targetIndex = 0;
        Node currentNode = map.PositionToNode(transform.position);
        for (int i = 0; i < scatterPatron.scatterPosibleStartNodeID.Length; i++)
            if (currentNode == map.IdToNode(scatterPatron.scatterPosibleStartNodeID[i]))
                targetIndex = i + 1;
        if (targetIndex >= scatterPatron.scatterPosibleStartNodeID.Length)
            targetIndex = 0;
        currentPath = pathFinding.GetPath(currentNode, map.IdToNode(scatterPatron.scatterPosibleStartNodeID[targetIndex]));
        scatterPatron.iterations++;
        if (scatterPatron.iterations >= scatterPatron.scatterPosibleStartNodeID.Length)
        {
            if (UnityEngine.Random.Range(0, 101) < scatterPatron.leaveScattProbability)
            {
                fsm.SendEvent((int)Flags.onStartPatrol);
            }
            scatterPatron.iterations = 0;
        }
    }

    private void Panic()
    {

    }

    private void LockPreviousPosition()
    {
        pathFinding.IgnoreNode(map.PositionToNode(currentPath[pathStepIndex]));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fsm.SendEvent((int)Flags.onOpenDoor);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            fsm.SendEvent((int)Flags.OnPanic);

        }
    }
}
