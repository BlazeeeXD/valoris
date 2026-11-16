using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;

namespace Valoris.Core.Training
{
    public static class MetricsCollector
    {
        public static double SuccessRate(IReadOnlyList<EpisodeMetric> metrics, int startIndex, int count)
        {
            if (metrics == null) throw new ArgumentNullException(nameof(metrics));
            if (startIndex < 0 || startIndex > metrics.Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > metrics.Count) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return 0.0;

            int successes = 0;
            for (int i = startIndex; i < startIndex + count; i++)
                if (metrics[i].ReachedGoal) successes++;
            return (double)successes / count;
        }

        public static double AverageReward(IReadOnlyList<EpisodeMetric> metrics, int startIndex, int count)
        {
            if (metrics == null) throw new ArgumentNullException(nameof(metrics));
            if (startIndex < 0 || startIndex > metrics.Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > metrics.Count) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return 0.0;

            double sum = 0.0;
            for (int i = startIndex; i < startIndex + count; i++) sum += metrics[i].TotalReward;
            return sum / count;
        }
    }
}
