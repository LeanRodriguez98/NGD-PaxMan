using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{

    [System.Serializable]
    public struct OnHomePatron
    {
        public uint startPositionNodeID;
        public uint endPositionNodeID;
    }
    [Tooltip("You can see the nodes ID checking the \"Show nodes ID\" toggle on the Map script")]
    public uint StartPositionnodeID;
    private Map map;
    private PathFinding pathFinding;
    public OnHomePatron homePatron;
    public List<Vector2> currentPath = new List<Vector2>();
    public int auxPath = 0;//  Change this
    public bool aux = true;// Change this
    private void Start()
    {
        map = Map.instance;
        transform.position = map.IdToNode(StartPositionnodeID).Position;
        pathFinding = new PathFinding();
        currentPath = pathFinding.GetPath(transform.position, map.IdToNode(homePatron.startPositionNodeID).Position);
        StartCoroutine(Movement());
    }

    public IEnumerator Movement()
    {
        Vector2 currentPosition = currentPath[auxPath];
        Vector2 destinationPosition = currentPath[auxPath + 1];
        float i = 0.0f;

        while (transform.position != (Vector3)destinationPosition)
        {
            if (i > 10.0f)
            {
                i = 10.0f;
            }
            transform.position = Vector3.Lerp(currentPosition, destinationPosition, i / 10.0f);
            i++;

            yield return null;
        }
        if (destinationPosition == currentPath[currentPath.Count-1])
        {
            auxPath = 0;
            UpdatePath();
        }
        else
        {
            auxPath++;
            StartCoroutine(Movement());
        }
    }

    private void UpdatePath()
    {
        if (aux)
        {
            currentPath = pathFinding.GetPath(map.IdToNode(homePatron.startPositionNodeID).Position, map.IdToNode(homePatron.endPositionNodeID).Position);
        }
        else
        {
            currentPath.Reverse();
        }
        aux = false;
        StartCoroutine(Movement());
    }
}
