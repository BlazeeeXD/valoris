using System.Collections.Generic;

namespace Valoris.Core.Diagnostics
{
    public sealed class SimulationTrace
    {
        private readonly List<SimulationEvent> _events = new List<SimulationEvent>();
        public IReadOnlyList<SimulationEvent> Events => _events.AsReadOnly();
        public void Add(SimulationEvent item)
        {
            if (item == null) throw new System.ArgumentNullException(nameof(item));
            _events.Add(item);
        }
        public void Clear() => _events.Clear();
    }
}
