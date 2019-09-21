using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Map))]
public class Editor_Map : Editor
{
    private GUIStyle style;
    private string[] tileStates;
    private int from;
    private int to;
    private int tileIndexA;
    private int tileIndexB;


    private bool gridFoldOut = false;
    private bool prefabsFoldOut = false;
    private bool cherrySettingsFoldOut = false;
    private bool tilesFoldOut = false;
    private bool dotsFadeOut = false;
    private void OnEnable()
    {
        tileStates = new string[(int)Tile.TileStates._count];
        for (Tile.TileStates tile = Tile.TileStates.Open; tile < Tile.TileStates._count; tile++)
        {
            tileStates[(int)tile] = tile.ToString();
        }
    }

    public override void OnInspectorGUI()
    {
        Map map = (Map)target;
        style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 25, };
        if (GUILayout.Button("Init Map"))
            map.InitMap();
        if (GUILayout.Button("Clear Map"))
            map.ClearMap();
        gridFoldOut = EditorGUILayout.Foldout(gridFoldOut, "Grid");
        if (gridFoldOut)
        {
            map.gameArea = EditorGUILayout.RectField("Game Area", map.gameArea, null);
            EditorGUILayout.Space();
            map.divisions = EditorGUILayout.Vector2IntField("Divisions", map.divisions, null);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cells = " + (map.divisions.x * map.divisions.y).ToString());
        }
        EditorGUILayout.Space();
        prefabsFoldOut = EditorGUILayout.Foldout(prefabsFoldOut, "Prefabs");
        if (prefabsFoldOut)
        {
            map.SmallDotPrefab = (GameObject)EditorGUILayout.ObjectField("Small Dot", map.SmallDotPrefab, typeof(GameObject), false, null);
            if (map.SmallDotPrefab != null && map.SmallDotPrefab.GetComponent<SpriteRenderer>().sprite != null)
                EditorGUILayout.ObjectField("Preview", map.SmallDotPrefab.GetComponent<SpriteRenderer>().sprite, typeof(Sprite), false, null);
            EditorGUILayout.Space();
            map.BigDotPrefab = (GameObject)EditorGUILayout.ObjectField("Small Dot", map.BigDotPrefab, typeof(GameObject), false, null);
            if (map.BigDotPrefab != null && map.BigDotPrefab.GetComponent<SpriteRenderer>().sprite != null)
                EditorGUILayout.ObjectField("Preview", map.BigDotPrefab.GetComponent<SpriteRenderer>().sprite, typeof(Sprite), false, null);
        }
        EditorGUILayout.Space();
        cherrySettingsFoldOut = EditorGUILayout.Foldout(cherrySettingsFoldOut, "Cherry Settings");
        if (cherrySettingsFoldOut)
        {
            map.cherrys = (PickupableObject)EditorGUILayout.ObjectField("Cherry", map.cherrys, typeof(PickupableObject), false, null);
            EditorGUILayout.Space();
            if (map.cherrys != null && map.cherrys.gameObject.GetComponent<SpriteRenderer>().sprite != null)
                EditorGUILayout.ObjectField("Preview", map.cherrys.gameObject.GetComponent<SpriteRenderer>().sprite, typeof(Sprite), false, null);
            EditorGUILayout.Space();
            SerializedProperty list = serializedObject.FindProperty("cherrysDuration");
            EditorGUILayout.PropertyField(list, new GUIContent("Time of Cherry life posible values"), true);
        }
        EditorGUILayout.Space();
        tilesFoldOut = EditorGUILayout.Foldout(tilesFoldOut, "Tiles");
        if (tilesFoldOut)
        {
            EditorGUILayout.LabelField("Add a warp zone");
            tileIndexA = EditorGUILayout.IntField("Tile Index A", tileIndexA);
            tileIndexB = EditorGUILayout.IntField("Tile Index B", tileIndexB);
            if (GUILayout.Button("Add warp"))
            {
                map.AddWarpZone(map.IdToTile((uint)tileIndexA), map.IdToTile((uint)tileIndexB));
                map.AddTileConection(map.IdToTile((uint)tileIndexA), map.IdToTile((uint)tileIndexB));
            }
            if (GUILayout.Button("Remove Warps"))
            {
                map.warpZones.Clear();
            }
            if (map.warpZones.Count != 0)
            {
                SerializedProperty warpConections = serializedObject.FindProperty("warpZones");
                EditorGUILayout.PropertyField(warpConections, new GUIContent("Warp Conections"), true);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Show tiles data");
            from = EditorGUILayout.IntField("Show From", from);
            to = EditorGUILayout.IntField("To", to);
            if (from < 0)
                from = 0;
            if (to < from)
                to = from;
            EditorGUILayout.BeginVertical();
            int i = 0;
            foreach (Tile tile in map.tiles)
            {
                if (i >= from && i <= to)
                {
                    EditorGUILayout.LabelField("Tile[" + i.ToString() + "] " + "Data", style, GUILayout.Height(50), GUILayout.ExpandWidth(true));
                    EditorGUILayout.Popup("State", (int)tile.TileState, tileStates);
                    EditorGUILayout.Vector2Field("Position", tile.Position);
                    EditorGUILayout.Toggle("Is Obstacle", tile.IsObstacle);
                    EditorGUILayout.LabelField("Conections: " + tile.Adjacents.Count.ToString());
                }
                i++;
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space();
        dotsFadeOut = EditorGUILayout.Foldout(dotsFadeOut, "Dots");
        if (dotsFadeOut)
        {
            SerializedProperty smallDots = serializedObject.FindProperty("smallDots");
            EditorGUILayout.PropertyField(smallDots, new GUIContent("Small Dots"), true);
            SerializedProperty bigDots = serializedObject.FindProperty("bigDots");
            EditorGUILayout.PropertyField(bigDots, new GUIContent("Big Dots"), true);
        }
        EditorGUILayout.Space();
        map.drawGizmos = EditorGUILayout.Toggle("Draw Gizmos", map.drawGizmos);
        if (map.drawGizmos)
        {
            EditorGUILayout.BeginHorizontal();
            map.drawGrid = EditorGUILayout.Toggle("Draw Grid", map.drawGrid);
            map.drawTileID = EditorGUILayout.Toggle("Draw Tile ID", map.drawTileID);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            map.drawTilesType = EditorGUILayout.Toggle("Draw Type Tiles", map.drawTilesType);
            if (map.drawTilesType)
                map.drawTilesConections = false;
            map.drawTilesConections = EditorGUILayout.Toggle("Draw Connection Tiles", map.drawTilesConections);
            if (map.drawTilesConections)
                map.drawTilesType = false;
            EditorGUILayout.EndHorizontal();
            if (map.drawTilesType)
            {
                SerializedProperty tileTypeColors = serializedObject.FindProperty("tileTypeColors");
                EditorGUILayout.PropertyField(tileTypeColors, new GUIContent("Tile Type Colors"), true);
            }
            if (map.drawTilesConections)
            {
                SerializedProperty tileConectionsColors = serializedObject.FindProperty("tileConectionsColors");
                EditorGUILayout.PropertyField(tileConectionsColors, new GUIContent("Tile Connections Colors"), true);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
