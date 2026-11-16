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
    public bool[] playerPlacedItems;
    public Vector2Int start;
    public Vector2Int goal;

    public GeneratedLevelData(int width, int height, int seed, Vector2Int start, Vector2Int goal)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        this.width = width;
        this.height = height;
        this.seed = seed;
        this.start = start;
        this.goal = goal;
        cells = new byte[checked(width * height)];
        playerPlacedItems = new bool[cells.Length];
    }

    public int GetIndex(int x, int y)
    {
        EnsureInside(x, y);
        return y * width + x;
    }

    public byte GetCell(int x, int y) => cells[GetIndex(x, y)];

    public void SetCell(int x, int y, byte value)
    {
        if (value > 3) throw new ArgumentOutOfRangeException(nameof(value), "Cell type must be 0..3.");
        cells[GetIndex(x, y)] = value;
    }

    public void MarkPlayerPlaced(int x, int y) => playerPlacedItems[GetIndex(x, y)] = true;
    public bool IsPlayerPlaced(int x, int y) => playerPlacedItems[GetIndex(x, y)];
    public void ClearPlayerPlaced(int x, int y) => playerPlacedItems[GetIndex(x, y)] = false;

    public GeneratedLevelData DeepCopy()
    {
        var copy = new GeneratedLevelData(width, height, seed, start, goal)
        {
            schemaVersion = schemaVersion,
            levelNumber = levelNumber,
            cells = (byte[])cells.Clone(),
            playerPlacedItems = (bool[])playerPlacedItems.Clone()
        };
        return copy;
    }

    private void EnsureInside(int x, int y)
    {
        if (cells == null || cells.Length != width * height)
            throw new InvalidOperationException("Generated level cell storage is invalid.");
        if (x < 0 || y < 0 || x >= width || y >= height)
            throw new ArgumentOutOfRangeException("Coordinates are outside the generated level.");
    }
}
