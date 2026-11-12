using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;

namespace Valoris.Core.Grid
{
    /// <summary>Validates the 24x24 campaign level contract and checks for a safe BFS route.</summary>
    public sealed class LevelValidator : ILevelValidator
    {
        public LevelValidationResult Validate(LevelDefinition level)
        {
            var messages = new List<string>();
            if (level == null)
            {
                messages.Add("Level is null.");
                return Result(false, false, messages);
            }

            if (level.SchemaVersion != LevelDefinition.CurrentSchemaVersion)
                messages.Add("Unsupported schema version: " + level.SchemaVersion + ".");
            if (level.LevelNumber <= 0)
                messages.Add("Level number must be positive.");
            if (level.Width != LevelDefinition.CampaignWidth || level.Height != LevelDefinition.CampaignHeight)
                messages.Add("The first campaign release requires a 24 x 24 board.");
            if (level.Width <= 0 || level.Height <= 0)
                messages.Add("Width and height must be positive.");

            long expectedCount = (long)level.Width * level.Height;
            if (expectedCount != level.CellCount)
                messages.Add("Cell count must equal width multiplied by height.");

            if (messages.Count > 0)
                return Result(false, false, messages);

            int[] cells = level.CopyCells();
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] < (int)CellType.Empty || cells[i] > (int)CellType.Mud)
                {
                    messages.Add("Undefined cell value " + cells[i] + " at index " + i + ".");
                    break;
                }
            }

            if (!GridRules.IsInside(level.Start.X, level.Start.Y, level.Width, level.Height))
                messages.Add("Start coordinate is outside the board.");
            if (!GridRules.IsInside(level.Goal.X, level.Goal.Y, level.Width, level.Height))
                messages.Add("Goal coordinate is outside the board.");
            if (level.Start == level.Goal)
                messages.Add("Start and goal must be different coordinates.");

            if (messages.Count > 0)
                return Result(false, false, messages);

            if (level.GetCell(level.Start.X, level.Start.Y) != CellType.Empty)
                messages.Add("Start coordinate must be on an Empty cell.");
            if (level.GetCell(level.Goal.X, level.Goal.Y) != CellType.Empty)
                messages.Add("Goal coordinate must be on an Empty cell.");

            if (messages.Count > 0)
                return Result(false, false, messages);

            bool hasSafePath = HasSafePath(level);
            if (!hasSafePath)
                messages.Add("No safe route exists from start to goal (walls and traps are blocked by validation).");

            return Result(messages.Count == 0, hasSafePath, messages);
        }

        private static bool HasSafePath(LevelDefinition level)
        {
            int width = level.Width;
            int height = level.Height;
            var visited = new bool[width * height];
            var queue = new Queue<GridPosition>();
            queue.Enqueue(level.Start);
            visited[level.Start.Y * width + level.Start.X] = true;

            MoveAction[] actions = { MoveAction.Up, MoveAction.Right, MoveAction.Down, MoveAction.Left };
            while (queue.Count > 0)
            {
                GridPosition current = queue.Dequeue();
                if (current == level.Goal) return true;

                for (int i = 0; i < actions.Length; i++)
                {
                    GridPosition next = GridRules.Apply(current, actions[i]);
                    if (!GridRules.IsInside(next.X, next.Y, width, height)) continue;
                    int index = next.Y * width + next.X;
                    if (visited[index]) continue;
                    CellType cell = level.GetCell(next.X, next.Y);
                    if (!GridRules.IsSafeTraversable(cell)) continue;
                    visited[index] = true;
                    queue.Enqueue(next);
                }
            }
            return false;
        }

        private static LevelValidationResult Result(bool valid, bool hasPath, List<string> messages)
        {
            return new LevelValidationResult
            {
                IsValid = valid,
                HasSafePath = hasPath,
                Messages = messages.AsReadOnly()
            };
        }
    }
}
