namespace Valoris.Core.Contracts
{
    public sealed class AgentMemorySummary
    {
        // Combined counts for compatibility.
        public int LearnedStateCount { get; set; }
        public int QTableEntryCount { get; set; }

        // Separate memory statistics.
        public int ContextLearnedStateCount { get; set; }
        public int SpatialLearnedStateCount { get; set; }

        public int ContextQTableEntryCount { get; set; }
        public int SpatialQTableEntryCount { get; set; }

        public long UpdateCount { get; set; }

        public int CampaignLevelCount { get; set; }
    }
}