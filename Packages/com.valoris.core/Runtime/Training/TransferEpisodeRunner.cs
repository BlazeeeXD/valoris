using System;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    public sealed class TransferEpisodeRunner
    {
        public EpisodeMetric Run(
            GridEnvironment environment,
            TransferPolicy policy,
            TrainingOptions options,
            int episodeNumber,
            double epsilon,
            Action onUpdate = null)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            environment.ResetEpisode();

            GridObservation observation = environment.Observe();

            double totalReward = 0.0;

            int steps = 0;

            bool reachedGoal = false;

            TerminationReason reason = TerminationReason.None;

            for (int step = 1; step <= options.MaxStepsPerEpisode; step++)
            {
                int action = policy.ChooseAction(
                    observation,
                    epsilon);

                if (action < 0)
                {
                    totalReward += options.Rewards.TimeoutPenalty;
                    reason = TerminationReason.NoAvailableMoves;
                    break;
                }

                StepResult transition = environment.Step(
                    (MoveAction)action);

                double reward = transition.Reward;

                bool terminal = transition.IsTerminal;

                reason = transition.TerminationReason;

                if (!terminal && step == options.MaxStepsPerEpisode)
                {
                    reward += options.Rewards.TimeoutPenalty;

                    terminal = true;

                    reason = TerminationReason.StepLimit;
                }

                // Update spatial and contextual memories.
                policy.Learn(
                    observation,
                    action,
                    reward,
                    transition.Observation,
                    terminal,
                    options.LearningRate,
                    options.DiscountFactor);

                onUpdate?.Invoke();

                totalReward += reward;

                observation = transition.Observation;

                steps = step;

                reachedGoal =
                    reason == TerminationReason.ReachedGoal;

                if (terminal)
                    break;
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