using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private Map map;
    private Tile startTile;
    private Tile destinationTile;
    private List<Tile> openTiles;
    private List<Tile> closedTiles;

    public PathFinding()
    {
        openTiles = new List<Tile>();
        closedTiles = new List<Tile>();
    }

    public void IgnoreTile(Tile n)
    {
        n.CloseTile();
        closedTiles.Add(n);
    }

    public List<Vector2> GetPath(Tile _startTile, Tile _destinationTile)
    {
        if (Map.instance == null)
        {
            return new List<Vector2>();
        }
        map = Map.instance;

        if (map.tiles == null)
        {
            return new List<Vector2>();
        }
        startTile = _startTile;
        destinationTile = _destinationTile;

        startTile.OpenTile();
        openTiles.Add(startTile);
        while (openTiles.Count > 0)
        {
            Tile n = GetNearestTile(destinationTile, openTiles);
            if (n == destinationTile)
            {
                List<Tile> tilePath = new List<Tile>();
                tilePath.Add(n);
                tilePath = GeneratePath(tilePath, n);

                List<Vector2> path = new List<Vector2>();
                for (int i = 0; i < tilePath.Count; i++)
                {
                    path.Add(tilePath[i].Position);
                }

                if (path[0] == destinationTile.Position)
                {
                    path.Reverse();
                }

                ResetTiles();
                return path;
            }
            n.CloseTile();
            openTiles.Remove(n);
            closedTiles.Add(n);
            for (int i = 0; i < n.Adjacents.Count; i++)
            {
                if (map.tiles[n.Adjacents[i]].GetState() == Tile.TileStates.Ready)
                {
                    if (!map.tiles[n.Adjacents[i]].IsObstacle)
                    {
                        map.tiles[n.Adjacents[i]].OpenTile(n);

                        openTiles.Add(map.tiles[n.Adjacents[i]]);
                    }
                }
            }
        }
        ResetTiles();
        List<Vector2> nullPath = new List<Vector2>();
        nullPath.Add(startTile.Position);
        return nullPath;
    }

    private void ResetTiles()
    {
        for (int i = 0; i < openTiles.Count; i++)
        {
            openTiles[i].RestartTile();
        }
        openTiles.Clear();
        for (int i = 0; i < closedTiles.Count; i++)
        {
            closedTiles[i].RestartTile();
        }
        closedTiles.Clear();
    }
    private List<Tile> GeneratePath(List<Tile> list, Tile n)
    {
        if (n.ParentTile != null)
        {
            list.Add(n.ParentTile);
            GeneratePath(list, n.ParentTile);
        }
        list.Reverse();
        return list;
    }

    public Tile GetNearestTile(Tile _currentTile, List<Tile> _targets)
    {
        Tile n = null;
        uint currentMinDistance = int.MaxValue;
        for (int i = 0; i < _targets.Count; i++)
        {
            uint manhattanDistance = ManhattanDistance(_currentTile.Position, _targets[i].Position);
            if (manhattanDistance < currentMinDistance)
            {
                n = _targets[i];
                currentMinDistance = manhattanDistance;
            }
        }
        return n;
    }

    public Tile GetNearestValidTile(Tile _target, Vector2 _currentPosition)
    {
        if (!_target.IsObstacle && _target.Position != map.PositionToTile(_currentPosition).Position )
        {
            return _target;
        }
        for (int i = 0; i < _target.Adjacents.Count; i++)
        {
            Tile n = GetNearestValidTile(map.tiles[_target.Adjacents[i]], _currentPosition);
            if (n != null)
            {
                return n;
            }
        }
        return null;
    }
    public uint ManhattanDistance(Vector2 origin, Vector2 destination)
    {
        origin.x /= map.horizontalTileDistance;
        destination.x /= map.horizontalTileDistance;

        origin.y /= map.verticalTileDistance;
        destination.y /= map.verticalTileDistance;

        uint x = (uint)Mathf.Abs(origin.x - destination.x);
        uint y = (uint)Mathf.Abs(origin.y - destination.y);

         return x + y;
    }
}

