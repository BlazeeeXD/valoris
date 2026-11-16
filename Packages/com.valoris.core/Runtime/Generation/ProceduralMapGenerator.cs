using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;

namespace Valoris.Core.Generation
{
    public sealed class ProceduralMapGenerator
    {
        private static readonly MoveAction[] Actions =
        {
            MoveAction.Up,
            MoveAction.Right,
            MoveAction.Down,
            MoveAction.Left
        };

        private readonly LevelValidator _validator =
            new LevelValidator();

        public MapGenerationResult Generate(MapGenerationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            int width = options.Width;
            int height = options.Height;

            var random = new Random(options.Seed);
            var cells = new int[width * height];

            FillWalls(cells);

            CarvePerfectMaze(cells, width, height, random);

            GridPosition start = new GridPosition(1, 1);
            GridPosition goal = new GridPosition(width - 2, height - 2);

            // Connect the final maze cell to the goal.
            SetCell(cells, width, goal.X, goal.Y - 1, CellType.Empty);
            SetCell(cells, width, goal.X, goal.Y, CellType.Empty);

            int defaultPassages;
            int defaultTraps;
            int defaultMud;

            GetDifficultyDefaults(
                options.Difficulty,
                out defaultPassages,
                out defaultTraps,
                out defaultMud);

            int requestedPassages =
                options.AdditionalPassages >= 0
                    ? options.AdditionalPassages
                    : defaultPassages;

            int requestedTraps =
                options.TrapCount >= 0
                    ? options.TrapCount
                    : defaultTraps;

            int requestedMud =
                options.MudCount >= 0
                    ? options.MudCount
                    : defaultMud;

            int openedPassages = OpenAdditionalPassages(
                cells,
                width,
                height,
                requestedPassages,
                random);

            // Find one safe route before placing hazards.
            var baseLevel = CreateLevel(
                options,
                cells,
                start,
                goal);

            bool[] protectedPath = FindSafePathMask(baseLevel);

            if (protectedPath == null)
                throw new InvalidOperationException(
                    "Generated maze has no safe route.");

            // Traps cannot occupy the protected route.
            int actualTraps = PlaceTraps(
                cells,
                width,
                height,
                protectedPath,
                start,
                goal,
                requestedTraps,
                random);

            // Mud is traversable, so it may occupy the safe route.
            int actualMud = PlaceMud(
                cells,
                width,
                height,
                start,
                goal,
                requestedMud,
                random);

            var finalLevel = CreateLevel(
                options,
                cells,
                start,
                goal);

            LevelValidationResult validation =
                _validator.Validate(finalLevel);

            if (!validation.IsValid || !validation.HasSafePath)
            {
                throw new InvalidOperationException(
                    "Generated level failed validation: "
                    + string.Join("; ", validation.Messages));
            }

            return new MapGenerationResult(
                finalLevel,
                options.Difficulty,
                openedPassages,
                actualTraps,
                actualMud,
                validation);
        }

        private static void FillWalls(int[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
                cells[i] = (int)CellType.Wall;
        }

        private static void SetCell(
            int[] cells,
            int width,
            int x,
            int y,
            CellType type)
        {
            cells[y * width + x] = (int)type;
        }

        private static void CarvePerfectMaze(
            int[] cells,
            int width,
            int height,
            Random random)
        {
            int columns = (width - 2) / 2;
            int rows = (height - 2) / 2;

            bool[,] visited = new bool[columns, rows];

            var stack = new List<GridPosition>();

            stack.Add(new GridPosition(0, 0));
            visited[0, 0] = true;

            SetCell(cells, width, 1, 1, CellType.Empty);

            while (stack.Count > 0)
            {
                GridPosition current = stack[stack.Count - 1];

                var neighbours = new List<GridPosition>();

                for (int i = 0; i < Actions.Length; i++)
                {
                    int nx = current.X;
                    int ny = current.Y;

                    switch (Actions[i])
                    {
                        case MoveAction.Up: ny--; break;
                        case MoveAction.Right: nx++; break;
                        case MoveAction.Down: ny++; break;
                        case MoveAction.Left: nx--; break;
                    }

                    if (nx < 0 || ny < 0 ||
                        nx >= columns || ny >= rows)
                        continue;

                    if (!visited[nx, ny])
                        neighbours.Add(new GridPosition(nx, ny));
                }

                // Dead end: backtrack.
                if (neighbours.Count == 0)
                {
                    stack.RemoveAt(stack.Count - 1);
                    continue;
                }

                GridPosition next =
                    neighbours[random.Next(neighbours.Count)];

                int currentX = 1 + current.X * 2;
                int currentY = 1 + current.Y * 2;

                int nextX = 1 + next.X * 2;
                int nextY = 1 + next.Y * 2;

                // Open the connecting corridor.
                int middleX = (currentX + nextX) / 2;
                int middleY = (currentY + nextY) / 2;

                SetCell(cells, width,
                    middleX, middleY, CellType.Empty);

                SetCell(cells, width,
                    nextX, nextY, CellType.Empty);

                visited[next.X, next.Y] = true;
                stack.Add(next);
            }
        }

        private static int OpenAdditionalPassages(
            int[] cells,
            int width,
            int height,
            int requested,
            Random random)
        {
            var candidates = new List<int>();

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = y * width + x;

                    if (cells[index] != (int)CellType.Wall)
                        continue;

                    bool horizontal =
                        cells[index - 1] == (int)CellType.Empty &&
                        cells[index + 1] == (int)CellType.Empty;

                    bool vertical =
                        cells[index - width] == (int)CellType.Empty &&
                        cells[index + width] == (int)CellType.Empty;

                    if (horizontal || vertical)
                        candidates.Add(index);
                }
            }

