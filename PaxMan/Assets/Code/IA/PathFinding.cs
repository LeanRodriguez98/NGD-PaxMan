﻿using System.Collections.Generic;
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
            Node n = GetOpenNode();
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

        return new List<Vector2>();
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

    private Node GetOpenNode()
    {
        Node n = null;
        uint currentMinDistance = int.MaxValue;
        for (int i = 0; i < openNodes.Count; i++)
        {
            if (ManhattanDistance(openNodes[i].Position, destinationNode.Position) < currentMinDistance)
            {
                n = openNodes[i];
                currentMinDistance = ManhattanDistance(openNodes[i].Position, destinationNode.Position);
            }
        }
        return n;

    }

    private uint ManhattanDistance(Vector2 origin, Vector2 destination)
    {
        uint x = (uint)Mathf.Abs(origin.x - destination.x);
        uint y = (uint)Mathf.Abs(origin.y - destination.y);
        return x + y;
    }
}

