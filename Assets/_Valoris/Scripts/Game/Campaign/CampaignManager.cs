
using UnityEngine;

public class CampaignManager : MonoBehaviour
{
    [Header("Campaign Settings")]
    [SerializeField] private int maxLevels = CampaignRules.MaxCampaignLevels;
    [SerializeField] private int winsRequired = CampaignRules.WinsRequired;

    [Header("Campaign Score")]
    [SerializeField] private int playerScore;
    [SerializeField] private int aiScore;
    [SerializeField] private int levelsCompleted;

    public int PlayerScore => playerScore;
    public int AIScore => aiScore;
    public int LevelsCompleted => levelsCompleted;

    public bool IsCampaignOver =>
        playerScore >= winsRequired ||
        aiScore >= winsRequired ||
        levelsCompleted >= maxLevels;

    public void BeginCampaign()
    {
        playerScore = 0;
        aiScore = 0;
        levelsCompleted = 0;

        Debug.Log("Campaign started!");
    }

    public void CompleteLevel(bool aiReachedGoal)
    {
        if (IsCampaignOver)
        {
            Debug.LogWarning("Campaign is already over.");
            return;
        }

        if (aiReachedGoal)
        {
            aiScore++;
            Debug.Log("AI successfully reached the goal. AI gets a point.");
        }
        else
        {
            playerScore++;
            Debug.Log("AI failed to reach the goal. Player gets a point.");
        }

        levelsCompleted++;

        Debug.Log(
            $"Level {levelsCompleted} completed. " +
            $"Player: {playerScore} | AI: {aiScore}"
        );

        if (IsCampaignOver)
        {
            Debug.Log("CAMPAIGN OVER!");

            if (playerScore >= winsRequired)
                Debug.Log("PLAYER WINS THE CAMPAIGN!");
            else if (aiScore >= winsRequired)
                Debug.Log("AI WINS THE CAMPAIGN!");
            else
                Debug.Log("Campaign ended after maximum levels.");
        }
    }
}

