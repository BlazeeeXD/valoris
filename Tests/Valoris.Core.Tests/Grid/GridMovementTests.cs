using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Tests.Helpers;
using Xunit;

namespace Valoris.Core.Tests.Grid
{
    public sealed class GridMovementTests
    {
        [Fact]
        public void RightAndDownActionsUpdatePosition()
        {
            var environment = new GridEnvironment();
            environment.Reset(TestLevelFactory.OpenLevel());

            StepResult right = environment.Step(MoveAction.Right);
            Assert.Equal(new GridPosition(2, 1), right.Position);
            Assert.False(right.IsTerminal);

            StepResult down = environment.Step(MoveAction.Down);
            Assert.Equal(new GridPosition(2, 2), down.Position);
            Assert.False(down.IsTerminal);
        }

        [Fact]
        public void ResetEpisodeReturnsAgentToStart()
        {
            var environment = new GridEnvironment();
            LevelDefinition level = TestLevelFactory.OpenLevel();
            environment.Reset(level);
            environment.Step(MoveAction.Right);
            environment.ResetEpisode();

            Assert.Equal(level.Start, environment.Position);
            Assert.Equal(0, environment.StepsTaken);
        }
    }
}
