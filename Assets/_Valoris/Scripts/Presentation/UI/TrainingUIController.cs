using Valoris.Core.Contracts;

public class TrainingUIController : UnityEngine.MonoBehaviour
{
    private CampaignHUDController _hud;
    public void Initialize(CampaignHUDController hud) => _hud = hud;

    public void ShowTraining(int levelNumber, int episodes)
        => _hud?.SetStatus("Level " + levelNumber + ": training for " + episodes + " episodes.\nThe final greedy evaluation will be replayed after training.");

    public void ShowCompleted(TrainingResult result)
    {
        if (result == null || result.FinalEvaluation == null) return;
        _hud?.SetStatus("Training complete. Evaluation: " + result.FinalEvaluation.TerminationReason +
            " in " + result.FinalEvaluation.StepsTaken + " steps. Replaying path...");
    }
}
