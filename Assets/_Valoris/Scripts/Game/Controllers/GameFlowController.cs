
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private PlacementController placementController;

    public void StartCampaign()
    {
        if (gameController == null)
        {
            Debug.LogError("GameFlowController: GameController reference is missing!");
            return;
        }

        gameController.StartNewCampaign();
    }

    public void LockCurrentLevel()
    {
        if (placementController == null)
        {
            Debug.LogError("GameFlowController: PlacementController reference is missing!");
            return;
        }

        placementController.LockBoard();

        Debug.Log("Level locked. Ready for AI processing.");
    }

    public void ReportAIResult(bool aiReachedGoal)
    {
        if (gameController == null)
        {
            Debug.LogError("GameFlowController: GameController reference is missing!");
            return;
        }

        gameController.CompleteCurrentLevel(aiReachedGoal);
    }
}
