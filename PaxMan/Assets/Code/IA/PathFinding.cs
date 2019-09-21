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

    public void IgnoreTile(Tile _tile)
    {
        _tile.CloseTile();
        closedTiles.Add(_tile);
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
            Tile tile = GetNearestTile(destinationTile, openTiles);
            if (tile == destinationTile)
            {
                List<Tile> tilePath = new List<Tile>();
                tilePath.Add(tile);
                tilePath = GeneratePath(tilePath, tile);

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
            tile.CloseTile();
            openTiles.Remove(tile);
            closedTiles.Add(tile);
            for (int i = 0; i < tile.Adjacents.Count; i++)
            {
                if (map.tiles[tile.Adjacents[i]].GetState() == Tile.TileStates.Ready)
                {
                    if (!map.tiles[tile.Adjacents[i]].IsObstacle)
                    {
                        map.tiles[tile.Adjacents[i]].OpenTile(tile);

                        openTiles.Add(map.tiles[tile.Adjacents[i]]);
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
    private List<Tile> GeneratePath(List<Tile> _list, Tile _tile)
    {
        if (_tile.ParentTile != null)
        {
            _list.Add(_tile.ParentTile);
            GeneratePath(_list, _tile.ParentTile);
        }
        _list.Reverse();
        return _list;
    }

    public Tile GetNearestTile(Tile _currentTile, List<Tile> _targets)
    {
        Tile tile = null;
        uint currentMinDistance = int.MaxValue;
        for (int i = 0; i < _targets.Count; i++)
        {
            uint manhattanDistance = ManhattanDistance(_currentTile.Position, _targets[i].Position);
            if (manhattanDistance < currentMinDistance)
            {
                tile = _targets[i];
                currentMinDistance = manhattanDistance;
            }
        }
        return tile;
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
    public uint ManhattanDistance(Vector2 _origin, Vector2 _destination)
    {
        _origin.x /= map.horizontalTileDistance;
        _destination.x /= map.horizontalTileDistance;

        _origin.y /= map.verticalTileDistance;
        _destination.y /= map.verticalTileDistance;

        uint x = (uint)Mathf.Abs(_origin.x - _destination.x);
        uint y = (uint)Mathf.Abs(_origin.y - _destination.y);

         return x + y;
    }
}

