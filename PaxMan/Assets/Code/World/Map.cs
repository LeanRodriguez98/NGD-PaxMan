using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public class Map : MonoBehaviour
{
    public static Map instance;

    [System.Serializable]
    public struct ConectionColors
    {
        public Color obstacleTileColor;
        public Color zeroConectionTileColor;
        public Color oneConectionTileColor;
        public Color twoConectionTileColor;
        public Color threeConectionTileColor;
        public Color fourConectionTileColor;
    }

    [System.Serializable]
    public struct TypeColor
    {
        public Color emptyTileColor;
        public Color smallDotTileColor;
        public Color bigDotTileColor;
        public Color obstacleTileColor;
        public Color unexpectedTileColor;
    }

    [System.Serializable]
    public class WarpZone
    {
        public WarpZone(Tile _a, Tile _b)
        {
            warpTileA = _a;
            warpTileB = _b;
        }
        public Tile warpTileA;
        public Tile warpTileB;
    }

    public Rect gameArea;
    public Vector2Int divisions;

    public List<Tile> tiles = new List<Tile>();
    public GameObject SmallDotPrefab;
    public GameObject BigDotPrefab;
    public PickupableObject cherrys;
    public List<float> cherrysDuration = new List<float>();

    public bool drawGizmos;
    public bool drawGrid;
    public bool drawTileID;
    public bool drawTilesType;
    public TypeColor tileTypeColors;
    public bool drawTilesConections;
    public ConectionColors tileConectionsColors;
    public float horizontalTileDistance;
    public float verticalTileDistance;
    private string[] lines;

    public List<WarpZone> warpZones = new List<WarpZone>();
    public List<PickupableObject> smallDots = new List<PickupableObject>();
    public List<PickupableObject> bigDots = new List<PickupableObject>();

    private const string mapFilePath = "Assets/Data/map.txt";
    private const char obstacleTile = 'x';
    private const char voidTile = ' ';
    private const char samllDotTile = '.';
    private const char bigDotTile = 'o';
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (tiles.Count == 0)
        {
            InitMap();
            Debug.LogWarning("The map was inited in ejecuton, plase use the \"Init Map\" button in this object", this.gameObject);
        }
    }

    public void InitMap()
    {
        ClearMap();
        if (InitMapFile(mapFilePath, out lines))
        {
            GenerateMap(lines);
        }
    }

    public void ClearMap()
    {
        if (tiles != null)
        {
            tiles.Clear();
        }
        if (smallDots.Count > 0)
        {
            GameObject parent = null;
            parent = smallDots[0].gameObject.transform.parent.gameObject;

            foreach (PickupableObject smallDot in smallDots)
            {
                DestroyImmediate(smallDot.gameObject);
            }
            DestroyImmediate(parent);
            smallDots.Clear();
        }
        if (bigDots.Count > 0)
        {
            GameObject parent = null;
            parent = bigDots[0].gameObject.transform.parent.gameObject;
            foreach (PickupableObject bigDot in bigDots)
            {
                DestroyImmediate(bigDot.gameObject);
            }
            DestroyImmediate(parent);
            bigDots.Clear();
        }
    }

    private bool InitMapFile(string _path, out string[] _lines)
    {
        _lines = null;
        try
        {
            lines = File.ReadAllLines(_path);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
            return false;
        }
        return true;
    }

    public void GenerateMap(string[] _lines)
    {
        horizontalTileDistance = gameArea.width / divisions.x;
        verticalTileDistance = gameArea.height / divisions.y;
        uint iterations = 0;
        for (int y = 0; y < _lines.Length; y++)
        {
            char[] line = _lines[y].ToCharArray();
            for (int x = 0; x < line.Length; x++)
            {
                Vector2 tilePosition;
                tilePosition.x = gameArea.xMin + (x * horizontalTileDistance) + (horizontalTileDistance / 2.0f);
                tilePosition.y = gameArea.yMax - (y * verticalTileDistance) - (verticalTileDistance / 2.0f);
                bool isObstacle = false;
                char charFile = line[x];
                Rect area = new Rect(new Vector2(gameArea.xMin + (x * horizontalTileDistance), gameArea.yMax - (y * verticalTileDistance) - verticalTileDistance), new Vector2(horizontalTileDistance, verticalTileDistance));
                tiles.Add(new Tile(tilePosition, Tile.TileStates.Ready, isObstacle, area, iterations, charFile));
                iterations++;
            }
        }
        AddDots(tiles);
        ConectTiles();
    }

    public void AddDots(List<Tile> _tiles)
    {
        GameObject smallDotsParent = new GameObject("SmallDots");
        smallDotsParent.transform.parent = this.gameObject.transform;
        GameObject bigDotsParent = new GameObject("BigDots");
        bigDotsParent.transform.parent = this.gameObject.transform;
        foreach (Tile tile in _tiles)
        {
            switch (tile.CharFile)
            {
                case obstacleTile:
                    tile.IsObstacle = true;
                    break;
                case samllDotTile:
                    PickupableObject smallDot = Instantiate(SmallDotPrefab, tile.Position, Quaternion.identity, smallDotsParent.transform).GetComponent<PickupableObject>();
                    smallDot.SetCollider();
                    smallDots.Add(smallDot);
                    break;
                case bigDotTile:
                    PickupableObject bigDot = Instantiate(BigDotPrefab, tile.Position, Quaternion.identity, bigDotsParent.transform).GetComponent<PickupableObject>();
                    bigDot.SetCollider();
                    bigDots.Add(bigDot);
                    break;
            }
        }
        cherrys.gameObject.SetActive(true);
        cherrys.SetCollider();
        cherrys.gameObject.SetActive(false);
    }

    public void EnableCherry()
    {
        cherrys.gameObject.SetActive(true);
        Invoke("DisableCherry", cherrysDuration[UnityEngine.Random.Range(0, cherrysDuration.Count)]);
    }

    public void DisableCherry()
    {
        cherrys.gameObject.SetActive(false);
        CancelInvoke("DisableCherry");
    }
    public void ConectTiles()
    {
        Vector2 rightDistance = new Vector2(horizontalTileDistance, 0.0f);
        Vector2 upDistance = new Vector2(0.0f, verticalTileDistance);

        foreach (Tile currentTile in tiles)
        {
            if (!currentTile.IsObstacle)
            {
                foreach (Tile tile in tiles)
                {
                    if (!tile.IsObstacle)
                    {
                        if (currentTile.Position + upDistance == tile.Position || currentTile.Position + rightDistance == tile.Position)
                        {
                            AddTileConection(currentTile, tile);
                        }
                    }
                }
            }
        }
    }
    public void AddTileConection(Tile _a, Tile _b)
    {
        _a.AddConection(_b, tiles);
        _b.AddConection(_a, tiles);
    }
    public Tile PositionToTile(Vector2 _objectPosition)
    {
        float distance = float.PositiveInfinity;
        Tile currentTile = null;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (!tiles[i].IsObstacle)
            {
                float currentDistance = Vector2.Distance(tiles[i].Position, _objectPosition);
                if (currentDistance < distance)
                {
                    currentTile = tiles[i];
                    distance = currentDistance;
                }
            }
        }
        return currentTile;
    }

    public Tile IdToTile(uint _i)
    {
        return tiles[(int)_i];
    }

    public Tile GetNextTile(Tile _currentTile, Vector2 _direction)
    {

        if (horizontalTileDistance == 0)                            // <--- Change this, make property
            horizontalTileDistance = gameArea.width / divisions.x;  // <--- Change this, make property
        if (verticalTileDistance == 0)                              // <--- Change this, make property
            verticalTileDistance = gameArea.height / divisions.y;   // <--- Change this, make property

        _direction *= new Vector2(horizontalTileDistance, verticalTileDistance);

        for (int i = 0; i < _currentTile.Adjacents.Count; i++)
        {
            if (_currentTile.Position + _direction == tiles[_currentTile.Adjacents[i]].Position)
            {
                return tiles[_currentTile.Adjacents[i]];
            }
        }

        return GetWarpTile(_currentTile);
    }

    public Tile GetWarpTile(Tile _currentTile)
    {
        for (int i = 0; i < warpZones.Count; i++)
        {
            if (_currentTile.Index == warpZones[i].warpTileA.Index)
            {
                return warpZones[i].warpTileB;
            }
            else if (_currentTile.Index == warpZones[i].warpTileB.Index)
            {
                return warpZones[i].warpTileA;
            }
        }
        return null;
    }

    public bool IsWarpZone(Tile _currentTile)
    {
        for (int i = 0; i < warpZones.Count; i++)
        {
            if (_currentTile.Index == warpZones[i].warpTileA.Index || _currentTile.Index == warpZones[i].warpTileB.Index)
            {
                return true;
            }
        }
        return false;
    }

    public Vector2 GetWarpDestination(Tile _currentTile)
    {
        for (int i = 0; i < warpZones.Count; i++)
        {
            if (_currentTile.Index == warpZones[i].warpTileA.Index)
            {
                return warpZones[i].warpTileB.Position;
            }
            else if (_currentTile.Index == warpZones[i].warpTileB.Index)
            {
                return warpZones[i].warpTileA.Position;
            }
        }
        return Vector2.zero;
    }

    public void AddWarpZone(Tile _a, Tile _b)
    {
        WarpZone warp = new WarpZone(_a, _b);
        warpZones.Add(warp);
    }

    public List<Tile> GetAllTilesOfConectionsNumber(int[] _indexes)
    {
        List<Tile> auxTiles = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            for (int i = 0; i < _indexes.Length; i++)
            {
                if (tile.Adjacents.Count == _indexes[i])
                {
                    auxTiles.Add(tile);
                }
            }

        }
        return auxTiles;
    }

    public bool IsPaxManVisible(Vector2 _currentPosition)
    {
        Tile currentTile = PositionToTile(_currentPosition);
        Tile paxManTile = PositionToTile(GameManager.instance.GameData.paxManPosition);

        int iterations = 0;
        Tile auxTile = tiles[currentTile.Index + iterations];
        while (!auxTile.IsObstacle)
        {
            iterations++;
            auxTile = tiles[currentTile.Index + iterations];
            if (paxManTile.Index == auxTile.Index)
                return true;
        }


        iterations = 0;
        auxTile = tiles[currentTile.Index + iterations];
        while (!auxTile.IsObstacle)
        {
            iterations--;
            auxTile = tiles[currentTile.Index + iterations];
            if (paxManTile.Index == auxTile.Index)
                return true;
        }


        iterations = 0;
        auxTile = tiles[currentTile.Index + iterations];
        while (!auxTile.IsObstacle)
        {
            iterations += divisions.x;
            auxTile = tiles[currentTile.Index + iterations];
            if (paxManTile.Index == auxTile.Index)
                return true;
        }


        iterations = 0;
        auxTile = tiles[currentTile.Index + iterations];
        while (!auxTile.IsObstacle)
        {
            iterations -= divisions.x;
            auxTile = tiles[currentTile.Index + iterations];
            if (paxManTile.Index == auxTile.Index)
                return true;
        }


        return false;
    }

    

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {

            if (drawGrid)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawLine(gameArea.position, gameArea.position + new Vector2(gameArea.width, 0.0f));
                Gizmos.DrawLine(gameArea.position, gameArea.position + new Vector2(0.0f, gameArea.height));
                Gizmos.DrawLine(gameArea.position + new Vector2(gameArea.width, 0.0f), gameArea.position + new Vector2(gameArea.width, gameArea.height));
                Gizmos.DrawLine(gameArea.position + new Vector2(0.0f, gameArea.height), gameArea.position + new Vector2(gameArea.width, gameArea.height));

                for (int i = 0; i < divisions.x; i++)
                {
                    Gizmos.DrawLine(new Vector2(gameArea.position.x + ((gameArea.width / (float)divisions.x) * i), gameArea.position.y), new Vector2(gameArea.position.x + ((gameArea.width / (float)divisions.x) * i), gameArea.position.y + gameArea.height));
                }
                for (int i = 0; i < divisions.y; i++)
                {
                    Gizmos.DrawLine(new Vector2(gameArea.position.x, gameArea.position.y + ((gameArea.height / (float)divisions.y) * i)), new Vector2(gameArea.position.x + gameArea.width, gameArea.position.y + ((gameArea.height / (float)divisions.y) * i)));
                }

            }
            if (drawTileID)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 15;
                style.normal.textColor = Color.blue;
                for (int i = 0; i < tiles.Count; i++)
                {
                    Handles.Label(tiles[i].Position, tiles[i].Index.ToString(), style);
                }
            }
            if (drawTilesType)
            {
                foreach (Tile tile in tiles)
                {
                    if (tile.CharFile == obstacleTile)
                    {
                        Gizmos.color = tileTypeColors.obstacleTileColor;
                    }
                    else if (tile.CharFile == samllDotTile)
                    {
                        Gizmos.color = tileTypeColors.smallDotTileColor;
                    }
                    else if (tile.CharFile == bigDotTile)
                    {
                        Gizmos.color = tileTypeColors.bigDotTileColor;
                    }
                    else if (tile.CharFile == voidTile)
                    {
                        Gizmos.color = tileTypeColors.emptyTileColor;
                    }
                    else
                    {
                        Gizmos.color = tileTypeColors.unexpectedTileColor;
                    }
                    Gizmos.DrawWireSphere(new Vector3(tile.Position.x, tile.Position.y, 0.0f), 0.05f);
                }

            }
            else if (drawTilesConections)
            {
                foreach (Tile tile in tiles)
                {
                    if (tile.IsObstacle)
                    {
                        Gizmos.color = tileConectionsColors.obstacleTileColor;
                    }
                    else
                    {
                        switch (tile.Adjacents.Count)
                        {
                            case 0:
                                Gizmos.color = tileConectionsColors.zeroConectionTileColor;
                                break;
                            case 1:
                                Gizmos.color = tileConectionsColors.oneConectionTileColor;
                                break;
                            case 2:
                                Gizmos.color = tileConectionsColors.twoConectionTileColor;
                                break;
                            case 3:
                                Gizmos.color = tileConectionsColors.threeConectionTileColor;
                                break;
                            case 4:
                                Gizmos.color = tileConectionsColors.fourConectionTileColor;
                                break;
                        }

                    }
                    Gizmos.DrawWireSphere(new Vector3((float)tile.Position.x, (float)tile.Position.y, 0.0f), 0.05f);
                }
            }
        }
    }
}
