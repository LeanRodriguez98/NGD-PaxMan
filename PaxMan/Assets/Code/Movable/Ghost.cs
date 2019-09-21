using System.Collections;
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
        [Tooltip("You can see the tiles ID checking the \"Show tiles ID\" toggle on the Map script")]
        public uint startPositionTileID;
        [Tooltip("You can see the tiles ID checking the \"Show tiles ID\" toggle on the Map script")]
        public uint endPositionTileID;
        [Tooltip("You can see the tiles ID checking the \"Show tiles ID\" toggle on the Map script")]
        public uint leavingPositionTileID;

        public uint dotsNecesaryToLeave;
    }
    [System.Serializable]
    public struct PatrolPatron
    {
        public int[] countOfTilesConectionPosibleTarget;
        [Tooltip("You can see the tiles ID checking the \"Show tiles ID\" toggle on the Map script")]
        public int[] excludedTilesID;
    }

    [System.Serializable]
    public struct ScatterPatron
    {
        [Tooltip("You can see the tiles ID checking the \"Show tiles ID\" toggle on the Map script")]
        public uint[] scatterPosibleStartTileID;
        [Range(0, 100)] public float goToScattProbability;
        [Range(0, 100)] public float leaveScattProbability;
        [HideInInspector] public uint iterations;
    }

    [System.Serializable]
    public struct ScaredPatron
    {
        public float scaredTime;
        [Range(0.0f,1.0f)] public float scaredPorcentualSpeed;
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

    [Tooltip("You can see the tiles ID checking the \"Show tiles ID\" toggle on the Map script")]
    public uint StartPositionTileID;
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
        transform.position = map.IdToTile(StartPositionTileID).Position;
        ia.pathStepIndex = 0;
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), map.IdToTile(homePatron.startPositionTileID));
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
                for (int i = 0; i < scatterPatron.scatterPosibleStartTileID.Length; i++)
                {
                    if (map.PositionToTile(transform.position) == map.IdToTile(scatterPatron.scatterPosibleStartTileID[i]))
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
                if (map.PositionToTile(transform.position).Index == homePatron.startPositionTileID)
                {
                    ia.fsm.SendEvent((int)Flags.onIdle);
                    ia.pathStepIndex = 0;
                    ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), map.IdToTile(homePatron.startPositionTileID));
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
        ia.currentPath = ia.pathFinding.GetPath(map.IdToTile(homePatron.startPositionTileID), map.IdToTile(homePatron.endPositionTileID));
        uint auxID = homePatron.startPositionTileID;
        homePatron.startPositionTileID = homePatron.endPositionTileID;
        homePatron.endPositionTileID = auxID;
        ia.pathStepIndex = 0;

    }

    private void LeavingHome()
    {
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), map.IdToTile(homePatron.leavingPositionTileID));
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
            Tile destinationTile = null;
            List<Tile> posibleDestinations = new List<Tile>();
            bool addTile = true;

            foreach (Tile tile in map.GetAllTilesOfConectionsNumber(patrolPatron.countOfTilesConectionPosibleTarget))
            {
                if (tile.Position != map.PositionToTile(transform.position).Position)
                {

                    for (int i = 0; i < patrolPatron.excludedTilesID.Length; i++)
                        if (tile == map.IdToTile((uint)patrolPatron.excludedTilesID[i]))
                            addTile = false;
                    if (addTile)
                        posibleDestinations.Add(tile);
                    addTile = true;
                }
            }
            destinationTile = posibleDestinations[UnityEngine.Random.Range(0, (posibleDestinations.Count))];
            LockPreviousPosition();
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), destinationTile);
            posibleDestinations.Clear();
        }
        ia.pathStepIndex = 0;

    }

    private void GoToScatter()
    {
        LockPreviousPosition();
        List<Tile> scatterPosibleStartTiles = new List<Tile>();
        for (int i = 0; i < scatterPatron.scatterPosibleStartTileID.Length; i++)
            scatterPosibleStartTiles.Add(map.IdToTile(scatterPatron.scatterPosibleStartTileID[i]));
        Tile destinationTile = ia.pathFinding.GetNearestTile(map.PositionToTile(transform.position), scatterPosibleStartTiles);
        scatterPatron.iterations = 0;
        ia.fsm.SendEvent((int)Flags.onScatter);
        if (destinationTile.Position != map.PositionToTile(transform.position).Position)
            ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), destinationTile);
        else
            UpdatePath();

        ia.pathStepIndex = 0;

    }

    private void Scatter()
    {
        LockPreviousPosition();
        int targetIndex = 0;
        Tile currentTile = map.PositionToTile(transform.position);
        for (int i = 0; i < scatterPatron.scatterPosibleStartTileID.Length; i++)
            if (currentTile == map.IdToTile(scatterPatron.scatterPosibleStartTileID[i]))
                targetIndex = i + 1;
        if (targetIndex >= scatterPatron.scatterPosibleStartTileID.Length)
            targetIndex = 0;
        ia.currentPath = ia.pathFinding.GetPath(currentTile, map.IdToTile(scatterPatron.scatterPosibleStartTileID[targetIndex]));
        scatterPatron.iterations++;
        if (scatterPatron.iterations >= scatterPatron.scatterPosibleStartTileID.Length)
        {
            if (UnityEngine.Random.Range(0, 101) < scatterPatron.leaveScattProbability)
                ia.fsm.SendEvent((int)Flags.onStartPatrol);
            scatterPatron.iterations = 0;
        }
        ia.pathStepIndex = 0;
    }

    private void Panic()
    {
        ia.pathFinding.IgnoreTile(map.PositionToTile(transform.position + (Vector3)(ia.destinationPosition - ia.currentPosition)));
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position + (Vector3)(ia.destinationPosition - ia.currentPosition)), map.PositionToTile(ia.currentPath[0]));
        ia.pathStepIndex = -1;
        sprites.spriteRenderer.sprite = sprites.scaredSprite;
        CancelInvoke("SetAsDefault");
        Invoke("SetAsDefault", scaredPatron.scaredTime);
        scaredPatron.isScared = true;
        movementSettings.porcentualSpeed = scaredPatron.scaredPorcentualSpeed;
        ia.fsm.SendEvent((int)Flags.onStartPatrol);
    }

    public void SetAsDefault()
    {
        if (sprites.spriteRenderer.sprite == sprites.scaredSprite)
        {
            sprites.spriteRenderer.sprite = sprites.defaultSprite;
        }
        scaredPatron.isScared = false;
        movementSettings.porcentualSpeed = defaultPorcentuslSpeed;
    }

    private void LockPreviousPosition()
    {
        ia.pathFinding.IgnoreTile(map.PositionToTile(ia.currentPath[ia.pathStepIndex]));
    }

    protected bool IsPaxManInsideRadius(Vector2 center, uint radius)
    {
        uint distanceToPaxMan = ia.pathFinding.ManhattanDistance(map.PositionToTile(center).Position, map.PositionToTile(gameManager.GameData.paxManPosition).Position);
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
        ia.currentPath = ia.pathFinding.GetPath(map.PositionToTile(transform.position), map.IdToTile(homePatron.startPositionTileID));
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
            CancelInvoke("SetAsDefault");
            Invoke("SetAsDefault", scaredPatron.scaredTime);
        }
    }

    public void LeaveHose()
    {
        ia.fsm.SendEvent((int)Flags.onOpenDoor);
    }
}
