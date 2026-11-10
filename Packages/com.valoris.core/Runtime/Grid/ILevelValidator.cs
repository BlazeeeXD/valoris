using Valoris.Core.Contracts;

namespace Valoris.Core.Grid
{
    public interface ILevelValidator
    {
        LevelValidationResult Validate(LevelDefinition level);
    }
}
