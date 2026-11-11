
using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int width = 24;
    public int height = 24;
    public int seed = 12345;
    public int levelNumber = 1;

    [Header("Level Coordinates")]
    public Vector2Int start = new Vector2Int(0, 0);
    public Vector2Int goal = new Vector2Int(23, 23);

    [Header("References")]
    public MazeGenerator mazeGenerator;
    public GridManager gridManager;

    public GeneratedLevelData CurrentLevel { get; private set; }

    private void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        if (mazeGenerator == null || gridManager == null)
        {
            Debug.LogError("ProceduralLevelGenerator: Missing references!");
            return;
        }

        bool[,] walls = mazeGenerator.GenerateMaze(width, height, seed);

        if (!mazeGenerator.HasSafePath(walls, start, goal))
        {
            Debug.LogError("Level generation failed: No safe path exists!");
            return;
        }

        CurrentLevel = new GeneratedLevelData(
            width, height, seed, start, goal
        );

        CurrentLevel.levelNumber = levelNumber;

        int wallCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte cellValue = walls[x, y] ? (byte)1 : (byte)0;

                CurrentLevel.SetCell(x, y, cellValue);

                if (walls[x, y])
                    wallCount++;
            }
        }

        gridManager.GenerateGrid(walls, start, goal);

        Debug.Log("BFS validation successful.");
        Debug.Log($"Level: {CurrentLevel.levelNumber}");
        Debug.Log($"Seed: {CurrentLevel.seed}");
        Debug.Log($"Dimensions: {width}x{height}");
        Debug.Log($"Walls: {wallCount}");
        Debug.Log($"Start: {CurrentLevel.start}");
        Debug.Log($"Goal: {CurrentLevel.goal}");
        Debug.Log($"Cell data count: {CurrentLevel.cells.Length}");
    }
}
