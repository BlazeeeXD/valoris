using System;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    /// <summary>Runs one training episode and updates the shared Q-table transition by transition.</summary>
    public sealed class EpisodeRunner
    {
        public EpisodeMetric Run(
            GridEnvironment environment,
            QTable qTable,
            StateEncoder encoder,
            TrainingOptions options,
            Random random,
            int episodeNumber,
            double epsilon,
            Action onUpdate = null)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (qTable == null) throw new ArgumentNullException(nameof(qTable));
            if (encoder == null) throw new ArgumentNullException(nameof(encoder));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (episodeNumber <= 0) throw new ArgumentOutOfRangeException(nameof(episodeNumber));
            if (epsilon < 0.0 || epsilon > 1.0) throw new ArgumentOutOfRangeException(nameof(epsilon));

            environment.ResetEpisode();
            GridObservation observation = environment.Observe();
            var policy = new EpsilonGreedyPolicy(qTable, random);
            double totalReward = 0.0;
            bool reachedGoal = false;
            TerminationReason reason = TerminationReason.None;
            int steps = 0;

            for (int step = 1; step <= options.MaxStepsPerEpisode; step++)
            {
                int state = encoder.Encode(observation);
                int actionIndex = policy.ChooseAction(state, observation, epsilon);
                if (actionIndex < 0)
                {
                    totalReward += options.Rewards.TimeoutPenalty;
                    reason = TerminationReason.NoAvailableMoves;
                    break;
                }

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
                double maxNext = terminal
                    ? 0.0
                    : qTable.GetMaxValue(nextState, transition.Observation);

                qTable.Update(state, actionIndex, reward, maxNext,
                    options.LearningRate, options.DiscountFactor, terminal);
                onUpdate?.Invoke();

                totalReward += reward;
                observation = transition.Observation;
                steps = step;
                reachedGoal = reason == TerminationReason.ReachedGoal;
                if (terminal) break;
            }

            return new EpisodeMetric
            {
                EpisodeNumber = episodeNumber,
                TotalReward = totalReward,
                Steps = steps,
                ReachedGoal = reachedGoal,
                Epsilon = epsilon,
                TerminationReason = reason
            };
        }
    }
}
