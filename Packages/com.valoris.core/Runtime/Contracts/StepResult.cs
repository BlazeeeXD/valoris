namespace Valoris.Core.Contracts
{
    public sealed class StepResult
    {
        public GridPosition Position { get; set; }
        public GridObservation Observation { get; set; }
        public double Reward { get; set; }
        public bool IsTerminal { get; set; }
        public TerminationReason TerminationReason { get; set; }
    }
}
