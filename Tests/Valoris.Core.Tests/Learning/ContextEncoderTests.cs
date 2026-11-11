using Valoris.Core.Contracts;
using Valoris.Core.Learning;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class ContextEncoderTests
    {
        private readonly ContextEncoder _encoder =
            new ContextEncoder();

        [Fact]
        public void StateIsWithinDefinedRange()
        {
            var observation = new GridObservation(
                new GridPosition(5, 5),
                new GridPosition(22, 22),
                CellType.Empty,
                CellType.Wall,
                CellType.Empty,
                CellType.Trap);

            int state = _encoder.Encode(observation);

            Assert.InRange(
                state,
                0,
                ContextEncoder.StateCount - 1);
        }

        [Fact]
        public void EquivalentLocalContextsShareTheSameState()
        {
            var first = new GridObservation(
                new GridPosition(5, 5),
                new GridPosition(22, 22),
                CellType.Empty,
                CellType.Wall,
                CellType.Empty,
                CellType.Empty);

            var second = new GridObservation(
                new GridPosition(6, 5),
                new GridPosition(22, 22),
                CellType.Empty,
                CellType.Wall,
                CellType.Empty,
                CellType.Empty);

            Assert.Equal(
                _encoder.Encode(first),
                _encoder.Encode(second));
        }

        [Fact]
        public void DifferentNeighbourhoodsProduceDifferentStates()
        {
            var first = new GridObservation(
                new GridPosition(5, 5),
                new GridPosition(22, 22),
                CellType.Empty,
                CellType.Wall,
                CellType.Empty,
                CellType.Empty);

            var second = new GridObservation(
                new GridPosition(5, 5),
                new GridPosition(22, 22),
                CellType.Trap,
                CellType.Wall,
                CellType.Empty,
                CellType.Empty);

            Assert.NotEqual(
                _encoder.Encode(first),
                _encoder.Encode(second));
        }
    }
}