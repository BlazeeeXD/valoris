using Valoris.Core.Contracts;
using Valoris.Core.Tests.Helpers;
using Valoris.Core.Training;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class MemoryPersistenceTests
    {
        [Fact]
        public void QTableSurvivesLevelTransitionButResetsForNewCampaign()
        {
            var agent = new ValorisAgent();
            agent.BeginCampaign(12);
            var options = new TrainingOptions
            {
                EpisodesPerLevel = 25,
                MaxStepsPerEpisode = 20,
                EvaluationMaxSteps = 20,
                RandomSeed = 3
            };

            TrainingResult first = agent.TrainLevel(TestLevelFactory.OpenLevel(1, 100), options);
            int firstStateCount = first.MemoryAfterTraining.LearnedStateCount;
            TrainingResult second = agent.TrainLevel(TestLevelFactory.OpenLevel(2, 200), options);

            Assert.Equal(2, second.MemoryAfterTraining.CampaignLevelCount);
            Assert.True(second.MemoryAfterTraining.UpdateCount > first.MemoryAfterTraining.UpdateCount);
            Assert.True(second.MemoryAfterTraining.LearnedStateCount >= firstStateCount);

            agent.BeginCampaign(13);
            AgentMemorySummary fresh = agent.GetMemorySummary();
            Assert.Equal(0, fresh.LearnedStateCount);
            Assert.Equal(0, fresh.UpdateCount);
            Assert.Equal(0, fresh.CampaignLevelCount);
        }
    }
}
