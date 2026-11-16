using Valoris.Core.Contracts;

public class TrainingUIController : UnityEngine.MonoBehaviour
{
    private CampaignHUDController _hud;
    public void Initialize(CampaignHUDController hud) => _hud = hud;

    public void ShowTraining(int levelNumber, int episodes)
        => _hud?.SetStatus("Level " + levelNumber + ": adaptive Q-learning for " + episodes + " episodes.\nThe final greedy evaluation will be replayed after training.");

    public void ShowEasy(int levelNumber, int maxSteps)
        => _hud?.SetStatus("Level " + levelNumber + ": Easy local heuristic. No Q-learning training is performed; the path budget is " + maxSteps + " moves.");

    public void ShowCompleted(TrainingResult result, AgentDifficulty difficulty)
    {
        if (result == null || result.FinalEvaluation == null) return;
        string mode = difficulty == AgentDifficulty.Easy
            ? "Easy navigation complete."
            : "Training complete.";
        _hud?.SetStatus(mode + " Evaluation: " + result.FinalEvaluation.TerminationReason +
            " in " + result.FinalEvaluation.StepsTaken + " steps. Replaying path...");
    }
}
