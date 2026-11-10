namespace Valoris.Core.Contracts
{
    /// <summary>Default rewards. Values are starting parameters and must be tuned experimentally.</summary>
    public sealed class RewardModel
    {
        public double GoalReward { get; set; } = 100.0;
        public double TrapPenalty { get; set; } = -100.0;
        public double WallPenalty { get; set; } = -3.0;
        public double MudPenalty { get; set; } = -2.0;
        public double StepPenalty { get; set; } = -0.5;
        public double TimeoutPenalty { get; set; } = -25.0;
    }
}
