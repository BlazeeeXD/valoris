using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    public sealed class TrainingRunner
    {
        public IReadOnlyList<EpisodeMetric> Train(
            GridEnvironment environment,
            QTable qTable,
            StateEncoder encoder,
            TrainingOptions options,
            Random random,
            Action onUpdate)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (qTable == null) throw new ArgumentNullException(nameof(qTable));
            if (encoder == null) throw new ArgumentNullException(nameof(encoder));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (random == null) throw new ArgumentNullException(nameof(random));
            options.Validate();

            var metrics = new List<EpisodeMetric>(options.EpisodesPerLevel);
            var episodeRunner = new EpisodeRunner();
            double epsilon = options.InitialEpsilon;

            for (int episode = 1; episode <= options.EpisodesPerLevel; episode++)
            {
                EpisodeMetric metric = episodeRunner.Run(environment, qTable, encoder,
                    options, random, episode, epsilon, onUpdate);
                metrics.Add(metric);

                // Slower per-episode decay keeps exploration active long enough for sparse,
                // longer routes. The schedule remains configurable per experiment.
                epsilon = Math.Max(options.MinimumEpsilon, epsilon * options.EpsilonDecay);
            }

            return metrics.AsReadOnly();
        }
    }
}
