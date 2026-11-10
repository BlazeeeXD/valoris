namespace Valoris.Core.Contracts
{
    public sealed class TrainingOptions
    {
        public int EpisodesPerLevel { get; set; } = 3000;
        public int MaxStepsPerEpisode { get; set; } = 300;
        public int EvaluationMaxSteps { get; set; } = 300;
        public double LearningRate { get; set; } = 0.15;
        public double DiscountFactor { get; set; } = 0.95;
        public double InitialEpsilon { get; set; } = 1.0;
        public double MinimumEpsilon { get; set; } = 0.05;
        public double EpsilonDecay { get; set; } = 0.9995;
        public int RandomSeed { get; set; } = 1;
        public RewardModel Rewards { get; set; } = new RewardModel();

        public void Validate()
        {
            if (EpisodesPerLevel <= 0) throw new System.ArgumentOutOfRangeException(nameof(EpisodesPerLevel));
            if (MaxStepsPerEpisode <= 0) throw new System.ArgumentOutOfRangeException(nameof(MaxStepsPerEpisode));
            if (EvaluationMaxSteps <= 0) throw new System.ArgumentOutOfRangeException(nameof(EvaluationMaxSteps));
            if (LearningRate <= 0 || LearningRate > 1) throw new System.ArgumentOutOfRangeException(nameof(LearningRate));
            if (DiscountFactor < 0 || DiscountFactor > 1) throw new System.ArgumentOutOfRangeException(nameof(DiscountFactor));
            if (InitialEpsilon < 0 || InitialEpsilon > 1) throw new System.ArgumentOutOfRangeException(nameof(InitialEpsilon));
            if (MinimumEpsilon < 0 || MinimumEpsilon > InitialEpsilon) throw new System.ArgumentOutOfRangeException(nameof(MinimumEpsilon));
            if (EpsilonDecay <= 0 || EpsilonDecay > 1) throw new System.ArgumentOutOfRangeException(nameof(EpsilonDecay));
            if (Rewards == null) throw new System.ArgumentNullException(nameof(Rewards));
        }
    }
}
