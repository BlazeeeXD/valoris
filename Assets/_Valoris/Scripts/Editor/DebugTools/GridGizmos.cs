using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridGizmos : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GridManager grid = (GridManager)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid diagnostics", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Dimensions", grid.width + " x " + grid.height);
        if (grid.groundTilemap == null)
            EditorGUILayout.HelpBox("Ground Tilemap is not assigned.", MessageType.Warning);
    }
}
