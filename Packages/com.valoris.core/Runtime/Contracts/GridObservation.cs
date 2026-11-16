
namespace Valoris.Core.Contracts
{
    /// <summary>
    /// Agent observation containing nearby terrain, wall ownership,
    /// and Wall Breaker availability.
    /// </summary>
    public readonly struct GridObservation
    {
        public GridPosition Position { get; }
        public GridPosition Goal { get; }

        public CellType North { get; }
        public CellType East { get; }
        public CellType South { get; }
        public CellType West { get; }

        public bool NorthIsPlayerWall { get; }
        public bool EastIsPlayerWall { get; }
        public bool SouthIsPlayerWall { get; }
        public bool WestIsPlayerWall { get; }

        public bool WallBreakerAvailable { get; }

        public GridObservation(
            GridPosition position,
            GridPosition goal,
            CellType north,
            CellType east,
            CellType south,
            CellType west,
            bool northIsPlayerWall = false,
            bool eastIsPlayerWall = false,
            bool southIsPlayerWall = false,
            bool westIsPlayerWall = false,
            bool wallBreakerAvailable = false)
        {
            Position = position;
            Goal = goal;

            North = north;
            East = east;
            South = south;
            West = west;

            NorthIsPlayerWall = northIsPlayerWall;
            EastIsPlayerWall = eastIsPlayerWall;
            SouthIsPlayerWall = southIsPlayerWall;
            WestIsPlayerWall = westIsPlayerWall;

            WallBreakerAvailable = wallBreakerAvailable;
        }
    }
}
