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
        public void PositionAwareAgentLearnsTheHardFixtureWithinThreeThousandEpisodes()
        {
            var agent = new ValorisAgent();
            agent.BeginCampaign(7);
            var options = new TrainingOptions
            {
                EpisodesPerLevel = 3000,
                MaxStepsPerEpisode = 300,
                EvaluationMaxSteps = 300,
                RandomSeed = 7
            };

            TrainingResult result = agent.TrainLevel(TestLevelFactory.LoadHardFixture(), options);

            Assert.True(result.FinalEvaluation.ReachedGoal,
                "The position-aware learner should solve the known-valid hard fixture. " +
                "Termination: " + result.FinalEvaluation.TerminationReason +
                "; steps: " + result.FinalEvaluation.StepsTaken);
            Assert.Equal(TerminationReason.ReachedGoal, result.FinalEvaluation.TerminationReason);
            Assert.InRange(result.FinalEvaluation.StepsTaken, 72, 300);
        }


        [Fact]
        public void ContextualMemorySurvivesMapTransitionWhileSpatialMemoryIsReplaced()
        {
            var agent = new ValorisAgent();
            agent.BeginCampaign(321);
            var options = new TrainingOptions
            {
                EpisodesPerLevel = 1,
                MaxStepsPerEpisode = 2,
                EvaluationMaxSteps = 2,
                InitialEpsilon = 0.0,
                MinimumEpsilon = 0.0,
                EpsilonDecay = 1.0,
                RandomSeed = 9
            };

            TrainingResult first = agent.TrainLevel(AdjacentGoalLevel(1, 1, 1, 100), options);
            Assert.Equal(1, first.MemoryAfterTraining.ContextLearnedStateCount);
            Assert.Equal(1, first.MemoryAfterTraining.SpatialLearnedStateCount);

            TrainingResult second = agent.TrainLevel(AdjacentGoalLevel(5, 5, 2, 200), options);
            Assert.Equal(2, second.MemoryAfterTraining.CampaignLevelCount);
            Assert.Equal(1, second.MemoryAfterTraining.ContextLearnedStateCount);
            Assert.Equal(1, second.MemoryAfterTraining.SpatialLearnedStateCount);
            Assert.Equal(2, second.MemoryAfterTraining.UpdateCount);

            agent.BeginCampaign(322);
            AgentMemorySummary fresh = agent.GetMemorySummary();
            Assert.Equal(0, fresh.ContextLearnedStateCount);
            Assert.Equal(0, fresh.SpatialLearnedStateCount);
            Assert.Equal(0, fresh.CampaignLevelCount);
        }

        private static LevelDefinition AdjacentGoalLevel(int startX, int startY, int levelNumber, int seed)
        {
            const int width = 24;
            const int height = 24;
            var cells = new int[width * height];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = (int)CellType.Wall;

            cells[startY * width + startX] = (int)CellType.Empty;
            cells[startY * width + startX + 1] = (int)CellType.Empty;

            return new LevelDefinition(
                LevelDefinition.CurrentSchemaVersion,
                levelNumber,
                seed,
                width,
                height,
                cells,
                new GridPosition(startX, startY),
                new GridPosition(startX + 1, startY));
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
