using Valoris.Core.Learning;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class QLearningUpdateTests
    {
        [Fact]
        public void NonTerminalUpdateBootstrapsFromNextState()
        {
            double value = QLearningUpdate.Calculate(2.0, 5.0, 10.0, 0.25, 0.9, false);
            Assert.Equal(5.0, value, 8);
        }

        [Fact]
        public void TerminalUpdateDoesNotBootstrap()
        {
            double value = QLearningUpdate.Calculate(2.0, 5.0, 1000.0, 0.25, 0.9, true);
            Assert.Equal(2.75, value, 8);
        }
    }
}
