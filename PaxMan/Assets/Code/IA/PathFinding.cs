using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private Map map;
    private Node startNode;
    private Node destinationNode;
    private List<Node> openNodes;
    private List<Node> closedNodes;
    private uint totalWeight;

    public PathFinding()
    {
        openNodes = new List<Node>();
        closedNodes = new List<Node>();
    }
    public void IgnoreNode(Node n)
    {
        n.CloseNode();
        closedNodes.Add(n);
    }


    public List<Vector2> GetPath(Node _startNode, Node _destinationNode)
    {
        if (Map.instance == null)
        {
            return new List<Vector2>();
        }
        map = Map.instance;

        if (map.nodes == null)
        {
            return new List<Vector2>();
        }
        startNode = _startNode;
        destinationNode = _destinationNode;

        startNode.OpenNode();
        openNodes.Add(startNode);
        while (openNodes.Count > 0)
        {
            Node n = GetNearestNode(destinationNode, openNodes);
            if (n == destinationNode)
            {
                List<Node> nodePath = new List<Node>();
                nodePath.Add(n);
                nodePath = GeneratePath(nodePath, n);

                List<Vector2> path = new List<Vector2>();
                for (int i = 0; i < nodePath.Count; i++)
                {
                    path.Add(nodePath[i].Position);
                }

                if (path[0] == destinationNode.Position)
                {
                    path.Reverse();
                }

                ResetNodes();
                return path;
            }
            n.CloseNode();
            openNodes.Remove(n);
            closedNodes.Add(n);
            for (int i = 0; i < n.Adjacents.Count; i++)
            {
                if (map.nodes[n.Adjacents[i]].GetState() == Node.NodeStates.Ready)
                {
                    if (!map.nodes[n.Adjacents[i]].IsObstacle)
                    {
                        map.nodes[n.Adjacents[i]].OpenNode(n);

                        openNodes.Add(map.nodes[n.Adjacents[i]]);
                    }
                }
            }
        }
        ResetNodes();
        List<Vector2> nullPath = new List<Vector2>();
        nullPath.Add(startNode.Position);
        return nullPath;
    }

    private void ResetNodes()
    {
        for (int i = 0; i < openNodes.Count; i++)
        {
            openNodes[i].RestartNode();
        }
        openNodes.Clear();
        for (int i = 0; i < closedNodes.Count; i++)
        {
            closedNodes[i].RestartNode();
        }
        closedNodes.Clear();
    }
    private List<Node> GeneratePath(List<Node> list, Node n)
    {
        if (n.ParentNode != null)
        {
            list.Add(n.ParentNode);
            GeneratePath(list, n.ParentNode);
        }
        list.Reverse();
        return list;
    }

    public Node GetNearestNode(Node _currentNode, List<Node> _targets)
    {
        Node n = null;
        uint currentMinDistance = int.MaxValue;
        for (int i = 0; i < _targets.Count; i++)
        {
            uint manhattanDistance = ManhattanDistance(_currentNode.Position, _targets[i].Position);
            if (manhattanDistance < currentMinDistance)
            {
                n = _targets[i];
                currentMinDistance = manhattanDistance;
            }
        }
        return n;
    }

    public Node GetNearestValidNode(Node _target)
    {
        if (!_target.IsObstacle)
        {
            return _target;
        }
        for (int i = 0; i < _target.Adjacents.Count; i++)
        {
            Node n = GetNearestValidNode(map.nodes[_target.Adjacents[i]]);
            if (n != null)
            {
                return n;
            }
        }
        return null;
    }
    public uint ManhattanDistance(Vector2 origin, Vector2 destination)
    {
        origin.x /= map.horizontalNodeDistance;
        destination.x /= map.horizontalNodeDistance;

        origin.y /= map.verticalNodeDistance;
        destination.y /= map.verticalNodeDistance;

        uint x = (uint)Mathf.Abs(origin.x - destination.x);
        uint y = (uint)Mathf.Abs(origin.y - destination.y);

         return x + y;
    }
}

