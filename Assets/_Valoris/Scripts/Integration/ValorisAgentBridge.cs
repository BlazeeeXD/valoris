using System;
using Valoris.Core.Contracts;
using Valoris.Core.Training;

/// <summary>Owns exactly one AI agent for one campaign. No Unity objects enter the Core package.</summary>
public sealed class ValorisAgentBridge
{
    private IOralisAgent _agent;
    private bool _campaignStarted;

    public void BeginCampaign(int campaignSeed)
    {
        _agent = new ValorisAgent();
        _agent.BeginCampaign(campaignSeed);
        _campaignStarted = true;
    }

    public TrainingResult TrainLevel(LevelDefinition immutableLevel, TrainingOptions options)
    {
        if (!_campaignStarted || _agent == null)
            throw new InvalidOperationException("BeginCampaign must be called before TrainLevel.");
        if (immutableLevel == null) throw new ArgumentNullException(nameof(immutableLevel));
        if (options == null) throw new ArgumentNullException(nameof(options));
        return _agent.TrainLevel(immutableLevel, options);
    }

    public AgentMemorySummary GetMemorySummary()
    {
        if (!_campaignStarted || _agent == null)
            throw new InvalidOperationException("No campaign has been started.");
        return _agent.GetMemorySummary();
    }
}
