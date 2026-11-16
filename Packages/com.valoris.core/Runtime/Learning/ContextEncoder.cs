
using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    /// <summary>
    /// Encodes reusable environmental context, including nearby wall
    /// ownership and Wall Breaker availability, without absolute coordinates.
    /// </summary>
    public sealed class ContextEncoder
    {
        public const int NeighbourPatternCount =
            4 * 4 * 4 * 4 * 2 * 2 * 2 * 2 * 2;

        public const int GoalContextCount = 3 * 3 * 3;

        public const int StateCount =
            NeighbourPatternCount * GoalContextCount;

        public int Encode(GridObservation observation)
        {
            ValidatePosition(observation.Position, nameof(observation));
            ValidatePosition(observation.Goal, nameof(observation));

            ValidateCell(observation.North);
            ValidateCell(observation.East);
            ValidateCell(observation.South);
            ValidateCell(observation.West);

            int code = (int)observation.North;

            code = code * 4 + (int)observation.East;
            code = code * 4 + (int)observation.South;
            code = code * 4 + (int)observation.West;

            code = code * 2 + Flag(observation.NorthIsPlayerWall);
            code = code * 2 + Flag(observation.EastIsPlayerWall);
            code = code * 2 + Flag(observation.SouthIsPlayerWall);
            code = code * 2 + Flag(observation.WestIsPlayerWall);

            code = code * 2 + Flag(observation.WallBreakerAvailable);

            int dx = Sign(
                observation.Goal.X - observation.Position.X);

            int dy = Sign(
                observation.Goal.Y - observation.Position.Y);

            int distance =
                Math.Abs(observation.Goal.X - observation.Position.X)
                + Math.Abs(observation.Goal.Y - observation.Position.Y);

            int distanceBand =
                distance <= 3 ? 0 :
                distance <= 8 ? 1 : 2;

            code = code * 3 + (dx + 1);
            code = code * 3 + (dy + 1);
            code = code * 3 + distanceBand;

            return code;
        }

        private static int Flag(bool value)
        {
            return value ? 1 : 0;
        }

        private static void ValidatePosition(
            GridPosition position,
            string name)
        {
            if (position.X < 0 || position.Y < 0 ||
                position.X >= LevelDefinition.CampaignWidth ||
                position.Y >= LevelDefinition.CampaignHeight)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    "Observation coordinates must be inside the 24 x 24 campaign grid.");
            }
        }

        private static int Sign(int value)
        {
            return value < 0 ? -1 : value > 0 ? 1 : 0;
        }

        private static void ValidateCell(CellType cell)
        {
            if ((byte)cell > (byte)CellType.Mud)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cell),
                    "Unknown cell type.");
            }
        }
    }
}
