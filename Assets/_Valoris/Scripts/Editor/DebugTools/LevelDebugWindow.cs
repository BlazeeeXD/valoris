using UnityEditor;
using UnityEngine;

public class LevelDebugWindow : EditorWindow
{
    private ProceduralLevelGenerator _generator;
    private Vector2 _scroll;

    [MenuItem("Oralis/Level Debugger")]
    public static void Open() => GetWindow<LevelDebugWindow>("Oralis Level Debugger");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Debugger", EditorStyles.boldLabel);
        _generator = (ProceduralLevelGenerator)EditorGUILayout.ObjectField("Generator", _generator, typeof(ProceduralLevelGenerator), true);
        if (GUILayout.Button("Find Active Generator")) _generator = UnityEngine.Object.FindObjectOfType<ProceduralLevelGenerator>();
        if (_generator == null || _generator.CurrentLevel == null)
        {
            EditorGUILayout.HelpBox("Enter Play Mode and select an active generated level.", MessageType.Info);
            return;
        }
        GeneratedLevelData level = _generator.CurrentLevel;
        EditorGUILayout.LabelField("Level", level.levelNumber.ToString());
        EditorGUILayout.LabelField("Seed", level.seed.ToString());
        EditorGUILayout.LabelField("Grid", level.width + " x " + level.height);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        for (int y = 0; y < level.height; y++)
        {
            using (new EditorGUILayout.HorizontalScope())
            for (int x = 0; x < level.width; x++)
            {
                byte cell = level.GetCell(x, y);
                string symbol = cell == 1 ? "#" : cell == 2 ? "T" : cell == 3 ? "M" : ".";
                if (level.start == new Vector2Int(x, y)) symbol = "S";
                if (level.goal == new Vector2Int(x, y)) symbol = "G";
                GUILayout.Label(symbol, GUILayout.Width(16));
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
