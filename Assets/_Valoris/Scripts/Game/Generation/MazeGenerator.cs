using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Generates deterministic base mazes while reserving a safe path between configured endpoints.</summary>
public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [Range(0f, 0.5f)]
    public float wallProbability = 0.25f;

    /// <summary>Compatibility overload: generates from the top-left to bottom-right corner.</summary>
    public bool[,] GenerateMaze(int width, int height, int seed)
        => GenerateMaze(width, height, seed, Vector2Int.zero, new Vector2Int(width - 1, height - 1));

    public bool[,] GenerateMaze(int width, int height, int seed, Vector2Int start, Vector2Int goal)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (!IsInsideGrid(start, width, height) || !IsInsideGrid(goal, width, height))
            throw new ArgumentException("Maze endpoints must be inside the generated grid.");
        if (start == goal)
            throw new ArgumentException("Maze start and goal must be different cells.");

        var random = new System.Random(seed);
        var walls = new bool[width, height];
        var safePath = new bool[width, height];

        int x = start.x;
        int y = start.y;
        safePath[x, y] = true;

        // Construct a seeded monotone route. The route always approaches the goal and
        // therefore cannot self-intersect or leave the board, regardless of endpoint choice.
        while (x != goal.x || y != goal.y)
        {
            bool canMoveX = x != goal.x;
            bool canMoveY = y != goal.y;
            bool moveX = canMoveX && (!canMoveY || random.Next(2) == 0);

            if (moveX) x += Math.Sign(goal.x - x);
            else y += Math.Sign(goal.y - y);

            safePath[x, y] = true;
        }

        double probability = Math.Max(0.0, Math.Min(0.5, wallProbability));
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (!safePath[col, row] && random.NextDouble() < probability)
                    walls[col, row] = true;
            }
        }

        return walls;
    }

    public bool HasSafePath(bool[,] walls, Vector2Int start, Vector2Int goal)
    {
        if (walls == null) return false;
        int width = walls.GetLength(0);
        int height = walls.GetLength(1);

        if (!IsInsideGrid(start, width, height) || !IsInsideGrid(goal, width, height) || start == goal)
            return false;
        if (walls[start.x, start.y] || walls[goal.x, goal.y])
            return false;

        var visited = new bool[width, height];
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == goal) return true;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;
                if (!IsInsideGrid(next, width, height)) continue;
                if (visited[next.x, next.y] || walls[next.x, next.y]) continue;
                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }
        return false;
    }

    private static bool IsInsideGrid(Vector2Int position, int width, int height)
        => position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
}
