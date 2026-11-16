using Valoris.Core.Contracts;

namespace Valoris.Core.Diagnostics
{
    public sealed class SimulationEvent
    {
        public int StepNumber { get; set; }
        public GridPosition From { get; set; }
        public GridPosition To { get; set; }
        public MoveAction Action { get; set; }
        public double Reward { get; set; }
        public TerminationReason TerminationReason { get; set; }
    }
}
