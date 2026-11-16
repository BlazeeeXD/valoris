namespace Valoris.Core.Contracts
{
    public interface IOralisAgent
    {
        void BeginCampaign(int campaignSeed);
        TrainingResult TrainLevel(LevelDefinition level, TrainingOptions options);
        AgentMemorySummary GetMemorySummary();
    }
}
