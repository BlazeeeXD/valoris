namespace Valoris.Core.Contracts
{
    /// <summary>Cell contents only. Start and Goal are stored as coordinates on LevelDefinition.</summary>
    public enum CellType : byte
    {
        Empty = 0,
        Wall = 1,
        Trap = 2,
        Mud = 3
    }
}
