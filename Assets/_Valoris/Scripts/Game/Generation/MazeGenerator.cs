
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0)
    };

    // Percentage of remaining internal walls to open.
    private const float LoopOpeningFraction = 0.25f;

    public bool[,] GenerateMaze(int width, int height, int seed)
    {
        if (width < 5 || height < 5)
        {
            Debug.LogError("Maze dimensions must be at least 5x5.");
            return null;
        }

        System.Random random = new System.Random(seed);

        // True represents a wall.
        bool[,] walls = new bool[width, height];

        // Start with every cell as a wall.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                walls[x, y] = true;
            }
        }

        // Logical maze dimensions.
        int columns = (width - 2) / 2;
        int rows = (height - 2) / 2;

        bool[,] visited = new bool[columns, rows];

        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // Begin carving from the first logical cell.
        Vector2Int start = new Vector2Int(0, 0);

        stack.Push(start);
        visited[start.x, start.y] = true;

        CarveCell(walls, start);

        // Randomized Depth First Search.
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();

            List<Vector2Int> neighbours = new List<Vector2Int>();

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next = current + direction;

                if (next.x < 0 || next.y < 0 ||
                    next.x >= columns || next.y >= rows)
                    continue;

                if (!visited[next.x, next.y])
                    neighbours.Add(next);
            }

            // Dead end: backtrack.
            if (neighbours.Count == 0)
            {
                stack.Pop();
                continue;
            }

            // Pick a random unvisited neighbour.
            Vector2Int chosen =
                neighbours[random.Next(neighbours.Count)];

            Vector2Int currentGrid = ToGridPosition(current);
            Vector2Int nextGrid = ToGridPosition(chosen);

            // Carve the connecting corridor.
            Vector2Int middle = new Vector2Int(
                (currentGrid.x + nextGrid.x) / 2,
                (currentGrid.y + nextGrid.y) / 2
            );

            walls[middle.x, middle.y] = false;

            // Carve the new cell.
            CarveCell(walls, chosen);

            visited[chosen.x, chosen.y] = true;
            stack.Push(chosen);
        }

        // Add loops and alternative routes.
        AddLoops(walls, columns, rows, random);

        // Entrance connecting to the top-left corner.
        walls[0, 0] = false;
        walls[1, 0] = false;
        walls[1, 1] = false;

        // Exit connecting the final maze cell to bottom-right.
        Vector2Int lastCell = ToGridPosition(
            new Vector2Int(columns - 1, rows - 1)
        );

        for (int x = lastCell.x; x < width; x++)
            walls[x, lastCell.y] = false;

        for (int y = lastCell.y; y < height; y++)
            walls[width - 1, y] = false;

        Debug.Log($"DFS maze generated with loops. Seed: {seed}");

        return walls;
    }

    private void AddLoops(
        bool[,] walls,
        int columns,
        int rows,
        System.Random random)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        // Find closed connections between horizontally
        // and vertically adjacent logical cells.
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2Int current = ToGridPosition(
                    new Vector2Int(x, y)
                );

                // Check connection to the right.
                if (x + 1 < columns)
                {
                    Vector2Int right = ToGridPosition(
                        new Vector2Int(x + 1, y)
                    );

                    Vector2Int middle = new Vector2Int(
                        (current.x + right.x) / 2,
                        (current.y + right.y) / 2
                    );

                    if (walls[middle.x, middle.y])
                        candidates.Add(middle);
                }

                // Check connection below.
                if (y + 1 < rows)
                {
                    Vector2Int below = ToGridPosition(
                        new Vector2Int(x, y + 1)
                    );

                    Vector2Int middle = new Vector2Int(
                        (current.x + below.x) / 2,
                        (current.y + below.y) / 2
                    );

                    if (walls[middle.x, middle.y])
                        candidates.Add(middle);
                }
            }
        }

        // Shuffle candidates using Fisher-Yates.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);

            Vector2Int temp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = temp;
        }

        // Open approximately 25% of remaining connections.
        int loopsToOpen = Mathf.RoundToInt(
            candidates.Count * LoopOpeningFraction
        );

        for (int i = 0; i < loopsToOpen; i++)
        {
            Vector2Int position = candidates[i];
            walls[position.x, position.y] = false;
        }

        Debug.Log(
            $"Maze loops added: {loopsToOpen}/{candidates.Count} possible connections."
        );
    }

    private void CarveCell(bool[,] walls, Vector2Int logicalPosition)
    {
        Vector2Int gridPosition = ToGridPosition(logicalPosition);

        walls[gridPosition.x, gridPosition.y] = false;
    }

    private Vector2Int ToGridPosition(Vector2Int logicalPosition)
    {
        return new Vector2Int(
            1 + logicalPosition.x * 2,
            1 + logicalPosition.y * 2
        );
    }

    public bool HasSafePath(
        bool[,] walls,
        Vector2Int start,
        Vector2Int goal)
    {
        if (walls == null)
            return false;

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

    private bool IsInsideGrid(
        Vector2Int position,
        int width,
        int height)
    {
        return position.x >= 0 && position.x < width &&
               position.y >= 0 && position.y < height;
    }
}
