using UnityEngine;

/// <summary>Owns campaign score and level progression. The AI never awards points itself.</summary>
public class CampaignManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxLevels = CampaignRules.DefaultLevelCount;
    [SerializeField] private int campaignSeed = 12345;

    public CampaignState State { get; private set; } = new CampaignState();
    public int CampaignSeed => campaignSeed;

    public void BeginCampaign(int seed)
    {
        campaignSeed = seed;
        State = new CampaignState();
        State.BeginCampaign(seed, maxLevels);
    }

    public void RecordLevelResult(bool aiReachedGoal)
    {
        State.RecordLevelResult(aiReachedGoal);
    }

    public bool CanStartNextLevel => !CampaignRules.IsCampaignComplete(State) && State.CompletedLevels < State.MaxLevels;

    public int GetSeedForLevel(int levelNumber)
    {
        unchecked { return campaignSeed * 31 + levelNumber * 1009; }
    }
}
