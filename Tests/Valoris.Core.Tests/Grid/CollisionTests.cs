using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Tests.Helpers;
using Xunit;

namespace Valoris.Core.Tests.Grid
{
    public sealed class CollisionTests
    {
        [Fact]
        public void WallBlocksMovementAndAppliesPenalty()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            cells[1 * 24 + 2] = (int)CellType.Wall;
            var level = new LevelDefinition(1, 1, 2, 24, 24, cells, source.Start, source.Goal);
            var environment = new GridEnvironment();
            environment.Reset(level);

            StepResult result = environment.Step(MoveAction.Right);

            Assert.Equal(source.Start, result.Position);
            Assert.False(result.IsTerminal);
            Assert.Equal(-3.5, result.Reward, 8);
        }

        [Fact]
        public void TrapTerminatesAttempt()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            cells[1 * 24 + 2] = (int)CellType.Trap;
            var level = new LevelDefinition(1, 1, 2, 24, 24, cells, source.Start, source.Goal);
            var environment = new GridEnvironment();
            environment.Reset(level);

            StepResult result = environment.Step(MoveAction.Right);

            Assert.True(result.IsTerminal);
            Assert.Equal(TerminationReason.EnteredTrap, result.TerminationReason);
            Assert.Equal(-100.5, result.Reward, 8);
        }

        [Fact]
        public void MudIsTraversableAndAppliesAdditionalPenalty()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            cells[1 * 24 + 2] = (int)CellType.Mud;
            var level = new LevelDefinition(1, 1, 2, 24, 24, cells, source.Start, source.Goal);
            var environment = new GridEnvironment();
            environment.Reset(level);

            StepResult result = environment.Step(MoveAction.Right);

            Assert.Equal(new GridPosition(2, 1), result.Position);
            Assert.False(result.IsTerminal);
            Assert.Equal(-2.5, result.Reward, 8);
        }
    }
}
