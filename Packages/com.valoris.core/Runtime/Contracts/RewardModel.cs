using System;

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

        public void Validate()
        {
            ValidateFinite(GoalReward, nameof(GoalReward));
            ValidateFinite(TrapPenalty, nameof(TrapPenalty));
            ValidateFinite(WallPenalty, nameof(WallPenalty));
            ValidateFinite(MudPenalty, nameof(MudPenalty));
            ValidateFinite(StepPenalty, nameof(StepPenalty));
            ValidateFinite(TimeoutPenalty, nameof(TimeoutPenalty));
        }

        private static void ValidateFinite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name, "Reward values must be finite numbers.");
        }
    }
}
