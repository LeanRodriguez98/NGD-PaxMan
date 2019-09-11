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

    [System.Serializable]
    public struct ScaredPatron
    {
        public float scaredTime;
    }

    [System.Serializable]
    public struct Sprites
    {
        [HideInInspector] public SpriteRenderer spriteRenderer;
        public Sprite defaultSprite;
        public Sprite scaredSprite;
        public Sprite desdSprite;
    }

    [System.Serializable]
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

    [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
    public uint StartPositionNodeID;
    private Map map;
    private IA ia;
    public Sprites sprites;
    public OnHomePatron homePatron;
    public PatrolPatron patrolPatron;
    public ScatterPatron scatterPatron;
    public ScaredPatron scaredPatron;
    private bool canMove = true;

    private void Start()
    {
        map = Map.instance;
        SetFSM();
        transform.position = map.IdToNode(StartPositionNodeID).Position;
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(homePatron.startPositionNodeID));
        sprites.spriteRenderer = GetComponent<SpriteRenderer>();
        SetDefaultSprite();
        StartCoroutine(Movement());
    }

    private void SetFSM()
    {
        ia = new IA((int)States._count, (int)Flags._count);
        ia.fsm.SetState((int)States.idle);
        ia.fsm.SetRelation((int)States.idle, (int)Flags.onOpenDoor, (int)States.leaveingHome);
        ia.fsm.SetRelation((int)States.leaveingHome, (int)Flags.onStartPatrol, (int)States.patrol);
        ia.fsm.SetRelation((int)States.patrol, (int)Flags.onGoToScatter, (int)States.goToScatter);
        ia.fsm.SetRelation((int)States.goToScatter, (int)Flags.onScatter, (int)States.scatter);
        ia.fsm.SetRelation((int)States.scatter, (int)Flags.onStartPatrol, (int)States.patrol);

        ia.fsm.SetRelation((int)States.patrol, (int)Flags.OnPanic, (int)States.panic);
        ia.fsm.SetRelation((int)States.goToScatter, (int)Flags.OnPanic, (int)States.panic);
        ia.fsm.SetRelation((int)States.scatter, (int)Flags.OnPanic, (int)States.panic);


        ia.fsm.SetRelation((int)States.panic, (int)Flags.onStartPatrol, (int)States.patrol);

    }

    public IEnumerator Movement()
    {
        while (canMove)
        {
            if (ia.currentPath.Count >= 2)
            {
                ia.currentPosition = ia.currentPath[ia.pathStepIndex];
                ia.destinationPosition = ia.currentPath[ia.pathStepIndex + 1];
                float i = 0.0f;
                while (transform.position != (Vector3)ia.destinationPosition)
                {
                    if (i > 10.0f)
                        i = 10.0f;
                    transform.position = Vector3.Lerp(ia.currentPosition, ia.destinationPosition, i / 10.0f);
                    if (ia.fsm.GetState() != (int)States.panic)
                    {
                        i++;
                    }
                    else
                    {
                        if ((Vector2)transform.position == ia.currentPosition)
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
            if (ia.destinationPosition == ia.currentPath[ia.currentPath.Count - 1])
                UpdatePath();
            else
                ia.pathStepIndex++;
        }
    }

    private void NewTileVerifications()
    {
        switch (ia.fsm.GetState())
        {
            case (int)States.goToScatter:
                for (int i = 0; i < scatterPatron.scatterPosibleStartNodeID.Length; i++)
                {
                    if (map.PositionToNode(transform.position) == map.IdToNode(scatterPatron.scatterPosibleStartNodeID[i]))
                    {
                        ia.fsm.SendEvent((int)Flags.onScatter);
                        UpdatePath();
                    }
                }
                break;
        }
    }

    private void UpdatePath()
    {
        switch (ia.fsm.GetState())
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
                Panic();
                break;

        }
        ia.pathStepIndex = 0;
    }

    private void Idle()
    {
        ia.currentPath = ia.pathFinding.GetPath(map.IdToNode(homePatron.startPositionNodeID), map.IdToNode(homePatron.endPositionNodeID));
        uint auxID = homePatron.startPositionNodeID;
        homePatron.startPositionNodeID = homePatron.endPositionNodeID;
        homePatron.endPositionNodeID = auxID;
    }

    private void LeavingHome()
    {
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(272));
        ia.fsm.SendEvent((int)Flags.onStartPatrol);
    }

    private void Patrol()
    {
        

        if (UnityEngine.Random.Range(0, 101) < scatterPatron.goToScattProbability)
        {
            ia.fsm.SendEvent((int)Flags.onGoToScatter);
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
            destinationNode = posibleDestinations[UnityEngine.Random.Range(0, (posibleDestinations.Count))];
            LockPreviousPosition();
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), destinationNode);
            posibleDestinations.Clear();
        }
    }

    private void GoToScatter()
    {
        LockPreviousPosition();
        List<Node> scatterPosibleStartNodes = new List<Node>();
        for (int i = 0; i < scatterPatron.scatterPosibleStartNodeID.Length; i++)
            scatterPosibleStartNodes.Add(map.IdToNode(scatterPatron.scatterPosibleStartNodeID[i]));
        Node destinationNode = ia.pathFinding.GetNearestNode(map.PositionToNode(transform.position), scatterPosibleStartNodes);
        scatterPatron.iterations = 0;
        ia.fsm.SendEvent((int)Flags.onScatter);
        if (destinationNode != map.PositionToNode(transform.position))
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), destinationNode);
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
        ia.currentPath = ia.pathFinding.GetPath(currentNode, map.IdToNode(scatterPatron.scatterPosibleStartNodeID[targetIndex]));
        scatterPatron.iterations++;
        if (scatterPatron.iterations >= scatterPatron.scatterPosibleStartNodeID.Length)
        {
            if (UnityEngine.Random.Range(0, 101) < scatterPatron.leaveScattProbability)
            {
                ia.fsm.SendEvent((int)Flags.onStartPatrol);
            }
            scatterPatron.iterations = 0;
        }
    }

    private void Panic()
    {
        ia.pathFinding.IgnoreNode(map.PositionToNode(transform.position + (Vector3)(ia.destinationPosition - ia.currentPosition)));
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position + (Vector3)(ia.destinationPosition - ia.currentPosition)), map.PositionToNode(ia.currentPath[0]));
        ia.pathStepIndex = -1;
        sprites.spriteRenderer.sprite = sprites.scaredSprite;
        Invoke("SetDefaultSprite",scaredPatron.scaredTime);
        ia.fsm.SendEvent((int)Flags.onStartPatrol);
    }

    public void SetDefaultSprite()
    {
        sprites.spriteRenderer.sprite = sprites.defaultSprite;
    }

    private void LockPreviousPosition()
    {
        ia.pathFinding.IgnoreNode(map.PositionToNode(ia.currentPath[ia.pathStepIndex]));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ia.fsm.SendEvent((int)Flags.onOpenDoor);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ia.fsm.SendEvent((int)Flags.OnPanic);
        }
    }
}
