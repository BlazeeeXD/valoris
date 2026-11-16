using System.Collections.Generic;

namespace Valoris.Core.Contracts
{
    public sealed class EvaluationResult
    {
        public bool ReachedGoal { get; set; }
        public int StepsTaken { get; set; }
        public double TotalReward { get; set; }
        public TerminationReason TerminationReason { get; set; }
        public IReadOnlyList<GridPosition> Path { get; set; }
    }
}
