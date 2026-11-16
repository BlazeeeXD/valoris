
namespace Valoris.Core.Contracts
{
    public enum MoveAction : byte
    {
        // Normal movement
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,

        // Wall Breaker actions
        BreakUp = 4,
        BreakRight = 5,
        BreakDown = 6,
        BreakLeft = 7
    }
}
