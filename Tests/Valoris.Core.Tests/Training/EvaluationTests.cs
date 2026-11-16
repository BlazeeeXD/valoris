using System;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;
using Valoris.Core.Tests.Helpers;
using Valoris.Core.Training;
using Xunit;

namespace Valoris.Core.Tests.Training
{
    public sealed class EvaluationTests
    {
        [Fact]
        public void UntrainedPolicyTakesOnlyAvailableActionAndCanReachAdjacentGoal()
        {
            LevelDefinition level = TestLevelFactory.DirectGoalLevel();
            var environment = new GridEnvironment();
            environment.Reset(level);

            EvaluationResult result = new PolicyEvaluator().Evaluate(
                environment, new QTable(), new StateEncoder(), 5, new Random(7), -25);

            Assert.True(result.ReachedGoal);
            Assert.Equal(TerminationReason.ReachedGoal, result.TerminationReason);
            Assert.Equal(1, result.StepsTaken);
            Assert.Equal(2, result.Path.Count);
        }
    }
}
