using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    /// <summary>
    /// Encodes the agent's absolute tile position plus its local terrain and goal context.
    /// Position is essential for a fixed level: it prevents visually identical intersections
    /// at different places from being collapsed into one state. Local context makes reuse
    /// across generated levels more conservative by distinguishing changed nearby terrain.
    /// </summary>
    public sealed class StateEncoder
    {
        public const int NeighbourPatternCount = 4 * 4 * 4 * 4;
        public const int GoalContextCount = 3 * 3 * 3;
        public const int LocalStateCount = NeighbourPatternCount * GoalContextCount;
        public const int StateCount = LevelDefinition.CampaignWidth
                                      * LevelDefinition.CampaignHeight
                                      * LocalStateCount;

        public int Encode(GridObservation observation)
        {
            GridPosition position = observation.Position;
            if (position.X < 0 || position.Y < 0
                || position.X >= LevelDefinition.CampaignWidth
                || position.Y >= LevelDefinition.CampaignHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(observation),
                    "Observation position is outside the 24 x 24 campaign grid.");
            }

            int code = EncodeNeighbourhood(observation);

            int dx = Sign(observation.Goal.X - position.X);
            int dy = Sign(observation.Goal.Y - position.Y);
            int distance = ManhattanDistance(position, observation.Goal);
            int distanceBand = distance <= 3 ? 0 : distance <= 8 ? 1 : 2;

            code = code * 3 + (dx + 1);
            code = code * 3 + (dy + 1);
            code = code * 3 + distanceBand;

            int positionIndex = position.Y * LevelDefinition.CampaignWidth + position.X;
            return positionIndex * LocalStateCount + code;
        }

        private static int EncodeNeighbourhood(GridObservation observation)
        {
            ValidateCell(observation.North);
            ValidateCell(observation.East);
            ValidateCell(observation.South);
            ValidateCell(observation.West);

            int code = (int)observation.North;
            code = code * 4 + (int)observation.East;
            code = code * 4 + (int)observation.South;
            code = code * 4 + (int)observation.West;
            return code;
        }

        private static void ValidateCell(CellType cell)
        {
            if ((byte)cell > (byte)CellType.Mud)
                throw new ArgumentOutOfRangeException(nameof(cell), "Unknown cell type in observation.");
        }

        private static int Sign(int value) => value < 0 ? -1 : value > 0 ? 1 : 0;

        private static int ManhattanDistance(GridPosition a, GridPosition b)
            => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
