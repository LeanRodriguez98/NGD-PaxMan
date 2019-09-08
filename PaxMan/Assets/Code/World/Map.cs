using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public class Map : MonoBehaviour
{

    public Rect gameArea;
    public Vector2Int divisions;

     [System.Serializable]
     public struct PathmapTile
     {
         public Vector2 position;
         public bool blocking;
         public bool visited;
        public char charFile;
     }
    public List<PathmapTile> tiles = new List<PathmapTile>();

    

     public GameObject SmallDotPrefab;
     public GameObject LargeDotPrefab;
    /*
     private int dotCount = 0;
     public int DotCount { get { return dotCount; } }

     public List<SmallDot> smallDots = new List<SmallDot>();
     public List<BigDot> bigDots = new List<BigDot>();
     public List<Cherry> cherry = new List<Cherry>();
     */
    // Start is called before the first frame update
    void Start()
     {
         //StartCoroutine( InitPathmap());
        InitPathmap();
         //InitDots();
         //initBigDots();
    }



     // Update is called once per frame
     void Update()
     {

     }

    //public IEnumerator InitPathmap()
    public void InitPathmap()
    {
         string[] lines = File.ReadAllLines("Assets/Data/map.txt");
         for (int y = 0; y < lines.Length; y++)
         {
             char[] line = lines[y].ToCharArray();
            Debug.Log(line.Length);
             for (int x = 0; x < line.Length; x++)
             {
                PathmapTile tile = new PathmapTile();
                tile.position.x = gameArea.xMin + (x * (gameArea.width / divisions.x)) + ((gameArea.width / divisions.x)/2.0f);
                tile.position.y = gameArea.yMax - (y * (gameArea.height / divisions.y)) - ((gameArea.height / divisions.y)/2.0f);
                tile.blocking = false;
                switch (line[x])
                {
                    case 'x':
                        tile.blocking = true;
                        break;
                    case '.':
                        Instantiate(SmallDotPrefab, tile.position, Quaternion.identity);
                        break;
                    case 'o':
                        Instantiate(LargeDotPrefab, tile.position, Quaternion.identity);
                        break;
                }
                tile.charFile = line[x];
                 tiles.Add(tile);
                //yield return null;// new WaitForSeconds(0.10f);
             }
         }
     }

     /*public bool InitDots()
     {
         string[] lines = System.IO.File.ReadAllLines("Assets/Data/map.txt");
         for (int y = 0; y < lines.Length; y++)
         {
             char[] line = lines[y].ToCharArray();
             for (int x = 0; x < line.Length; x++)
             {
                 if (line[x] == '.')
                 {
                     SmallDot dot = GameObject.Instantiate(SmallDotPrefab).GetComponent<SmallDot>();
                     dot.SetPosition(new Vector2((x - (line.Length / 2)) * 22 + 11, (-y + (lines.Length / 2)) * 22));
                     dotCount++;
                 }
             }
         }
         return true;
     }

     public bool initBigDots()
     {
         string[] lines = System.IO.File.ReadAllLines("Assets/Data/map.txt");
         for (int y = 0; y < lines.Length; y++)
         {
             char[] line = lines[y].ToCharArray();
             for (int x = 0; x < line.Length; x++)
             {
                 if (line[x] == 'o')
                 {
                     BigDot dot = GameObject.Instantiate(LargeDotPrefab).GetComponent<BigDot>();
                     dot.SetPosition(new Vector2((x - (line.Length / 2)) * 22 + 11, (-y + (lines.Length / 2)) * 22));
                     dotCount++;
                 }
             }
         }
         return true;
     }

     internal bool TileIsValid(int tileX, int tileY)
     {
         for (int t = 0; t < tiles.Count; t++)
         {
             if (tileX == tiles[t].posX && tileY == tiles[t].posY && !tiles[t].blocking)
                 return true;
         }
         return false;
     }

     public List<PathmapTile> GetPath(int currentTileX, int currentTileY, int targetX, int targetY)
     {
         PathmapTile fromTile = GetTile(currentTileX, currentTileY);
         PathmapTile toTile = GetTile(targetX, targetY);

         for (int t = 0; t < tiles.Count; t++)
         {
             tiles[t].visited = false;
         }

         List<PathmapTile> path = new List<PathmapTile>();
         if (Pathfind(fromTile, toTile, path))
         {
             return path;
         }
         return null;
     }

     private bool Pathfind(PathmapTile fromTile, PathmapTile toTile, List<PathmapTile> path)
     {
         fromTile.visited = true;

         if (fromTile.blocking)
             return false;
         path.Add(fromTile);
         if (fromTile == toTile)
             return true;

         List<PathmapTile> neighbours = new List<PathmapTile>();

         PathmapTile up = GetTile(fromTile.posX, fromTile.posY - 1);
         if (up != null && !up.visited && !up.blocking && !path.Contains(up))
             neighbours.Insert(0, up);

         PathmapTile down = GetTile(fromTile.posX, fromTile.posY + 1);
         if (down != null && !down.visited && !down.blocking && !path.Contains(down))
             neighbours.Insert(0, down);

         PathmapTile right = GetTile(fromTile.posX + 1, fromTile.posY);
         if (right != null && !right.visited && !right.blocking && !path.Contains(right))
             neighbours.Insert(0, right);

         PathmapTile left = GetTile(fromTile.posX - 1, fromTile.posY);
         if (left != null && !left.visited && !left.blocking && !path.Contains(left))
             neighbours.Insert(0, left);

         for(int n = 0; n < neighbours.Count; n++)
         {
             PathmapTile tile = neighbours[n];

             path.Add(tile);

             if (Pathfind(tile, toTile, path))
                 return true;

             path.Remove(tile);
         }

         return false;
     }

     public PathmapTile GetTile(int tileX, int tileY)
     {
         for (int t = 0; t < tiles.Count; t++)
         {
             if (tileX == tiles[t].posX && tileY == tiles[t].posY)
                 return tiles[t];
         }

         return null;
     }

     public bool HasIntersectedDot(Vector2 aPosition)
     {
         for (int d = 0; d < smallDots.Count; d++)
         {
             if ((smallDots[d].GetPosition() - aPosition).magnitude < 5.0f)
             {
                 GameObject.DestroyImmediate(smallDots[d]);
                 smallDots.Remove(smallDots[d]);
                 return true;
             }
         }

         return false;
     }

     public bool HasIntersectedBigDot(Vector2 aPosition)
     {
         for (int d = 0; d < bigDots.Count; d++)
         {
             if ((bigDots[d].GetPosition() - aPosition).magnitude < 5.0f)
             {
                 GameObject.DestroyImmediate(bigDots[d]);
                 bigDots.Remove(bigDots[d]);
                 return true;
             }
         }

         return false;
     }

     bool HasIntersectedCherry(Vector2 aPosition)
     {
         return true;
     }*/

    private void OnDrawGizmos()
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


        foreach (PathmapTile tile in tiles)
        {
            if (tile.charFile == 'x')
            {
                Gizmos.color = Color.blue;
            }
            else if (tile.charFile == '.')
            {
                Gizmos.color = Color.yellow;
            }
            else if (tile.charFile == 'o')
            {
                Gizmos.color = Color.magenta;
            }
            else if( tile.charFile == ' ')
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.black;
            }
            Gizmos.DrawWireSphere(new Vector3(tile.position.x, tile.position.y, 0.0f), 0.05f);


        }


    }
}

