using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Tests.Helpers;
using Xunit;

namespace Valoris.Core.Tests.Grid
{
    public sealed class LevelValidationTests
    {
        [Fact]
        public void RejectsUnsupportedDimensions()
        {
            var level = new LevelDefinition(1, 1, 1, 23, 24, new int[23 * 24],
                new GridPosition(1, 1), new GridPosition(2, 1));
            LevelValidationResult result = new LevelValidator().Validate(level);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void RejectsNonPositiveLevelNumber()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            var invalid = new LevelDefinition(
                LevelDefinition.CurrentSchemaVersion,
                0,
                source.Seed,
                source.Width,
                source.Height,
                source.CopyCells(),
                source.Start,
                source.Goal);

            LevelValidationResult result = new LevelValidator().Validate(invalid);
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, message => message.Contains("Level number must be positive"));
        }

        [Fact]
        public void RejectsUnknownCellValues()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            cells[2 * 24 + 2] = 99;
            var invalid = new LevelDefinition(1, 1, 2, 24, 24, cells, source.Start, source.Goal);

            LevelValidationResult result = new LevelValidator().Validate(invalid);
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, message => message.Contains("Undefined cell value"));
        }
    }
}
