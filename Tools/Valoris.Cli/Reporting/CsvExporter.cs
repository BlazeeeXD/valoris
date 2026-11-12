using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Valoris.Core.Contracts;

namespace Valoris.Core.Cli.Reporting
{
    /// <summary>Exports episode metrics together with the experiment configuration needed to reproduce them.</summary>
    public static class CsvExporter
    {
        public static void Export(
            IReadOnlyList<EpisodeMetric> metrics,
            string path,
            LevelDefinition level,
            TrainingOptions options,
            EvaluationResult evaluation)
        {
            if (metrics == null) throw new ArgumentNullException(nameof(metrics));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A CSV output path is required.", nameof(path));
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (evaluation == null) throw new ArgumentNullException(nameof(evaluation));

            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(false));
            writer.WriteLine("levelNumber,levelSeed,width,height,runSeed,episodesPerLevel,maxStepsPerEpisode,evaluationMaxSteps,learningRate,discountFactor,initialEpsilon,minimumEpsilon,epsilonDecay,goalReward,trapPenalty,wallPenalty,mudPenalty,stepPenalty,timeoutPenalty,episode,totalReward,steps,reachedGoal,epsilon,terminationReason,finalEvaluationReachedGoal,finalEvaluationSteps,finalEvaluationReward,finalTerminationReason");

            foreach (EpisodeMetric metric in metrics)
            {
                writer.WriteLine(string.Join(",",
                    level.LevelNumber.ToString(CultureInfo.InvariantCulture),
                    level.Seed.ToString(CultureInfo.InvariantCulture),
                    level.Width.ToString(CultureInfo.InvariantCulture),
                    level.Height.ToString(CultureInfo.InvariantCulture),
                    options.RandomSeed.ToString(CultureInfo.InvariantCulture),
                    options.EpisodesPerLevel.ToString(CultureInfo.InvariantCulture),
                    options.MaxStepsPerEpisode.ToString(CultureInfo.InvariantCulture),
                    options.EvaluationMaxSteps.ToString(CultureInfo.InvariantCulture),
                    Format(options.LearningRate),
                    Format(options.DiscountFactor),
                    Format(options.InitialEpsilon),
                    Format(options.MinimumEpsilon),
                    Format(options.EpsilonDecay),
                    Format(options.Rewards.GoalReward),
                    Format(options.Rewards.TrapPenalty),
                    Format(options.Rewards.WallPenalty),
                    Format(options.Rewards.MudPenalty),
                    Format(options.Rewards.StepPenalty),
                    Format(options.Rewards.TimeoutPenalty),
                    metric.EpisodeNumber.ToString(CultureInfo.InvariantCulture),
                    Format(metric.TotalReward),
                    metric.Steps.ToString(CultureInfo.InvariantCulture),
                    metric.ReachedGoal ? "true" : "false",
                    Format(metric.Epsilon),
                    metric.TerminationReason.ToString(),
                    evaluation.ReachedGoal ? "true" : "false",
                    evaluation.StepsTaken.ToString(CultureInfo.InvariantCulture),
                    Format(evaluation.TotalReward),
                    evaluation.TerminationReason.ToString()));
            }
        }

        private static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    }
}
