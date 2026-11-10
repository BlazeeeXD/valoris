using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    /// <summary>
    /// Encodes four neighbouring cell types, goal-relative X/Y direction and goal-distance band.
    /// This supports transfer of local preferences; it does not eliminate state aliasing.
    /// </summary>
    public sealed class StateEncoder
    {
        public const int StateCount = 4 * 4 * 4 * 4 * 3 * 3 * 3;

        public int Encode(GridObservation observation)
        {
            int code = (int)observation.North;
            code = code * 4 + (int)observation.East;
            code = code * 4 + (int)observation.South;
            code = code * 4 + (int)observation.West;

            int dx = Sign(observation.Goal.X - observation.Position.X);
            int dy = Sign(observation.Goal.Y - observation.Position.Y);
            int distance = ManhattanDistance(observation.Position, observation.Goal);
            int distanceBand = distance <= 3 ? 0 : distance <= 8 ? 1 : 2;

            code = code * 3 + (dx + 1);
            code = code * 3 + (dy + 1);
            code = code * 3 + distanceBand;
            return code;
        }

        private static int Sign(int value) => value < 0 ? -1 : value > 0 ? 1 : 0;
        private static int ManhattanDistance(GridPosition a, GridPosition b)
            => System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y);
    }
}
