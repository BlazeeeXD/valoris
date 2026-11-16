using Valoris.Core.Contracts;
using Valoris.Core.Grid;

namespace Valoris.Core.Generation
{
    public sealed class MapGenerationResult
    {
        public LevelDefinition Level { get; }

        public MapDifficulty Difficulty { get; }

        public int OpenedPassages { get; }

        public int ActualTrapCount { get; }

        public int ActualMudCount { get; }

        public LevelValidationResult Validation { get; }

        public MapGenerationResult(
            LevelDefinition level,
            MapDifficulty difficulty,
            int openedPassages,
            int actualTrapCount,
            int actualMudCount,
            LevelValidationResult validation)
        {
            Level = level;
            Difficulty = difficulty;
            OpenedPassages = openedPassages;
            ActualTrapCount = actualTrapCount;
            ActualMudCount = actualMudCount;
            Validation = validation;
        }
    }
}