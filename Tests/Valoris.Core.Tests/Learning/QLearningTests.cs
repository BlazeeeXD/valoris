using Valoris.Core.Learning;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class QLearningTests
    {
        [Fact]
        public void UpdateUsesBellmanEquation()
        {
            double updated = QLearningUpdate.Calculate(0.0, 10.0, 4.0, 0.5, 0.9, false);
            Assert.Equal(6.8, updated, 8);
        }

        [Fact]
        public void TerminalUpdateDoesNotBootstrapFromNextState()
        {
            double updated = QLearningUpdate.Calculate(0.0, 10.0, 100.0, 0.5, 0.9, true);
            Assert.Equal(5.0, updated, 8);
        }

        [Fact]
        public void QTableStoresActionValues()
        {
            var table = new QTable();
            table.Update(10, 1, 5, 0, 0.5, 0.9, true);
            Assert.Equal(2.5, table.GetValue(10, 1), 8);
            Assert.Equal(1, table.LearnedStateCount);
            Assert.Equal(4, table.EntryCount);
        }
    }
}
