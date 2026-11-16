
using System.Collections.Generic;

namespace Valoris.Core.Contracts
{
    public sealed class EvaluationResult
    {
        public bool ReachedGoal { get; set; }
        public int StepsTaken { get; set; }
        public double TotalReward { get; set; }
        public TerminationReason TerminationReason { get; set; }

        // Contains the starting position followed by each resulting position.
        public IReadOnlyList<GridPosition> Path { get; set; }

        // Contains one action for every transition in Path.
        public IReadOnlyList<MoveAction> Actions { get; set; }
    }
}
