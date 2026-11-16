using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    public sealed class TransferPolicyEvaluator
    {
        // Normal mode uses this short-term memory only during final path replay.
        // It does not alter training, either Q-table, or the learned policy.
        private const int RecentPathWindow = 8;

        public EvaluationResult Evaluate(
            GridEnvironment environment,
            TransferPolicy policy,
            int maxSteps,
            double timeoutPenalty,
            bool preventShortLoops = false)
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
                GridObservation observation = environment.Observe();

                int action = preventShortLoops
                    ? ChooseLoopAwareAction(observation, policy, path)
                    : policy.ChooseGreedyAction(observation);

                if (action < 0)
                {
                    totalReward += timeoutPenalty;
                    reason = TerminationReason.NoAvailableMoves;
                    break;
                }

                StepResult transition = environment.Step((MoveAction)action);
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
                ReachedGoal = reason == TerminationReason.ReachedGoal,
                StepsTaken = steps,
                TotalReward = totalReward,
                TerminationReason = reason,
                Path = path.AsReadOnly()
            };
        }

        private static int ChooseLoopAwareAction(
            GridObservation observation,
            TransferPolicy policy,
            List<GridPosition> path)
        {
            int preferredAction = policy.ChooseGreedyAction(observation);
            if (preferredAction < 0)
                return -1;

            GridPosition preferredDestination = GridRules.Apply(
                observation.Position,
                (MoveAction)preferredAction);

            if (!WasVisitedRecently(path, preferredDestination))
                return preferredAction;

            // The learned policy wants to revisit a tile from the last few
            // moves. Prefer its highest-valued legal alternative that moves
            // outside that short loop. If this is a genuine dead end and all
            // legal moves require backtracking, fall back to the learned move.
            int alternative = -1;
            double bestValue = double.NegativeInfinity;
            int bestGoalDistance = int.MaxValue;

            for (int action = 0; action < ActionAvailability.ActionCount; action++)
            {
                if (!ActionAvailability.IsAvailable(observation, action))
                    continue;

                GridPosition destination = GridRules.Apply(
                    observation.Position,
                    (MoveAction)action);

                if (WasVisitedRecently(path, destination))
                    continue;

                double value = policy.GetCombinedValue(observation, action);
                int goalDistance = Math.Abs(destination.X - observation.Goal.X)
                                 + Math.Abs(destination.Y - observation.Goal.Y);

                if (value > bestValue + 1e-12 ||
                    (Math.Abs(value - bestValue) <= 1e-12 &&
                     goalDistance < bestGoalDistance))
                {
                    alternative = action;
                    bestValue = value;
                    bestGoalDistance = goalDistance;
                }
            }

            return alternative >= 0 ? alternative : preferredAction;
        }

        private static bool WasVisitedRecently(
            List<GridPosition> path,
            GridPosition candidate)
        {
            int firstIndex = Math.Max(0, path.Count - RecentPathWindow);
            for (int i = firstIndex; i < path.Count; i++)
            {
                if (path[i] == candidate)
                    return true;
            }

            return false;
        }
    }
}
