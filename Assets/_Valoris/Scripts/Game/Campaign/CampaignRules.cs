
public static class CampaignRules
{
    public const int MaxCampaignLevels = 15;
    public const int WinsRequired = 8;

    public static bool HasPlayerWon(int playerScore)
    {
        return playerScore >= WinsRequired;
    }

    public static bool HasAIWon(int aiScore)
    {
        return aiScore >= WinsRequired;
    }

    public static bool IsCampaignOver(
        int playerScore,
        int aiScore,
        int levelsCompleted)
    {
        return HasPlayerWon(playerScore) ||
               HasAIWon(aiScore) ||
               levelsCompleted >= MaxCampaignLevels;
    }

    public static bool IsValidScore(int score)
    {
        return score >= 0 && score <= MaxCampaignLevels;
    }

    public static bool CanStartNextLevel(
        int playerScore,
        int aiScore,
        int levelsCompleted)
    {
        return !IsCampaignOver(
            playerScore,
            aiScore,
            levelsCompleted
        );
    }
}
