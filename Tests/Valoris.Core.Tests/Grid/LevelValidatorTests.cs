using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Tests.Helpers;
using Xunit;

namespace Valoris.Core.Tests.Grid
{
    public sealed class LevelValidatorTests
    {
        [Fact]
        public void OpenMapIsValidAndHasSafePath()
        {
            LevelValidationResult result = new LevelValidator().Validate(TestLevelFactory.OpenLevel());
            Assert.True(result.IsValid);
            Assert.True(result.HasSafePath);
        }

        [Fact]
        public void FullyBlockedMapIsInvalid()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            for (int y = 1; y < 23; y++) cells[y * 24 + 2] = (int)CellType.Wall;
            var level = new LevelDefinition(1, 1, 1, 24, 24, cells, source.Start, source.Goal);
            LevelValidationResult result = new LevelValidator().Validate(level);
            Assert.False(result.IsValid);
            Assert.False(result.HasSafePath);
        }

        [Fact]
        public void TrapIsNotConsideredSafeRoute()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            for (int y = 1; y < 23; y++) cells[y * 24 + 2] = (int)CellType.Trap;
            var level = new LevelDefinition(1, 1, 1, 24, 24, cells, source.Start, source.Goal);
            LevelValidationResult result = new LevelValidator().Validate(level);
            Assert.False(result.HasSafePath);
        }

        [Fact]
        public void StartAndGoalCannotBeSame()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            var level = new LevelDefinition(1, 1, 1, 24, 24, source.CopyCells(), source.Start, source.Start);
            Assert.False(new LevelValidator().Validate(level).IsValid);
        }
    }
}
