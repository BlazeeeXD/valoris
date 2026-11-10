namespace Valoris.Core.Contracts
{
    public enum TerminationReason : byte
    {
        None = 0,
        ReachedGoal = 1,
        EnteredTrap = 2,
        StepLimit = 3
    }
}
