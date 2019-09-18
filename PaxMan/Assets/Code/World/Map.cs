using System;
using System.Collections;
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
        public Color obstacleNodeColor;
        public Color zeroConectionNodeColor;
        public Color oneConectionNodeColor;
        public Color twoConectionNodeColor;
        public Color threeConectionNodeColor;
        public Color fourConectionNodeColor;
    }

    [System.Serializable]
    public struct TypeColor
    {
        public Color emptyNodeColor;
        public Color smallDotNodeColor;
        public Color bigDotNodeColor;
        public Color obstacleNodeColor;
        public Color unexpectedNodeColor;
    }

    [System.Serializable]
    public class WarpZone
    {
        public WarpZone(Node a, Node b)
        {
            warpNodeA = a;
            warpNodeB = b;
        }
        public Node warpNodeA;
        public Node warpNodeB;
    }

    public Rect gameArea;
    public Vector2Int divisions;

    public List<Node> nodes = new List<Node>();
    public GameObject SmallDotPrefab;
    public GameObject BigDotPrefab;

    public PickupableObject cherrys;
    public List<float> cherrysDuration = new List<float>();

    public bool drawGizmos;
    public bool drawGrid;
    public bool drawNodeID;
    public bool drawNodesType;
    public TypeColor nodeTypeColors;
    public bool drawNodesConections;
    public ConectionColors nodeConectionsColors;
    private string[] lines;
    public float horizontalNodeDistance;
    public float verticalNodeDistance;

    public List<WarpZone> warpZones = new List<WarpZone>();

    public List<PickupableObject> smallDots = new List<PickupableObject>();
    public List<PickupableObject> bigDots = new List<PickupableObject>();

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (nodes == null)
        {
            InitMap();
            Debug.LogWarning("The map was inited in ejecuton, plase use the \"Init Map\" button in this object", this.gameObject);
        }
    }

    public void InitMap()
    {
        ClearMap();
        if (InitMapFile("Assets/Data/map.txt", out lines))
        {
            GenerateMap(lines);
        }
    }

    public void ClearMap()
    {
        if (nodes != null)
        {
            nodes.Clear();
        }
        if (smallDots != null)
        {
            foreach (PickupableObject smallDot in smallDots)
            {
                DestroyImmediate(smallDot.gameObject);
            }
            smallDots.Clear();
        }
        if (bigDots != null)
        {
            foreach (PickupableObject bigDot in bigDots)
            {
                DestroyImmediate(bigDot.gameObject);
            }
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
        horizontalNodeDistance = gameArea.width / divisions.x;
        verticalNodeDistance = gameArea.height / divisions.y;
        uint iterations = 0;
        for (int y = 0; y < _lines.Length; y++)
        {
            char[] line = _lines[y].ToCharArray();
            for (int x = 0; x < line.Length; x++)
            {
                Vector2 nodePosition;
                nodePosition.x = gameArea.xMin + (x * horizontalNodeDistance) + (horizontalNodeDistance / 2.0f);
                nodePosition.y = gameArea.yMax - (y * verticalNodeDistance) - (verticalNodeDistance / 2.0f);
                bool isObstacle = false;
                char charFile = line[x];
                Rect area = new Rect(new Vector2(gameArea.xMin + (x * horizontalNodeDistance), gameArea.yMax - (y * verticalNodeDistance) - verticalNodeDistance), new Vector2(horizontalNodeDistance, verticalNodeDistance));
                nodes.Add(new Node(nodePosition, Node.NodeStates.Ready, isObstacle, area, iterations , charFile));
                iterations++;
            }
        }
        AddDots(nodes);
        ConectNodes();
    }

    public void AddDots(List<Node> _nodes)
    {
        GameObject smallDotsParent = new GameObject("SmallDots");
        smallDotsParent.transform.parent = this.gameObject.transform;
        GameObject bigDotsParent = new GameObject("BigDots");
        bigDotsParent.transform.parent = this.gameObject.transform;
        foreach (Node node in _nodes)
        {
            switch (node.CharFile)
            {
                case 'x':
                    node.IsObstacle = true;
                    break;
                case '.':
                    PickupableObject smallDot = Instantiate(SmallDotPrefab, node.Position, Quaternion.identity, smallDotsParent.transform).GetComponent<PickupableObject>();
                    smallDot.SetCollider();
                    smallDots.Add(smallDot);
                    break;
                case 'o':
                    PickupableObject bigDot = Instantiate(BigDotPrefab, node.Position, Quaternion.identity, bigDotsParent.transform).GetComponent<PickupableObject>();
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
    public void ConectNodes()
    {
        Vector2 rightDistance = new Vector2(horizontalNodeDistance, 0.0f);
        Vector2 upDistance = new Vector2(0.0f, verticalNodeDistance);

        foreach (Node currentNode in nodes)
        {
            if (!currentNode.IsObstacle)
            {
                foreach (Node node in nodes)
                {
                    if (!node.IsObstacle)
                    {
                        if (currentNode.Position + upDistance == node.Position || currentNode.Position + rightDistance == node.Position)
                        {
                            AddNodeConection(currentNode, node);
                        }
                    }
                }
            }
        }
    }
    public void AddNodeConection(Node a, Node b)
    {
        a.AddConection(b, nodes);
        b.AddConection(a, nodes);
    }
    public Node PositionToNode(Vector2 objectPosition)
    {
        float distance = float.PositiveInfinity;
        Node currentNode = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (!nodes[i].IsObstacle)
            {
                float currentDistance = Vector2.Distance(nodes[i].Position, objectPosition);
                if (currentDistance < distance)
                {
                    currentNode = nodes[i];
                    distance = currentDistance;
                }
            }
        }
        return currentNode;
    }

    public Node IdToNode(uint i)
    {
        return nodes[(int)i];
    }

    public Node GetNextNode(Node currentNode, Vector2 direction)
    {
     
        if (horizontalNodeDistance == 0)
            horizontalNodeDistance = gameArea.width / divisions.x;
        if (verticalNodeDistance == 0)
            verticalNodeDistance = gameArea.height / divisions.y;

        direction *= new Vector2(horizontalNodeDistance, verticalNodeDistance);

        for (int i = 0; i < currentNode.Adjacents.Count; i++)
        {
            if (currentNode.Position + direction == nodes[currentNode.Adjacents[i]].Position)
            {
                return nodes[currentNode.Adjacents[i]];
            }
        }

        return GetWarpNode(currentNode);
    }

    public Node GetWarpNode(Node currentNode)
    {
        for (int i = 0; i < warpZones.Count; i++)
        {
            if (currentNode.Position == warpZones[i].warpNodeA.Position)
            {
                return warpZones[i].warpNodeB;
            }
            else if (currentNode.Position == warpZones[i].warpNodeB.Position)
            {
                return warpZones[i].warpNodeA;
            }
        }
        return null;
    }

    public bool IsWarpZone(Node currentNode)
    {
        for (int i = 0; i < warpZones.Count; i++)
        {
            if (currentNode.Position == warpZones[i].warpNodeA.Position || currentNode.Position == warpZones[i].warpNodeB.Position)
            {
                return true;
            }
        }
        return false;
    }

    public Vector2 GetWarpDestination(Node currentNode)
    {
        for (int i = 0; i < warpZones.Count; i++)
        {
            if (currentNode.Position == warpZones[i].warpNodeA.Position)
            {
                return warpZones[i].warpNodeB.Position;
            }
            else if (currentNode.Position == warpZones[i].warpNodeB.Position)
            {
                return warpZones[i].warpNodeA.Position;
            }
        }
        return Vector2.zero;
    }

    public void AddWarpZone(Node a, Node b)
    {
        WarpZone warp = new WarpZone(a, b);
        warpZones.Add(warp);
    }

    public List<Node> GetAllNodesOfConectionsNumber(int[] indexes)
    {
        List<Node> _nodes = new List<Node>();
        foreach (Node node in nodes)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                if (node.Adjacents.Count == indexes[i])
                {
                    _nodes.Add(node);
                }
            }
            
        }
        return _nodes;
    }

    public bool IsPaxManVisible(Vector2 currentPosition)
    {
        Node currentNode = PositionToNode(currentPosition);
        Node paxManNode = PositionToNode(GameManager.instance.player.transform.position);

        

        int iterations = 0;
        Node auxNode = nodes[currentNode.Index + iterations]; 
        while (!auxNode.IsObstacle)
        {
            iterations++;
            auxNode = nodes[currentNode.Index + iterations];
            if (paxManNode.Position == auxNode.Position)
                return true;
        }


        iterations = 0;
        auxNode = nodes[currentNode.Index + iterations];
        while (!auxNode.IsObstacle)
        {
            iterations--;
            auxNode = nodes[currentNode.Index + iterations];
            if (paxManNode.Position == auxNode.Position)
                return true;
        }


        iterations = 0;
        auxNode = nodes[currentNode.Index + iterations];
        while (!auxNode.IsObstacle)
        {
            iterations+= divisions.x;
            auxNode = nodes[currentNode.Index + iterations];
            if (paxManNode.Position == auxNode.Position)
                return true;
        }


        iterations = 0;
        auxNode = nodes[currentNode.Index + iterations];
        while (!auxNode.IsObstacle)
        {
            iterations -= divisions.x;
            auxNode = nodes[currentNode.Index + iterations];
            if (paxManNode.Position == auxNode.Position)
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

                /* foreach (Node node in nodes)
                 {
                     Gizmos.color = new Color(UnityEngine.Random.Range(0F, 1F), UnityEngine.Random.Range(0, 1F), UnityEngine.Random.Range(0, 1F));

                     Gizmos.DrawLine(node.Area.position, node.Area.position + new Vector2(node.Area.width, 0.0f));
                     Gizmos.DrawLine(node.Area.position, node.Area.position + new Vector2(0.0f, node.Area.height));
                     Gizmos.DrawLine(node.Area.position + new Vector2(node.Area.width, 0.0f), node.Area.position + new Vector2(node.Area.width, node.Area.height));
                     Gizmos.DrawLine(node.Area.position + new Vector2(0.0f, node.Area.height), node.Area.position + new Vector2(node.Area.width, node.Area.height));
                 }*/



            }
            if (drawNodeID)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 15;
                style.normal.textColor = Color.blue;
                for (int i = 0; i < nodes.Count; i++)
                {
                    Handles.Label(nodes[i].Position, nodes[i].Index.ToString(), style);
                }
            }
            if (drawNodesType)
            {
                foreach (Node tile in nodes)
                {
                    if (tile.CharFile == 'x')
                    {
                        Gizmos.color = nodeTypeColors.obstacleNodeColor;
                    }
                    else if (tile.CharFile == '.')
                    {
                        Gizmos.color = nodeTypeColors.smallDotNodeColor;
                    }
                    else if (tile.CharFile == 'o')
                    {
                        Gizmos.color = nodeTypeColors.bigDotNodeColor;
                    }
                    else if (tile.CharFile == ' ')
                    {
                        Gizmos.color = nodeTypeColors.emptyNodeColor;
                    }
                    else
                    {
                        Gizmos.color = nodeTypeColors.unexpectedNodeColor;
                    }
                    Gizmos.DrawWireSphere(new Vector3(tile.Position.x, tile.Position.y, 0.0f), 0.05f);
                }

            }
            else if (drawNodesConections)
            {
                foreach (Node node in nodes)
                {
                    if (node.IsObstacle)
                    {
                        Gizmos.color = nodeConectionsColors.obstacleNodeColor;
                    }
                    else
                    {
                        switch (node.Adjacents.Count)
                        {
                            case 0:
                                Gizmos.color = nodeConectionsColors.zeroConectionNodeColor;
                                break;
                            case 1:
                                Gizmos.color = nodeConectionsColors.oneConectionNodeColor;
                                break;
                            case 2:
                                Gizmos.color = nodeConectionsColors.twoConectionNodeColor;
                                break;
                            case 3:
                                Gizmos.color = nodeConectionsColors.threeConectionNodeColor;
                                break;
                            case 4:
                                Gizmos.color = nodeConectionsColors.fourConectionNodeColor;
                                break;
                        }

                    }
                    Gizmos.DrawWireSphere(new Vector3((float)node.Position.x, (float)node.Position.y, 0.0f), 0.05f);
                }
            }
        }
    }
}
