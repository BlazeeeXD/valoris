namespace Valoris.Core.Contracts
{
    /// <summary>Agent observation. Neighbours use Wall for out-of-bounds cells.</summary>
    public readonly struct GridObservation
    {
        public GridPosition Position { get; }
        public GridPosition Goal { get; }
        public CellType North { get; }
        public CellType East { get; }
        public CellType South { get; }
        public CellType West { get; }

        public GridObservation(GridPosition position, GridPosition goal,
            CellType north, CellType east, CellType south, CellType west)
        {
            Position = position;
            Goal = goal;
            North = north;
            East = east;
            South = south;
            West = west;
        }
    }
}
