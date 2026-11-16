using System;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;
using Valoris.Core.Tests.Helpers;
using Valoris.Core.Training;
using Xunit;

namespace Valoris.Core.Tests.Training
{
    public sealed class EpisodeTests
    {
        [Fact]
        public void EpisodeStopsAtConfiguredStepLimit()
        {
            var environment = new GridEnvironment();
            environment.Reset(TestLevelFactory.OpenLevel());
            var options = new TrainingOptions
            {
                MaxStepsPerEpisode = 1,
                InitialEpsilon = 1.0,
                MinimumEpsilon = 0.05
            };

            EpisodeMetric metric = new EpisodeRunner().Run(environment, new QTable(),
                new StateEncoder(), options, new Random(4), 1, 1.0);

            Assert.Equal(1, metric.Steps);
            Assert.False(metric.ReachedGoal);
            Assert.Equal(TerminationReason.StepLimit, metric.TerminationReason);
        }
    }
}
