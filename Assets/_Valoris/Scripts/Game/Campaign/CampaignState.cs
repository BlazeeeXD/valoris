using System;

[Serializable]
public class CampaignState
{
    public bool boardLocked { get; private set; }
    public bool IsBoardLocked => boardLocked;
    public int CampaignSeed { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int CompletedLevels { get; private set; }
    public int MaxLevels { get; private set; } = CampaignRules.DefaultLevelCount;
    public int PlayerScore { get; private set; }
    public int AiScore { get; private set; }
    public bool IsComplete { get; private set; }

    public void BeginCampaign(int seed, int maxLevels)
    {
        CampaignSeed = seed;
        MaxLevels = Math.Max(1, maxLevels);
        CurrentLevel = 1;
        CompletedLevels = 0;
        PlayerScore = 0;
        AiScore = 0;
        boardLocked = false;
        IsComplete = false;
    }

    public bool LockBoard()
    {
        if (boardLocked || IsComplete) return false;
        boardLocked = true;
        return true;
    }

    public void ResetBoard() => boardLocked = false;

    public void RecordLevelResult(bool aiReachedGoal)
    {
        if (!boardLocked) throw new InvalidOperationException("A result cannot be recorded before the level is locked.");
        if (IsComplete) throw new InvalidOperationException("The campaign is already complete.");

        if (aiReachedGoal) AiScore++;
        else PlayerScore++;
        CompletedLevels++;
        // The just-completed board is no longer locked. The next level must be
        // lockable independently; keeping this true silently breaks round 2+.
        boardLocked = false;
        IsComplete = CampaignRules.IsCampaignComplete(this);
        if (!IsComplete) CurrentLevel++;
    }
}
