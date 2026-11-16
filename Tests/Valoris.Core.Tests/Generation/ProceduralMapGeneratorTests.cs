using System;
using Valoris.Core.Generation;
using Valoris.Core.Contracts;
using Xunit;

namespace Valoris.Core.Tests.Generation
{
    public sealed class ProceduralMapGeneratorTests
    {
        private readonly ProceduralMapGenerator _generator =
            new ProceduralMapGenerator();

        [Theory]
        [InlineData(MapDifficulty.Easy)]
        [InlineData(MapDifficulty.Medium)]
        [InlineData(MapDifficulty.Hard)]
        public void GeneratedMapsAreValid(MapDifficulty difficulty)
        {
            var options = new MapGenerationOptions
            {
                Seed = 12345,
                LevelNumber = 1,
                Difficulty = difficulty
            };

            MapGenerationResult result =
                _generator.Generate(options);

            Assert.NotNull(result.Level);
            Assert.True(result.Validation.IsValid);
            Assert.True(result.Validation.HasSafePath);

            Assert.Equal(24, result.Level.Width);
            Assert.Equal(24, result.Level.Height);

            Assert.Equal(
                CellType.Empty,
                result.Level.GetCell(
                    result.Level.Start.X,
                    result.Level.Start.Y));

            Assert.Equal(
                CellType.Empty,
                result.Level.GetCell(
                    result.Level.Goal.X,
                    result.Level.Goal.Y));
        }

        [Fact]
        public void SameSeedProducesIdenticalMaps()
        {
            var optionsA = new MapGenerationOptions
            {
                Seed = 777,
                LevelNumber = 1,
                Difficulty = MapDifficulty.Hard
            };

            var optionsB = new MapGenerationOptions
            {
                Seed = 777,
                LevelNumber = 1,
                Difficulty = MapDifficulty.Hard
            };

            LevelDefinition first =
                _generator.Generate(optionsA).Level;

            LevelDefinition second =
                _generator.Generate(optionsB).Level;

            Assert.Equal(first.CopyCells(), second.CopyCells());
        }

        [Fact]
        public void DifferentSeedsProduceDifferentMaps()
        {
            var optionsA = new MapGenerationOptions
            {
                Seed = 1001,
                Difficulty = MapDifficulty.Hard
            };

            var optionsB = new MapGenerationOptions
            {
                Seed = 2002,
                Difficulty = MapDifficulty.Hard
            };

            LevelDefinition first =
                _generator.Generate(optionsA).Level;

            LevelDefinition second =
                _generator.Generate(optionsB).Level;

            Assert.False(System.Linq.Enumerable.SequenceEqual(first.CopyCells(),second.CopyCells()));
        }

        [Fact]
        public void HardMapContainsRequestedHazards()
        {
            var options = new MapGenerationOptions
            {
                Seed = 3003,
                Difficulty = MapDifficulty.Hard
            };

            MapGenerationResult result =
                _generator.Generate(options);

            Assert.Equal(12, result.ActualTrapCount);
            Assert.Equal(16, result.ActualMudCount);
            Assert.True(result.ActualTrapCount <= options.TrapCount
                        || options.TrapCount == -1);
        }


        [Fact]
        public void BaseGameplayMapCanLeaveAllSabotagePlacementToThePlayer()
        {
            var options = new MapGenerationOptions
            {
                Seed = 8801,
                LevelNumber = 1,
                Difficulty = MapDifficulty.Hard,
                TrapCount = 0,
                MudCount = 0
            };

            MapGenerationResult result = _generator.Generate(options);

            Assert.True(result.Validation.IsValid);
            Assert.True(result.Validation.HasSafePath);
            Assert.Equal(0, result.ActualTrapCount);
            Assert.Equal(0, result.ActualMudCount);
        }

        [Fact]
        public void GeneratedMapsAlwaysRetainASafeRoute()
        {
            for (int seed = 1; seed <= 100; seed++)
            {
                var options = new MapGenerationOptions
                {
                    Seed = seed,
                    Difficulty = MapDifficulty.Hard
                };

                MapGenerationResult result =
                    _generator.Generate(options);

                Assert.True(
                    result.Validation.HasSafePath,
                    "No safe path for seed " + seed);
            }
        }
    }
}