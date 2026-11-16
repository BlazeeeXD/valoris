
using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    /// <summary>
    /// Provides the legal action mask for movement and Wall Breaker actions.
    /// </summary>
    public static class ActionAvailability
    {
        public const int ActionCount = 8;

        public static bool IsAvailable(
            GridObservation observation,
            int action)
        {
            switch ((MoveAction)action)
            {
                // Normal movement
                case MoveAction.Up:
                    return observation.North != CellType.Wall;

                case MoveAction.Right:
                    return observation.East != CellType.Wall;

                case MoveAction.Down:
                    return observation.South != CellType.Wall;

                case MoveAction.Left:
                    return observation.West != CellType.Wall;

                // Wall Breaker actions
                case MoveAction.BreakUp:
                    return observation.WallBreakerAvailable &&
                           observation.NorthIsPlayerWall;

                case MoveAction.BreakRight:
                    return observation.WallBreakerAvailable &&
                           observation.EastIsPlayerWall;

                case MoveAction.BreakDown:
                    return observation.WallBreakerAvailable &&
                           observation.SouthIsPlayerWall;

                case MoveAction.BreakLeft:
                    return observation.WallBreakerAvailable &&
                           observation.WestIsPlayerWall;

                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        public static int CountAvailable(GridObservation observation)
        {
            int count = 0;

            for (int action = 0; action < ActionCount; action++)
            {
                if (IsAvailable(observation, action))
                    count++;
            }

            return count;
        }

        public static int GetAvailableAt(
            GridObservation observation,
            int availableIndex)
        {
            if (availableIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(availableIndex));

            int current = 0;

            for (int action = 0; action < ActionCount; action++)
            {
                if (!IsAvailable(observation, action))
                    continue;

                if (current == availableIndex)
                    return action;

                current++;
            }

            throw new ArgumentOutOfRangeException(nameof(availableIndex));
        }
    }
}
