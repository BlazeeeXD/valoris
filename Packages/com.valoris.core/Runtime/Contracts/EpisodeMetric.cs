namespace Valoris.Core.Contracts
{
    public sealed class EpisodeMetric
    {
        public int EpisodeNumber { get; set; }
        public double TotalReward { get; set; }
        public int Steps { get; set; }
        public bool ReachedGoal { get; set; }
        public double Epsilon { get; set; }
        public TerminationReason TerminationReason { get; set; }
    }
}
