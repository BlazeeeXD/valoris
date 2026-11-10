using System.Collections.Generic;

namespace Valoris.Core.Grid
{
    public sealed class LevelValidationResult
    {
        public bool IsValid { get; set; }
        public bool HasSafePath { get; set; }
        public IReadOnlyList<string> Messages { get; set; }
    }
}
