using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    public sealed class TransferTrainingRunner
    {
        public IReadOnlyList<EpisodeMetric> Train(
            GridEnvironment environment,
            TransferPolicy policy,
            TrainingOptions options,
            Action onUpdate = null)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            var metrics =
                new List<EpisodeMetric>(options.EpisodesPerLevel);

            var episodeRunner = new TransferEpisodeRunner();

            double epsilon = options.InitialEpsilon;

            for (int episode = 1;
                 episode <= options.EpisodesPerLevel;
                 episode++)
            {
                EpisodeMetric metric = episodeRunner.Run(
                    environment,
                    policy,
                    options,
                    episode,
                    epsilon,
                    onUpdate);

                metrics.Add(metric);

                epsilon = Math.Max(
                    options.MinimumEpsilon,
                    epsilon * options.EpsilonDecay);
            }

            return metrics.AsReadOnly();
        }
    }
}