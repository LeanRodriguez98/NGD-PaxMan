using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Tile
{
    [SerializeField] private List<int> adjacentsIndex;
    [SerializeField] private Vector2 position;
    [SerializeField] private TileStates tileState;
    [SerializeField] private TileStates originalState;
    [SerializeField] private bool isObstacle;
    [SerializeField] private Tile parentTile;
    [SerializeField] private bool used;
    [SerializeField] private char charFile;
    [SerializeField] private Rect area;
    [SerializeField] private uint index;
    public List<int> Adjacents
    {
        get {

            if (adjacentsIndex == null)
            {
                adjacentsIndex = new List<int>();
            }

            return adjacentsIndex; }
    }

    public int Index
    {
        get { return (int)index; }
    }

    public TileStates TileState
    {
        get { return tileState;  }
        set { tileState = value; }
    }

    public Rect Area
    {
        get { return area; }
        set { area = value; }
    }

    public Tile ParentTile
    {
        get { return parentTile; }
        set { parentTile = value; }
    }

    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    public bool IsObstacle
    {
        get { return isObstacle; }
        set { isObstacle = value; }
    }

    public char CharFile
    {
        get { return charFile; }
        set { charFile = value; }
    }

    public enum TileStates
    {
        Open,
        Close,
        Ready,
        [HideInInspector]_count
    }

    public Tile(Vector2 _position, TileStates _state, bool _isObstacle, Rect _area, uint _index ,char _charFile)
    {
        isObstacle = _isObstacle;
        position = _position;
        tileState = originalState = _state;
        used = false;
        area = _area;
        index = _index;
        charFile = _charFile;
    }

    public TileStates GetState()
    {
        return tileState;
    }

    public void AddConection(Tile _tile, List<Tile> _list)
    {
        if (adjacentsIndex == null)
        {
            adjacentsIndex = new List<int>();
        }
        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i] == _tile)
            {
                adjacentsIndex.Add(i);
                return;
            }
        }
    }

    public void OpenTile()
    {
        if (!IsObstacle && !used)
            tileState = TileStates.Open;
    }

    public void OpenTile(Tile _tile)
    {
        if (!IsObstacle && !used)
        {
            parentTile = _tile;
            tileState = TileStates.Open;
        }
    }

    public void CloseTile() 
    {
        tileState = TileStates.Close;
        used = true;
    }

    public void RestartTile()
    {
        tileState = originalState;
        used = false;
        parentTile = null;
    }
}
