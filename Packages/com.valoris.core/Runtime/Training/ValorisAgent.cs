using System;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    /// <summary>
    /// One agent instance represents one campaign.
    /// Contextual memory persists across levels.
    /// Spatial memory resets when a new level begins.
    /// </summary>
    public sealed class ValorisAgent : IOralisAgent
    {
        private readonly AgentMemory _memory = new AgentMemory();

        private readonly StateEncoder _spatialEncoder =
            new StateEncoder();

        private readonly ContextEncoder _contextEncoder =
            new ContextEncoder();

        private readonly LevelValidator _validator =
            new LevelValidator();

        private int _campaignSeed;

        private bool _campaignStarted;

        public void BeginCampaign(int campaignSeed)
        {
            _memory.Reset();

            _campaignSeed = campaignSeed;

            _campaignStarted = true;
        }

        public TrainingResult TrainLevel(
            LevelDefinition level,
            TrainingOptions options)
        {
            if (!_campaignStarted)
            {
                throw new InvalidOperationException(
                    "Call BeginCampaign before training a level.");
            }

            if (level == null)
                throw new ArgumentNullException(nameof(level));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            LevelValidationResult validation =
                _validator.Validate(level);

            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    "Cannot train on invalid level: "
                    + string.Join("; ", validation.Messages),
                    nameof(level));
            }

            // Keep contextual memory.
            // Clear only the previous level's spatial memory.
            _memory.BeginLevel();

            int seed = CombineSeed(
                _campaignSeed,
                options.RandomSeed,
                level.Seed,
                level.LevelNumber);

            var random = new Random(seed);

            var environment =
                new GridEnvironment(options.Rewards);

            environment.Reset(level);

            var policy = new TransferPolicy(
                _memory.SpatialQTable,
                _memory.ContextQTable,
                _spatialEncoder,
                _contextEncoder,
                random,
                contextWeight: 0.0);

            var runner = new TransferTrainingRunner();

            var metrics = runner.Train(
                environment,
                policy,
                options,
                () => _memory.UpdateCount++);

            var evaluator = new TransferPolicyEvaluator();

            EvaluationResult evaluation = evaluator.Evaluate(
                environment,
                policy,
                options.EvaluationMaxSteps,
                options.Rewards.TimeoutPenalty);

            _memory.CampaignLevelCount++;

            return new TrainingResult
            {
                EpisodeMetrics = metrics,

                FinalEvaluation = evaluation,

                MemoryAfterTraining = GetMemorySummary()
            };
        }

        public AgentMemorySummary GetMemorySummary()
        {
            int contextualStates =
                _memory.ContextQTable.LearnedStateCount;

            int spatialStates =
                _memory.SpatialQTable.LearnedStateCount;

            int contextualEntries =
                _memory.ContextQTable.EntryCount;

            int spatialEntries =
                _memory.SpatialQTable.EntryCount;

            return new AgentMemorySummary
            {
                LearnedStateCount =
                    contextualStates + spatialStates,

                QTableEntryCount =
                    contextualEntries + spatialEntries,

                ContextLearnedStateCount =
                    contextualStates,

                SpatialLearnedStateCount =
                    spatialStates,

                ContextQTableEntryCount =
                    contextualEntries,

                SpatialQTableEntryCount =
                    spatialEntries,

                UpdateCount =
                    _memory.UpdateCount,

                CampaignLevelCount =
                    _memory.CampaignLevelCount
            };
        }

        private static int CombineSeed(
            int campaignSeed,
            int optionsSeed,
            int levelSeed,
            int levelNumber)
        {
            unchecked
            {
                int value = campaignSeed;

                value = value * 31 + optionsSeed;

                value = value * 31 + levelSeed;

                value = value * 31 + levelNumber;

                return value;
            }
        }
    }
}