using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    /// <summary>Provides a shared legal-action mask. Walls and out-of-bounds cells are never selected.</summary>
    public static class ActionAvailability
    {
        public static bool IsAvailable(GridObservation observation, int action)
        {
            switch ((MoveAction)action)
            {
                case MoveAction.Up: return observation.North != CellType.Wall;
                case MoveAction.Right: return observation.East != CellType.Wall;
                case MoveAction.Down: return observation.South != CellType.Wall;
                case MoveAction.Left: return observation.West != CellType.Wall;
                default: throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        public static int CountAvailable(GridObservation observation)
        {
            int count = 0;
            for (int action = 0; action < 4; action++)
                if (IsAvailable(observation, action)) count++;
            return count;
        }

        public static int GetAvailableAt(GridObservation observation, int availableIndex)
        {
            if (availableIndex < 0) throw new ArgumentOutOfRangeException(nameof(availableIndex));
            int current = 0;
            for (int action = 0; action < 4; action++)
            {
                if (!IsAvailable(observation, action)) continue;
                if (current == availableIndex) return action;
                current++;
            }
            throw new ArgumentOutOfRangeException(nameof(availableIndex));
        }
    }
}
