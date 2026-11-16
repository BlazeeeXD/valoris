using Valoris.Core.Contracts;

/// <summary>Per-level data passed between gameplay, integration and presentation.</summary>
public sealed class GameSession
{
    public GeneratedLevelData MutableLevel { get; private set; }
    public LevelDefinition LockedSnapshot { get; private set; }
    public TrainingResult LastTrainingResult { get; private set; }
    public bool IsTraining { get; set; }

    public void BeginLevel(GeneratedLevelData level)
    {
        MutableLevel = level;
        LockedSnapshot = null;
        LastTrainingResult = null;
        IsTraining = false;
    }

    public void Lock(LevelDefinition snapshot) => LockedSnapshot = snapshot;
    public void Complete(TrainingResult result) => LastTrainingResult = result;
}
