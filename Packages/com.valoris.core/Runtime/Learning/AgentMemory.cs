namespace Valoris.Core.Learning
{
    public sealed class AgentMemory
    {
        // General knowledge that survives level transitions.
        public QTable ContextQTable { get; } = new QTable();

        // Map-specific knowledge that is cleared for a new level.
        public QTable SpatialQTable { get; } = new QTable();

        // Backwards compatibility for existing code using QTable.
        public QTable QTable => SpatialQTable;

        // Counts one environment transition where both tables are updated.
        public long UpdateCount { get; set; }

        public int CampaignLevelCount { get; set; }

        /// <summary>
        /// Called when a new level begins.
        /// Retains contextual knowledge but clears spatial knowledge.
        /// </summary>
        public void BeginLevel()
        {
            SpatialQTable.Clear();
        }

        /// <summary>
        /// Called when a new campaign begins.
        /// Clears all learned experience.
        /// </summary>
        public void Reset()
        {
            ContextQTable.Clear();
            SpatialQTable.Clear();

            UpdateCount = 0;
            CampaignLevelCount = 0;
        }
    }
}