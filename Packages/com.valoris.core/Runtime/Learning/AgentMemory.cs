namespace Valoris.Core.Learning
{
    public sealed class AgentMemory
    {
        public QTable QTable { get; } = new QTable();
        public long UpdateCount { get; set; }
        public int CampaignLevelCount { get; set; }

        public void Reset()
        {
            QTable.Clear();
            UpdateCount = 0;
            CampaignLevelCount = 0;
        }
    }
}
