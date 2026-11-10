using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Grid
{
    public static class GridRules
    {
        public static GridPosition Apply(GridPosition position, MoveAction action)
        {
            switch (action)
            {
                case MoveAction.Up: return new GridPosition(position.X, position.Y - 1);
                case MoveAction.Right: return new GridPosition(position.X + 1, position.Y);
                case MoveAction.Down: return new GridPosition(position.X, position.Y + 1);
                case MoveAction.Left: return new GridPosition(position.X - 1, position.Y);
                default: throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        public static bool IsInside(int x, int y, int width, int height)
            => x >= 0 && y >= 0 && x < width && y < height;

        public static CellType GetNeighbourCell(LevelDefinition level, GridPosition position, MoveAction direction)
        {
            GridPosition next = Apply(position, direction);
            if (!IsInside(next.X, next.Y, level.Width, level.Height)) return CellType.Wall;
            return level.GetCell(next.X, next.Y);
        }

        public static bool IsSafeTraversable(CellType cell)
            => cell != CellType.Wall && cell != CellType.Trap;

    }
}
