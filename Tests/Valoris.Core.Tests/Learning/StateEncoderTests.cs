using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;
using Valoris.Core.Tests.Helpers;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class StateEncoderTests
    {
        [Fact]
        public void EncodedStateIsWithinDefinedRange()
        {
            var environment = new GridEnvironment();
            environment.Reset(TestLevelFactory.OpenLevel());
            int state = new StateEncoder().Encode(environment.Observe());
            Assert.InRange(state, 0, StateEncoder.StateCount - 1);
        }

        [Fact]
        public void DifferentNeighbourTerrainProducesDifferentState()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            var firstEnvironment = new GridEnvironment();
            firstEnvironment.Reset(source);
            int first = new StateEncoder().Encode(firstEnvironment.Observe());

            int[] cells = source.CopyCells();
            cells[1 * 24 + 2] = (int)CellType.Wall;
            var altered = new LevelDefinition(1, 1, 2, 24, 24, cells, source.Start, source.Goal);
            var secondEnvironment = new GridEnvironment();
            secondEnvironment.Reset(altered);
            int second = new StateEncoder().Encode(secondEnvironment.Observe());

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void DifferentGoalDirectionProducesDifferentState()
        {
            LevelDefinition source = TestLevelFactory.OpenLevel();
            var firstEnvironment = new GridEnvironment();
            firstEnvironment.Reset(source);
            int first = new StateEncoder().Encode(firstEnvironment.Observe());

            var altered = new LevelDefinition(1, 1, 2, 24, 24, source.CopyCells(),
                source.Start, new GridPosition(22, 1));
            var secondEnvironment = new GridEnvironment();
            secondEnvironment.Reset(altered);
            int second = new StateEncoder().Encode(secondEnvironment.Observe());

            Assert.NotEqual(first, second);
        }
    }
}
