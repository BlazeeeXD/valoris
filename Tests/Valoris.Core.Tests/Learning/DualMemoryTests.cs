using Valoris.Core.Learning;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class DualMemoryTests
    {
        [Fact]
        public void BeginningNewLevelClearsOnlySpatialMemory()
        {
            var memory = new AgentMemory();

            memory.ContextQTable.Update(
                10, 1, 5, 0, 1, 0, true);

            memory.SpatialQTable.Update(
                20, 2, 8, 0, 1, 0, true);

            Assert.Equal(1, memory.ContextQTable.LearnedStateCount);
            Assert.Equal(1, memory.SpatialQTable.LearnedStateCount);

            memory.BeginLevel();

            Assert.Equal(1, memory.ContextQTable.LearnedStateCount);
            Assert.Equal(0, memory.SpatialQTable.LearnedStateCount);
        }

        [Fact]
        public void NewCampaignClearsBothMemories()
        {
            var memory = new AgentMemory();

            memory.ContextQTable.Update(
                10, 1, 5, 0, 1, 0, true);

            memory.SpatialQTable.Update(
                20, 2, 8, 0, 1, 0, true);

            memory.UpdateCount = 100;
            memory.CampaignLevelCount = 5;

            memory.Reset();

            Assert.Equal(0, memory.ContextQTable.LearnedStateCount);
            Assert.Equal(0, memory.SpatialQTable.LearnedStateCount);
            Assert.Equal(0, memory.UpdateCount);
            Assert.Equal(0, memory.CampaignLevelCount);
        }
    }
}