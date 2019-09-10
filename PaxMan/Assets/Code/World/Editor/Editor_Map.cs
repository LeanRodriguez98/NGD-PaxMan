using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Map))]
public class Editor_Map : Editor
{
    private GUIStyle style;
    private string[] nodeStates;
    private int from;
    private int to;

    private bool gridFoldOut = false;
    private bool prefabsFoldOut = false;
    private bool cherrySettingsFoldOut = false;
    private bool nodesFoldOut = false;
    private bool dotsFadeOut = false;
    private void OnEnable()
    {
        nodeStates = new string[(int)Node.NodeStates._count];
        for (Node.NodeStates n = Node.NodeStates.Open; n < Node.NodeStates._count; n++)
        {
            nodeStates[(int)n] = n.ToString();
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
        nodesFoldOut = EditorGUILayout.Foldout(nodesFoldOut, "Nodes");
        if (nodesFoldOut)
        {
            from = EditorGUILayout.IntField("Show From", from);
            to = EditorGUILayout.IntField("To", to);
            if (from < 0)
                from = 0;
            if (to < from)
                to = from;
            EditorGUILayout.BeginVertical();
            int i = 0;
            foreach (Node n in map.nodes)
            {
                if (i >= from && i <= to)
                {
                    EditorGUILayout.LabelField("Node[" + i.ToString() + "] " + "Data", style, GUILayout.Height(50), GUILayout.ExpandWidth(true));
                    EditorGUILayout.Popup("State", (int)n.NodeState, nodeStates);
                    EditorGUILayout.Vector2Field("Position", n.Position);
                    EditorGUILayout.Toggle("Is Obstacle", n.IsObstacle);
                    EditorGUILayout.LabelField("Conections: " + n.Adjacents.Count.ToString());
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
            map.drawNodeID = EditorGUILayout.Toggle("Draw Node ID", map.drawNodeID);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            map.drawNodesType = EditorGUILayout.Toggle("Draw Type Nodes", map.drawNodesType);
            if (map.drawNodesType)
                map.drawNodesConections = false;
            map.drawNodesConections = EditorGUILayout.Toggle("Draw Connection Nodes", map.drawNodesConections);
            if (map.drawNodesConections)
                map.drawNodesType = false;
            EditorGUILayout.EndHorizontal();
            if (map.drawNodesType)
            {
                SerializedProperty nodeTypeColors = serializedObject.FindProperty("nodeTypeColors");
                EditorGUILayout.PropertyField(nodeTypeColors, new GUIContent("Node Type Colors"), true);
            }
            if (map.drawNodesConections)
            {
                SerializedProperty nodeConectionsColors = serializedObject.FindProperty("nodeConectionsColors");
                EditorGUILayout.PropertyField(nodeConectionsColors, new GUIContent("Node Connections Colors"), true);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
