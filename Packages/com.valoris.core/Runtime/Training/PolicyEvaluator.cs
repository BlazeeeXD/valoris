using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    public sealed class PolicyEvaluator
    {
        public EvaluationResult Evaluate(GridEnvironment environment, QTable qTable,
            StateEncoder encoder, int maxSteps, Random random, double timeoutPenalty)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (qTable == null) throw new ArgumentNullException(nameof(qTable));
            if (encoder == null) throw new ArgumentNullException(nameof(encoder));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (maxSteps <= 0) throw new ArgumentOutOfRangeException(nameof(maxSteps));

            environment.ResetEpisode();
            var path = new List<GridPosition> { environment.Position };
            var policy = new EpsilonGreedyPolicy(qTable, random);
            double totalReward = 0.0;
            TerminationReason reason = TerminationReason.None;
            int steps = 0;

            for (int step = 1; step <= maxSteps; step++)
            {
                GridObservation observation = environment.Observe();
                int state = encoder.Encode(observation);
                int actionIndex = policy.ChooseGreedyAction(state);
                StepResult transition = environment.Step((MoveAction)actionIndex);
                totalReward += transition.Reward;
                path.Add(transition.Position);
                steps = step;
                reason = transition.TerminationReason;

                if (transition.IsTerminal) break;
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
    }
}
