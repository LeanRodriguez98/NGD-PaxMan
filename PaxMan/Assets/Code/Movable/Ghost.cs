﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MobileEntity
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
        onPanic = 7,
        onDead = 8,
        _count
    }

    [System.Serializable]
    public struct OnHomePatron
    {
        [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
        public uint startPositionNodeID;
        [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
        public uint endPositionNodeID;
        [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
        public uint leavingPositionNodeID;

        public uint dotsNecesaryToLeave;
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
        [HideInInspector] public bool isScared;
    }

    [System.Serializable]
    public struct Sprites
    {
        [HideInInspector] public SpriteRenderer spriteRenderer;
        public Sprite defaultSprite;
        public Sprite scaredSprite;
        public Sprite deadSprite;
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
    protected IA ia;
    public Sprites sprites;
    public OnHomePatron homePatron;
    public PatrolPatron patrolPatron;
    public ScatterPatron scatterPatron;
    public ScaredPatron scaredPatron;

    private const string paxManTag = "Player";

    public override void Start()
    {
        base.Start();
        ia = new IA((int)States._count, (int)Flags._count);
        SetFSMRelations();
        StartIA();
    }

    public void StartIA()
    {
        transform.position = map.IdToNode(StartPositionNodeID).Position;
        ia.pathStepIndex = 0;
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(homePatron.startPositionNodeID));
        sprites.spriteRenderer = GetComponent<SpriteRenderer>();
        sprites.spriteRenderer.sprite = sprites.defaultSprite;
        ia.fsm.SetState((int)States.idle);
        dead = false;
        StartCoroutine(Movement());
    }



    private void SetFSMRelations()
    {

        ia.fsm.SetRelation((int)States.idle, (int)Flags.onOpenDoor, (int)States.leaveingHome);
        ia.fsm.SetRelation((int)States.leaveingHome, (int)Flags.onStartPatrol, (int)States.patrol);
        ia.fsm.SetRelation((int)States.patrol, (int)Flags.onGoToScatter, (int)States.goToScatter);
        ia.fsm.SetRelation((int)States.goToScatter, (int)Flags.onScatter, (int)States.scatter);
        ia.fsm.SetRelation((int)States.scatter, (int)Flags.onStartPatrol, (int)States.patrol);

        ia.fsm.SetRelation((int)States.patrol, (int)Flags.onPanic, (int)States.panic);
        ia.fsm.SetRelation((int)States.goToScatter, (int)Flags.onPanic, (int)States.panic);
        ia.fsm.SetRelation((int)States.scatter, (int)Flags.onPanic, (int)States.panic);
        ia.fsm.SetRelation((int)States.chase, (int)Flags.onPanic, (int)States.panic);

        ia.fsm.SetRelation((int)States.panic, (int)Flags.onStartPatrol, (int)States.patrol);
        ia.fsm.SetRelation((int)States.patrol, (int)Flags.onSeePaxMan, (int)States.chase);
        ia.fsm.SetRelation((int)States.chase, (int)Flags.onStartPatrol, (int)States.patrol);

        ia.fsm.SetRelation((int)States.panic, (int)Flags.onDead, (int)States.goToHome);
        ia.fsm.SetRelation((int)States.patrol, (int)Flags.onDead, (int)States.goToHome);
        ia.fsm.SetRelation((int)States.goToScatter, (int)Flags.onDead, (int)States.goToHome);
        ia.fsm.SetRelation((int)States.scatter, (int)Flags.onDead, (int)States.goToHome);

        ia.fsm.SetRelation((int)States.goToHome, (int)Flags.onIdle, (int)States.idle);
    }

    public IEnumerator Movement()
    {
        float iterations;
        float currentSpeed;
        while (canMove)
        {
            if (ia.currentPath.Count >= 2)
            {
                ia.currentPosition = ia.currentPath[ia.pathStepIndex];
                ia.destinationPosition = ia.currentPath[ia.pathStepIndex + 1];
                Reset(out currentSpeed, out iterations);
                while (IsEqualToPosition(ia.destinationPosition))
                {
                    MoveOnTile(ia.currentPosition, ia.destinationPosition, iterations, currentSpeed);
                    if (ia.fsm.GetState() != (int)States.panic)
                        iterations++;
                    else
                    {
                        if ((Vector2)transform.position == ia.currentPosition)
                        {
                            UpdatePath();
                            break;
                        }
                        iterations--;
                    }
                    yield return new WaitForFixedUpdate();
                }

            }
            NewTileVerifications();

            try // <----- remove this
            {
                if (ia.currentPath.Count - 1 < 0)
                {
                    throw new System.Exception();
                }
                if (ia.destinationPosition == ia.currentPath[ia.currentPath.Count - 1])
                    UpdatePath();
                else
                    ia.pathStepIndex++;
            }
            catch (System.Exception)
            {
                Debug.Log(ia.currentPath.Count);
                throw;
            }

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
            case (int)States.patrol:
                if (!scaredPatron.isScared)
                {
                    FindPaxMan();
                }
                break;
            case (int)States.goToHome:
                if (map.PositionToNode(transform.position).Index == homePatron.startPositionNodeID)
                {
                    ia.fsm.SendEvent((int)Flags.onIdle);
                    ia.pathStepIndex = 0;
                    ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(homePatron.startPositionNodeID));
                    sprites.spriteRenderer.sprite = sprites.defaultSprite;
                }
                break;
        }
    }

    public virtual bool FindPaxMan()
    {
        return false;
    }

    public void Chase()
    {
        if (!FindPaxMan())
        {
            ia.fsm.SendEvent((int)Flags.onStartPatrol);
            UpdatePath();
        }
        ia.pathStepIndex = 0;
    }

    protected void UpdatePath()
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
            case (int)States.chase:
                Chase();
                break;
        }
    }

    private void Idle()
    {
        ia.currentPath = ia.pathFinding.GetPath(map.IdToNode(homePatron.startPositionNodeID), map.IdToNode(homePatron.endPositionNodeID));
        uint auxID = homePatron.startPositionNodeID;
        homePatron.startPositionNodeID = homePatron.endPositionNodeID;
        homePatron.endPositionNodeID = auxID;
        ia.pathStepIndex = 0;

    }

    private void LeavingHome()
    {
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(homePatron.leavingPositionNodeID));
        ia.fsm.SendEvent((int)Flags.onStartPatrol);
        ia.pathStepIndex = 0;

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
                if (node.Position != map.PositionToNode(transform.position).Position)
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
        ia.pathStepIndex = 0;

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
        if (destinationNode.Position != map.PositionToNode(transform.position).Position)
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), destinationNode);
        else
            UpdatePath();

        ia.pathStepIndex = 0;

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
                ia.fsm.SendEvent((int)Flags.onStartPatrol);
            scatterPatron.iterations = 0;
        }

        ia.pathStepIndex = 0;

    }

    private void Panic()
    {
        ia.pathFinding.IgnoreNode(map.PositionToNode(transform.position + (Vector3)(ia.destinationPosition - ia.currentPosition)));
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position + (Vector3)(ia.destinationPosition - ia.currentPosition)), map.PositionToNode(ia.currentPath[0]));
        if (ia.currentPath.Count == 2)
        {
            ;
        }
        ia.pathStepIndex = -1;
        sprites.spriteRenderer.sprite = sprites.scaredSprite;
        CancelInvoke("SetDefaultSprite");
        Invoke("SetDefaultSprite", scaredPatron.scaredTime);
        scaredPatron.isScared = true;
        ia.fsm.SendEvent((int)Flags.onStartPatrol);
    }

    public void SetDefaultSprite()
    {
        if (sprites.spriteRenderer.sprite == sprites.scaredSprite)
        {
            sprites.spriteRenderer.sprite = sprites.defaultSprite;
        }
        scaredPatron.isScared = false;
    }

    private void LockPreviousPosition()
    {
        ia.pathFinding.IgnoreNode(map.PositionToNode(ia.currentPath[ia.pathStepIndex]));
    }

    protected bool IsPaxManInsideRadius(Vector2 center, uint radius)
    {
        uint distanceToPaxMan = ia.pathFinding.ManhattanDistance(map.PositionToNode(center).Position, map.PositionToNode(gameManager.GameData.paxManPosition).Position);
        if (distanceToPaxMan <= radius)
            return true;
        return false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!dead && scaredPatron.isScared && collision.gameObject.CompareTag(paxManTag))
        {
            BoxCollider2D collider2D = collision.gameObject.GetComponent<BoxCollider2D>();
            if (collider2D.bounds.Contains(transform.position))
            {
                ia.fsm.SendEvent((int)Flags.onDead);
                dead = true;
                GoToHome();
                gameManager.OnGhostIsEaten(transform.position);
            }
        }
    }

    private void GoToHome()
    {
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToNode(transform.position), map.IdToNode(homePatron.startPositionNodeID));
        ia.currentPath.Insert(0, (Vector2)transform.position);
        ia.pathStepIndex = 0;
        sprites.spriteRenderer.sprite = sprites.deadSprite;
        scaredPatron.isScared = false;

    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ia.fsm.SendEvent((int)Flags.onOpenDoor);

        if (Input.GetKeyDown(KeyCode.M))
            if (!scaredPatron.isScared)
                ia.fsm.SendEvent((int)Flags.onPanic);
    }

    public void SetPanic()
    {
        if (!scaredPatron.isScared)
        {
            ia.fsm.SendEvent((int)Flags.onPanic);
        }
        else
        {
            CancelInvoke("SetDefaultSprite");
            Invoke("SetDefaultSprite", scaredPatron.scaredTime);
        }
    }

    public void LeaveHose()
    {
        ia.fsm.SendEvent((int)Flags.onOpenDoor);
    }
}
