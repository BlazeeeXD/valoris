using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Tests.Helpers;
using Xunit;

namespace Valoris.Core.Tests.Grid
{
    public sealed class GridEnvironmentTests
    {
        [Fact]
        public void RightMoveChangesPosition()
        {
            var environment = new GridEnvironment();
            environment.Reset(TestLevelFactory.OpenLevel());
            StepResult result = environment.Step(MoveAction.Right);
            Assert.Equal(new GridPosition(2, 1), result.Position);
            Assert.False(result.IsTerminal);
        }

        [Fact]
        public void WallMoveDoesNotChangePosition()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            cells[1 * 24 + 2] = (int)CellType.Wall;
            var level = new LevelDefinition(1, 1, 1, 24, 24, cells, source.Start, source.Goal);
            var environment = new GridEnvironment();
            environment.Reset(level);
            StepResult result = environment.Step(MoveAction.Right);
            Assert.Equal(source.Start, result.Position);
            Assert.False(result.IsTerminal);
        }

        [Fact]
        public void TrapTerminatesAttempt()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            cells[1 * 24 + 2] = (int)CellType.Trap;
            var level = new LevelDefinition(1, 1, 1, 24, 24, cells, source.Start, source.Goal);
            var environment = new GridEnvironment();
            environment.Reset(level);
            StepResult result = environment.Step(MoveAction.Right);
            Assert.True(result.IsTerminal);
            Assert.Equal(TerminationReason.EnteredTrap, result.TerminationReason);
            Assert.Equal(new GridPosition(2, 1), result.Position);
        }

        [Fact]
        public void GoalTerminatesSuccessfully()
        {
            var source = TestLevelFactory.OpenLevel();
            var level = new LevelDefinition(1, 1, 1, 24, 24, source.CopyCells(),
                new GridPosition(1, 1), new GridPosition(2, 1));
            var environment = new GridEnvironment();
            environment.Reset(level);
            StepResult result = environment.Step(MoveAction.Right);
            Assert.True(result.IsTerminal);
            Assert.Equal(TerminationReason.ReachedGoal, result.TerminationReason);
        }

        [Fact]
        public void MudIsTraversableAndHasAdditionalPenalty()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            int[] cells = source.CopyCells();
            cells[1 * 24 + 2] = (int)CellType.Mud;
            var level = new LevelDefinition(1, 1, 1, 24, 24, cells, source.Start, source.Goal);
            var environment = new GridEnvironment();
            environment.Reset(level);
            StepResult result = environment.Step(MoveAction.Right);
            Assert.False(result.IsTerminal);
            Assert.Equal(new GridPosition(2, 1), result.Position);
            Assert.True(result.Reward < -0.5);
        }
    }
}
