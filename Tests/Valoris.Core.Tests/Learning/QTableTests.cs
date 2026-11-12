using System;
using Valoris.Core.Contracts;
using Valoris.Core.Learning;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class QTableTests
    {

        [Fact]
        public void StateActionVisitCountsIncrementAndClearWithTheTable()
        {
            var table = new QTable();
            const int state = 314;

            Assert.Equal(0, table.GetVisitCount(state, (int)MoveAction.Right));
            table.Update(state, (int)MoveAction.Right, 1.0, 0.0, 1.0, 0.0, true);
            table.Update(state, (int)MoveAction.Right, 1.0, 0.0, 1.0, 0.0, true);
            table.Update(state, (int)MoveAction.Left, 1.0, 0.0, 1.0, 0.0, true);

            Assert.Equal(2, table.GetVisitCount(state, (int)MoveAction.Right));
            Assert.Equal(1, table.GetVisitCount(state, (int)MoveAction.Left));

            table.Clear();
            Assert.Equal(0, table.GetVisitCount(state, (int)MoveAction.Right));
            Assert.Equal(0, table.LearnedStateCount);
        }

        [Fact]
        public void MaskedMaximumIgnoresWallActions()
        {
            var table = new QTable();
            const int state = 42;
            table.Update(state, (int)MoveAction.Up, 100, 0, 1, 0, true);
            table.Update(state, (int)MoveAction.Right, 2, 0, 1, 0, true);
            table.Update(state, (int)MoveAction.Down, 50, 0, 1, 0, true);
            table.Update(state, (int)MoveAction.Left, 3, 0, 1, 0, true);

            var observation = new GridObservation(
                new GridPosition(5, 5), new GridPosition(22, 22),
                CellType.Wall, CellType.Empty, CellType.Wall, CellType.Empty);

            Assert.Equal(3.0, table.GetMaxValue(state, observation));
        }

        [Fact]
        public void GreedySelectionNeverChoosesWallAction()
        {
            var table = new QTable();
            var random = new Random(17);
            var observation = new GridObservation(
                new GridPosition(5, 5), new GridPosition(22, 22),
                CellType.Wall, CellType.Empty, CellType.Wall, CellType.Empty);

            for (int i = 0; i < 100; i++)
            {
                int action = table.SelectGreedyAction(123, observation, random);
                Assert.True(action == (int)MoveAction.Right || action == (int)MoveAction.Left);
            }
        }

        [Fact]
        public void EmptyLegalStateDoesNotDefaultToUp()
        {
            var table = new QTable();
            var observation = new GridObservation(
                new GridPosition(5, 5), new GridPosition(22, 22),
                CellType.Wall, CellType.Empty, CellType.Wall, CellType.Wall);

            Assert.Equal((int)MoveAction.Right,
                table.SelectGreedyAction(999, observation, new Random(1)));
        }
    }
}
