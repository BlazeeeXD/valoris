using System;
using Valoris.Core.Contracts;
using Valoris.Core.Learning;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class TrainingOptionsValidationTests
    {
        [Fact]
        public void RejectsNonFiniteLearningRate()
        {
            var options = new TrainingOptions { LearningRate = double.NaN };
            Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
        }

        [Fact]
        public void RejectsNonFiniteEpsilonDecay()
        {
            var options = new TrainingOptions { EpsilonDecay = double.PositiveInfinity };
            Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
        }

        [Fact]
        public void RejectsNonFiniteRewardValues()
        {
            var options = new TrainingOptions
            {
                Rewards = new RewardModel { TrapPenalty = double.NegativeInfinity }
            };
            Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
        }

        [Fact]
        public void QTableRejectsNonFiniteTransitionValues()
        {
            var table = new QTable();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                table.Update(1, 0, double.NaN, 0.0, 0.1, 0.9, true));
        }
    }
}
