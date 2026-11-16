public static class CampaignRules
{
    public const int DefaultLevelCount = 15;
    public const int WinsRequired = 8;

    public static bool IsCampaignComplete(CampaignState state)
    {
        if (state == null) return false;
        return state.AiScore >= WinsRequired || state.PlayerScore >= WinsRequired ||
               state.CompletedLevels >= state.MaxLevels;
    }

    public static bool DidAiWin(CampaignState state)
        => state != null && state.AiScore > state.PlayerScore;
}
