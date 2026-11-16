using UnityEngine;

/// <summary>Unity-facing seeded maze generator. The generated board is still editable until lock.</summary>
public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [Min(1)] public int width = 24;
    [Min(1)] public int height = 24;
    public int seed = 12345;
    [Min(1)] public int levelNumber = 1;

    [Header("Level Coordinates")]
    public Vector2Int start = new Vector2Int(0, 0);
    public Vector2Int goal = new Vector2Int(23, 23);

    [Header("References")]
    public MazeGenerator mazeGenerator;
    public GridManager gridManager;

    public GeneratedLevelData CurrentLevel { get; private set; }

    private void Start()
    {
        // GameFlowController may already have generated the first level. Do not overwrite it.
        if (CurrentLevel == null) GenerateLevel();
    }

    public void GenerateLevel()
    {
        GenerateLevel(levelNumber, seed);
    }

    public void GenerateLevel(int requestedLevelNumber, int requestedSeed)
    {
        CurrentLevel = null;
        if (mazeGenerator == null) mazeGenerator = FindObjectOfType<MazeGenerator>();
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (mazeGenerator == null || gridManager == null)
        {
            Debug.LogError("ProceduralLevelGenerator: MazeGenerator or GridManager reference is missing.");
            return;
        }
        if (width != 24 || height != 24)
        {
            Debug.LogError("ProceduralLevelGenerator: the first release requires a 24 x 24 board.");
            return;
        }

        levelNumber = requestedLevelNumber;
        seed = requestedSeed;
        if (start == goal || start.x < 0 || start.y < 0 || start.x >= width || start.y >= height ||
            goal.x < 0 || goal.y < 0 || goal.x >= width || goal.y >= height)
        {
            Debug.LogError("ProceduralLevelGenerator: start and goal must be distinct cells inside the board.");
            return;
        }

        bool[,] walls = mazeGenerator.GenerateMaze(width, height, seed, start, goal);
        if (walls == null || !mazeGenerator.HasSafePath(walls, start, goal))
        {
            Debug.LogError("Level generation failed: no safe route exists from start to goal.");
            CurrentLevel = null;
            return;
        }

        CurrentLevel = LevelBuilder.FromWalls(walls, levelNumber, seed, start, goal);
        gridManager.GenerateGrid(walls, start, goal);

        int wallCount = 0;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            if (walls[x, y]) wallCount++;

        Debug.Log($"Generated level {levelNumber} (seed {seed}, {width}x{height}, {wallCount} walls). BFS safe path: PASS.");
    }
}
