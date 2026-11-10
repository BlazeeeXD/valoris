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
            var policy = new EpsilonGreedyPolicy(qTable, random);
            double epsilon = options.InitialEpsilon;

            for (int episode = 1; episode <= options.EpisodesPerLevel; episode++)
            {
                environment.ResetEpisode();
                GridObservation observation = environment.Observe();
                double totalReward = 0.0;
                bool reachedGoal = false;
                TerminationReason reason = TerminationReason.None;
                int steps = 0;

                for (int step = 1; step <= options.MaxStepsPerEpisode; step++)
                {
                    int state = encoder.Encode(observation);
                    int actionIndex = policy.ChooseAction(state, epsilon);
                    StepResult transition = environment.Step((MoveAction)actionIndex);
                    double reward = transition.Reward;
                    bool terminal = transition.IsTerminal;
                    reason = transition.TerminationReason;

                    if (!terminal && step == options.MaxStepsPerEpisode)
                    {
                        reward += options.Rewards.TimeoutPenalty;
                        terminal = true;
                        reason = TerminationReason.StepLimit;
                    }

                    int nextState = encoder.Encode(transition.Observation);
                    double maxNext = terminal ? 0.0 : qTable.GetMaxValue(nextState);
                    qTable.Update(state, actionIndex, reward, maxNext,
                        options.LearningRate, options.DiscountFactor, terminal);
                    onUpdate?.Invoke();

                    totalReward += reward;
                    observation = transition.Observation;
                    steps = step;
                    reachedGoal = reason == TerminationReason.ReachedGoal;
                    if (terminal) break;
                }

                metrics.Add(new EpisodeMetric
                {
                    EpisodeNumber = episode,
                    TotalReward = totalReward,
                    Steps = steps,
                    ReachedGoal = reachedGoal,
                    Epsilon = epsilon,
                    TerminationReason = reason
                });

                epsilon = System.Math.Max(options.MinimumEpsilon, epsilon * options.EpsilonDecay);
            }

            return metrics.AsReadOnly();
        }
    }
}
