using System;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    /// <summary>One instance is created per campaign. Its Q-table survives calls to TrainLevel.</summary>
    public sealed class ValorisAgent : IOralisAgent
    {
        private readonly AgentMemory _memory = new AgentMemory();
        private readonly StateEncoder _encoder = new StateEncoder();
        private readonly LevelValidator _validator = new LevelValidator();
        private int _campaignSeed;
        private bool _campaignStarted;

        public void BeginCampaign(int campaignSeed)
        {
            _memory.Reset();
            _campaignSeed = campaignSeed;
            _campaignStarted = true;
        }

        public TrainingResult TrainLevel(LevelDefinition level, TrainingOptions options)
        {
            if (!_campaignStarted) throw new InvalidOperationException("Call BeginCampaign before training a level.");
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();

            LevelValidationResult validation = _validator.Validate(level);
            if (!validation.IsValid)
                throw new ArgumentException("Cannot train on invalid level: " + string.Join("; ", validation.Messages), nameof(level));

            int seed = CombineSeed(_campaignSeed, options.RandomSeed, level.Seed, level.LevelNumber);
            var random = new Random(seed);
            var environment = new GridEnvironment(options.Rewards);
            environment.Reset(level);

            var runner = new TrainingRunner();
            var metrics = runner.Train(environment, _memory.QTable, _encoder, options, random,
                () => _memory.UpdateCount++);

            var evaluator = new PolicyEvaluator();
            EvaluationResult evaluation = evaluator.Evaluate(environment, _memory.QTable,
                _encoder, options.EvaluationMaxSteps, random, options.Rewards.TimeoutPenalty);

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
            return new AgentMemorySummary
            {
                LearnedStateCount = _memory.QTable.LearnedStateCount,
                QTableEntryCount = _memory.QTable.EntryCount,
                UpdateCount = _memory.UpdateCount,
                CampaignLevelCount = _memory.CampaignLevelCount
            };
        }

        private static int CombineSeed(int campaignSeed, int optionsSeed, int levelSeed, int levelNumber)
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
