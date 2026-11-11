using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    public sealed class TransferPolicyEvaluator
    {
        public EvaluationResult Evaluate(
            GridEnvironment environment,
            TransferPolicy policy,
            int maxSteps,
            double timeoutPenalty)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            if (maxSteps <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSteps));

            environment.ResetEpisode();

            var path = new List<GridPosition>
            {
                environment.Position
            };

            double totalReward = 0.0;

            TerminationReason reason = TerminationReason.None;

            int steps = 0;

            for (int step = 1; step <= maxSteps; step++)
            {
                GridObservation observation =
                    environment.Observe();

                int action = policy.ChooseGreedyAction(observation);

                if (action < 0)
                {
                    totalReward += timeoutPenalty;
                    reason = TerminationReason.NoAvailableMoves;
                    break;
                }

                StepResult transition = environment.Step(
                    (MoveAction)action);

                totalReward += transition.Reward;

                path.Add(transition.Position);

                steps = step;

                reason = transition.TerminationReason;

                if (transition.IsTerminal)
                    break;

                if (step == maxSteps)
                {
                    totalReward += timeoutPenalty;
                    reason = TerminationReason.StepLimit;
                }
            }

            return new EvaluationResult
            {
                ReachedGoal =
                    reason == TerminationReason.ReachedGoal,

                StepsTaken = steps,

                TotalReward = totalReward,

                TerminationReason = reason,

                Path = path.AsReadOnly()
            };
        }
    }
}