            Shuffle(candidates, random);

            int count = Math.Min(requested, candidates.Count);

            for (int i = 0; i < count; i++)
                cells[candidates[i]] = (int)CellType.Empty;

            return count;
        }

        private static bool[] FindSafePathMask(LevelDefinition level)
        {
            int width = level.Width;
            int height = level.Height;
            int total = width * height;

            int start = level.Start.Y * width + level.Start.X;
            int goal = level.Goal.Y * width + level.Goal.X;

            var parent = new int[total];

            for (int i = 0; i < total; i++)
                parent[i] = -2;

            var queue = new Queue<int>();

            queue.Enqueue(start);
            parent[start] = -1;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                if (current == goal)
                    break;

                var position = new GridPosition(
                    current % width,
                    current / width);

                for (int i = 0; i < Actions.Length; i++)
                {
                    GridPosition next =
                        GridRules.Apply(position, Actions[i]);

                    if (!GridRules.IsInside(
                        next.X, next.Y, width, height))
                        continue;

                    int nextIndex = next.Y * width + next.X;

                    if (parent[nextIndex] != -2)
                        continue;

                    CellType cell = level.GetCell(
                        next.X, next.Y);

                    if (cell == CellType.Wall ||
                        cell == CellType.Trap)
                        continue;

                    parent[nextIndex] = current;
                    queue.Enqueue(nextIndex);
                }
            }

            if (parent[goal] == -2)
                return null;

            var protectedPath = new bool[total];

            int cursor = goal;

            while (cursor != -1)
            {
                protectedPath[cursor] = true;
                cursor = parent[cursor];
            }

            return protectedPath;
        }

        private static int PlaceTraps(
            int[] cells,
            int width,
            int height,
            bool[] protectedPath,
            GridPosition start,
            GridPosition goal,
            int requested,
            Random random)
        {
            var candidates = new List<int>();

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != (int)CellType.Empty)
                    continue;

                if (protectedPath[i])
                    continue;

                int x = i % width;
                int y = i / width;

                if ((x == start.X && y == start.Y) ||
                    (x == goal.X && y == goal.Y))
                    continue;

                candidates.Add(i);
            }

            Shuffle(candidates, random);

            int count = Math.Min(requested, candidates.Count);

            for (int i = 0; i < count; i++)
                cells[candidates[i]] = (int)CellType.Trap;

            return count;
        }

        private static int PlaceMud(
            int[] cells,
            int width,
            int height,
            GridPosition start,
            GridPosition goal,
            int requested,
            Random random)
        {
            var candidates = new List<int>();

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != (int)CellType.Empty)
                    continue;

                int x = i % width;
                int y = i / width;

                if ((x == start.X && y == start.Y) ||
                    (x == goal.X && y == goal.Y))
                    continue;

                candidates.Add(i);
            }

            Shuffle(candidates, random);

            int count = Math.Min(requested, candidates.Count);

            for (int i = 0; i < count; i++)
                cells[candidates[i]] = (int)CellType.Mud;

            return count;
        }

        private static void Shuffle<T>(List<T> items, Random random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                T temporary = items[i];
                items[i] = items[j];
                items[j] = temporary;
            }
        }

        private static LevelDefinition CreateLevel(
            MapGenerationOptions options,
            int[] cells,
            GridPosition start,
            GridPosition goal)
        {
            return new LevelDefinition(
                LevelDefinition.CurrentSchemaVersion,
                options.LevelNumber,
                options.Seed,
                options.Width,
                options.Height,
                cells,
                start,
                goal);
        }

        private static void GetDifficultyDefaults(
            MapDifficulty difficulty,
            out int passages,
            out int traps,
            out int mud)
        {
            switch (difficulty)
            {
                case MapDifficulty.Easy:
                    passages = 30;
                    traps = 0;
                    mud = 0;
                    break;

                case MapDifficulty.Medium:
                    passages = 14;
                    traps = 6;
                    mud = 8;
                    break;

                case MapDifficulty.Hard:
                    passages = 4;
                    traps = 12;
                    mud = 16;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(difficulty));
            }
        }
    }
}