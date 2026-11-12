using System;
using UnityEngine;

/// <summary>Builds the Unity-side mutable level data from a generated wall grid.</summary>
public static class LevelBuilder
{
    public static GeneratedLevelData FromWalls(
        bool[,] walls,
        int levelNumber,
        int seed,
        Vector2Int start,
        Vector2Int goal)
    {
        if (walls == null) throw new ArgumentNullException(nameof(walls));
        int width = walls.GetLength(0);
        int height = walls.GetLength(1);
        if (width != 24 || height != 24)
            throw new ArgumentException("The campaign board must be 24 x 24.", nameof(walls));
        if (!Inside(start, width, height) || !Inside(goal, width, height) || start == goal)
            throw new ArgumentException("Start and goal must be distinct cells inside the board.");
        if (walls[start.x, start.y] || walls[goal.x, goal.y])
            throw new ArgumentException("Start and goal cannot be walls.");

        var data = new GeneratedLevelData(width, height, seed, start, goal)
        {
            levelNumber = levelNumber,
            schemaVersion = 1
        };

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            data.SetCell(x, y, walls[x, y] ? (byte)1 : (byte)0);

        return data;
    }

    private static bool Inside(Vector2Int p, int width, int height)
        => p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
}
