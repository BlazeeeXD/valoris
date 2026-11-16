
using System;

namespace Valoris.Core.Contracts
{
    /// <summary>
    /// Immutable snapshot of a locked level.
    /// Cell IDs are stored row-major: index = y * Width + x.
    /// </summary>
    public sealed class LevelDefinition
    {
        public const int CurrentSchemaVersion = 1;
        public const int CampaignWidth = 24;
        public const int CampaignHeight = 24;

        private readonly int[] _cells;
        private readonly bool[] _playerPlacedWalls;

        public int SchemaVersion { get; }
        public int LevelNumber { get; }
        public int Seed { get; }
        public int Width { get; }
        public int Height { get; }
        public GridPosition Start { get; }
        public GridPosition Goal { get; }
        public int CellCount => _cells.Length;

        public LevelDefinition(
            int schemaVersion,
            int levelNumber,
            int seed,
            int width,
            int height,
            int[] cells,
            GridPosition start,
            GridPosition goal,
            bool[] playerPlacedWalls = null)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            if (playerPlacedWalls != null &&
                playerPlacedWalls.Length != cells.Length)
            {
                throw new ArgumentException(
                    "Player placed wall metadata must match the cell array length.",
                    nameof(playerPlacedWalls));
            }

            SchemaVersion = schemaVersion;
            LevelNumber = levelNumber;
            Seed = seed;
            Width = width;
            Height = height;
            Start = start;
            Goal = goal;

            _cells = (int[])cells.Clone();

            _playerPlacedWalls = playerPlacedWalls == null
                ? new bool[cells.Length]
                : (bool[])playerPlacedWalls.Clone();

            for (int i = 0; i < _playerPlacedWalls.Length; i++)
            {
                if (_playerPlacedWalls[i] && _cells[i] != (int)CellType.Wall)
                {
                    throw new ArgumentException(
                        "Player placed wall metadata can only identify wall cells.",
                        nameof(playerPlacedWalls));
                }
            }
        }

        public CellType GetCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException(
                    "Coordinates are outside the level.");

            return (CellType)_cells[y * Width + x];
        }

        public int GetCellValue(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException(
                    "Coordinates are outside the level.");

            return _cells[y * Width + x];
        }

        public bool IsPlayerPlacedWall(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException(
                    "Coordinates are outside the level.");

            return _playerPlacedWalls[y * Width + x];
        }

        public int[] CopyCells()
        {
            return (int[])_cells.Clone();
        }

        public bool[] CopyPlayerPlacedWalls()
        {
            return (bool[])_playerPlacedWalls.Clone();
        }
    }
}
