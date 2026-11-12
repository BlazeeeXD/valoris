using System;
using System.Linq;
using Valoris.Core.Contracts;
using UnityEngine;

public class ResultsUIController : MonoBehaviour
{
    private CampaignHUDController _hud;
    public void Initialize(CampaignHUDController hud) => _hud = hud;

    public void ShowResult(bool aiWon, TrainingResult result, CampaignState campaign)
    {
        string outcome = aiWon ? "AI SUCCESS — AI earns one point." : "AI FAILURE — PLAYER earns one point.";
        string metrics = "";
        if (result != null && result.FinalEvaluation != null)
        {
            var episodes = result.EpisodeMetrics;
            double successRate = episodes == null || episodes.Count == 0
                ? 0.0
                : 100.0 * episodes.Count(metric => metric.ReachedGoal) / episodes.Count;
            double averageReward = episodes == null || episodes.Count == 0
                ? 0.0
                : episodes.Average(metric => metric.TotalReward);
            AgentMemorySummary memory = result.MemoryAfterTraining;
            metrics = "\nEvaluation: " + result.FinalEvaluation.TerminationReason +
                " | Steps: " + result.FinalEvaluation.StepsTaken +
                " | Reward: " + result.FinalEvaluation.TotalReward.ToString("0.0") +
                "\nTraining success: " + successRate.ToString("0.0") + "%" +
                " | Avg reward: " + averageReward.ToString("0.0");
            if (memory != null)
                metrics += "\nMemory: " + memory.ContextLearnedStateCount + " context states / " +
                    memory.SpatialLearnedStateCount + " spatial states / " + memory.UpdateCount + " updates";
        }
        string score = campaign == null ? "" :
            "\nScore: Player " + campaign.PlayerScore + " : " + campaign.AiScore + " AI";
        _hud?.SetStatus(outcome + metrics + score);
    }
}
