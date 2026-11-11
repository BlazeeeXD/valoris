
using System;
using UnityEngine;

[Serializable]
public class GeneratedLevelData
{
    public int schemaVersion = 1;
    public int levelNumber = 1;
    public int seed;

    public int width;
    public int height;

    public byte[] cells;

    // Tracks which cells contain player-placed sabotage.
    public bool[] playerPlacedItems;

    public Vector2Int start;
    public Vector2Int goal;

    public GeneratedLevelData(
        int width,
        int height,
        int seed,
        Vector2Int start,
        Vector2Int goal)
    {
        this.width = width;
        this.height = height;
        this.seed = seed;
        this.start = start;
        this.goal = goal;

        cells = new byte[width * height];
        playerPlacedItems = new bool[width * height];
    }

    public int GetIndex(int x, int y)
    {
        return y * width + x;
    }

    public byte GetCell(int x, int y)
    {
        return cells[GetIndex(x, y)];
    }

    public void SetCell(int x, int y, byte value)
    {
        cells[GetIndex(x, y)] = value;
    }

    public void MarkPlayerPlaced(int x, int y)
    {
        playerPlacedItems[GetIndex(x, y)] = true;
    }

    public bool IsPlayerPlaced(int x, int y)
    {
        return playerPlacedItems[GetIndex(x, y)];
    }

    public void ClearPlayerPlaced(int x, int y)
    {
        playerPlacedItems[GetIndex(x, y)] = false;
    }
}
