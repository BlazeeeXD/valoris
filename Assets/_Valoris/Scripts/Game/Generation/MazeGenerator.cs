
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [Range(0f, 0.5f)]
    public float wallProbability = 0.25f;

    public bool[,] GenerateMaze(int width, int height, int seed)
    {
        System.Random random = new System.Random(seed);

        bool[,] walls = new bool[width, height];
        bool[,] safePath = new bool[width, height];

        int x = 0;
        int y = 0;

        safePath[x, y] = true;

        while (x < width - 1 || y < height - 1)
        {
            bool moveRight = x < width - 1;
            bool moveDown = y < height - 1;

            if (moveRight && moveDown)
            {
                if (random.Next(2) == 0)
                    x++;
                else
                    y++;
            }
            else if (moveRight)
            {
                x++;
            }
            else
            {
                y++;
            }

            safePath[x, y] = true;
        }

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (!safePath[col, row] &&
                    random.NextDouble() < wallProbability)
                {
                    walls[col, row] = true;
                }
            }
        }

        Debug.Log($"Maze generated using seed: {seed}");

        return walls;
    }

    public bool HasSafePath(
        bool[,] walls,
        Vector2Int start,
        Vector2Int goal)
    {
        int width = walls.GetLength(0);
        int height = walls.GetLength(1);

        if (!IsInsideGrid(start, width, height) ||
            !IsInsideGrid(goal, width, height))
            return false;

        if (walls[start.x, start.y] || walls[goal.x, goal.y])
            return false;

        bool[,] visited = new bool[width, height];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

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

            if (current == goal)
                return true;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (!IsInsideGrid(next, width, height))
                    continue;

                if (visited[next.x, next.y] || walls[next.x, next.y])
                    continue;

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return false;
    }

    private bool IsInsideGrid(Vector2Int position, int width, int height)
    {
        return position.x >= 0 && position.x < width &&
               position.y >= 0 && position.y < height;
    }
}
