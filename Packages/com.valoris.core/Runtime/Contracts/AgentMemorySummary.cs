namespace Valoris.Core.Contracts
{
    public sealed class AgentMemorySummary
    {
        public int LearnedStateCount { get; set; }
        public int QTableEntryCount { get; set; }
        public long UpdateCount { get; set; }
        public int CampaignLevelCount { get; set; }
    }
}
