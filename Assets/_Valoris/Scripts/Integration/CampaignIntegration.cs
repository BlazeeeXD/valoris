using System;
using Valoris.Core.Contracts;

/// <summary>Unity-to-Core application boundary for the one-agent-per-campaign lifecycle.</summary>
public sealed class CampaignIntegration
{
    private readonly ValorisAgentBridge _bridge;

    public CampaignIntegration(ValorisAgentBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    public void BeginCampaign(int campaignSeed) => _bridge.BeginCampaign(campaignSeed);

    public TrainingResult TrainLockedLevel(LevelDefinition snapshot, TrainingOptions options)
        => _bridge.TrainLevel(snapshot, options);

    public AgentMemorySummary GetMemorySummary() => _bridge.GetMemorySummary();
}
