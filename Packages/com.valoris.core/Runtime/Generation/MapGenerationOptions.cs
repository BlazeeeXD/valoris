using System;

namespace Valoris.Core.Generation
{
    public sealed class MapGenerationOptions
    {
        public int Width { get; set; } = 24;
        public int Height { get; set; } = 24;

        public int Seed { get; set; } = 1;
        public int LevelNumber { get; set; } = 1;

        public MapDifficulty Difficulty { get; set; }
            = MapDifficulty.Easy;

        // -1 means use the difficulty defaults.
        public int AdditionalPassages { get; set; } = -1;
        public int TrapCount { get; set; } = -1;
        public int MudCount { get; set; } = -1;

        public void Validate()
        {
            if (Width != 24 || Height != 24)
                throw new ArgumentException(
                    "Valoris currently requires a 24x24 grid.");

            if (LevelNumber <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(LevelNumber));

            if (!Enum.IsDefined(typeof(MapDifficulty), Difficulty))
                throw new ArgumentOutOfRangeException(
                    nameof(Difficulty));

            if (AdditionalPassages < -1)
                throw new ArgumentOutOfRangeException(
                    nameof(AdditionalPassages));

            if (TrapCount < -1)
                throw new ArgumentOutOfRangeException(
                    nameof(TrapCount));

            if (MudCount < -1)
                throw new ArgumentOutOfRangeException(
                    nameof(MudCount));
        }
    }
}