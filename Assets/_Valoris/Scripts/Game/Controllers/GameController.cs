
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CampaignManager campaignManager;
    [SerializeField] private ProceduralLevelGenerator levelGenerator;
    [SerializeField] private PlacementController placementController;

    private GameSession gameSession;

    public GameSession Session => gameSession;

    private void Awake()
    {
        gameSession = new GameSession();
    }

    public void StartNewCampaign()
    {
        if (campaignManager == null)
        {
            Debug.LogError("GameController: CampaignManager reference is missing!");
            return;
        }

        gameSession.StartSession();
        campaignManager.BeginCampaign();

        GenerateCurrentLevel();

        Debug.Log("New Oralis campaign started.");
    }

    public void GenerateCurrentLevel()
    {
        if (!gameSession.isCampaignActive)
            return;

        if (levelGenerator == null)
        {
            Debug.LogError("GameController: LevelGenerator reference is missing!");
            return;
        }

        levelGenerator.levelNumber = gameSession.currentLevel;
        levelGenerator.GenerateLevel();

        if (placementController != null)
            placementController.ResetBoard();

        Debug.Log($"Level {gameSession.currentLevel} is ready.");
    }

    public void CompleteCurrentLevel(bool aiReachedGoal)
    {
        if (!gameSession.isCampaignActive)
            return;

        campaignManager.CompleteLevel(aiReachedGoal);

        gameSession.RecordLevelResult(
            campaignManager.IsCampaignOver
        );

        if (gameSession.isCampaignComplete)
        {
            Debug.Log(
                $"Campaign finished! Player: {campaignManager.PlayerScore} | " +
                $"AI: {campaignManager.AIScore}"
            );
            return;
        }

        GenerateCurrentLevel();
    }
}
