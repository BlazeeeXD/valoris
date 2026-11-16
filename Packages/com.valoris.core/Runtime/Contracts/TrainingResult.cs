using System.Collections.Generic;

namespace Valoris.Core.Contracts
{
    public sealed class TrainingResult
    {
        public IReadOnlyList<EpisodeMetric> EpisodeMetrics { get; set; }
        public EvaluationResult FinalEvaluation { get; set; }
        public AgentMemorySummary MemoryAfterTraining { get; set; }
    }
}
