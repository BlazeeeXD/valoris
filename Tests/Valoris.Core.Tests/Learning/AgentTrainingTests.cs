using Valoris.Core.Contracts;
using Valoris.Core.Tests.Helpers;
using Valoris.Core.Training;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class AgentTrainingTests
    {
        [Fact]
        public void AgentMustBeStartedBeforeTraining()
        {
            var agent = new ValorisAgent();
            Assert.Throws<System.InvalidOperationException>(() => agent.TrainLevel(TestLevelFactory.OpenLevel(), new TrainingOptions()));
        }

        [Fact]
        public void CampaignMemoryPersistsAcrossLevelsAndResetsAtNewCampaign()
        {
            var agent = new ValorisAgent();
            agent.BeginCampaign(123);
            var first = new LevelDefinition(1, 1, 11, 24, 24, TestLevelFactory.OpenLevel().CopyCells(),
                new GridPosition(1, 1), new GridPosition(2, 1));
            var second = new LevelDefinition(1, 2, 12, 24, 24, TestLevelFactory.OpenLevel().CopyCells(),
                new GridPosition(1, 1), new GridPosition(2, 1));
            var options = new TrainingOptions
            {
                EpisodesPerLevel = 100,
                MaxStepsPerEpisode = 10,
                EvaluationMaxSteps = 10,
                InitialEpsilon = 0.5,
                MinimumEpsilon = 0.05,
                EpsilonDecay = 0.98,
                RandomSeed = 7
            };

            TrainingResult result1 = agent.TrainLevel(first, options);
            long updatesAfterFirst = result1.MemoryAfterTraining.UpdateCount;
            TrainingResult result2 = agent.TrainLevel(second, options);

            Assert.Equal(2, result2.MemoryAfterTraining.CampaignLevelCount);
            Assert.True(result2.MemoryAfterTraining.UpdateCount > updatesAfterFirst);
            Assert.True(result2.MemoryAfterTraining.LearnedStateCount > 0);

            agent.BeginCampaign(456);
            AgentMemorySummary reset = agent.GetMemorySummary();
            Assert.Equal(0, reset.LearnedStateCount);
            Assert.Equal(0, reset.UpdateCount);
            Assert.Equal(0, reset.CampaignLevelCount);
        }
    }
}
