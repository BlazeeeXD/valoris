using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;

namespace Valoris.Core.Training
{
    /// <summary>
    /// A deliberately lightweight local navigator for Easy mode.
    /// It scores only the four neighbouring cells; it does not train, search
    /// the whole map, or retain knowledge between levels.
    /// </summary>
    public sealed class GreedyHeuristicEvaluator
    {
        private const double RevisitPenalty = 0.25;
        private const double TrapPenalty = 1000.0;
        private const double MudPenalty = 0.5;

        public EvaluationResult Evaluate(
            GridEnvironment environment,
            int maxSteps,
            double timeoutPenalty,
            Random random)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (maxSteps <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSteps));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            environment.ResetEpisode();

            LevelDefinition level = environment.Level;
            var visitCounts = new int[level.Width, level.Height];
            visitCounts[level.Start.X, level.Start.Y] = 1;

            var path = new List<GridPosition> { environment.Position };
            double totalReward = 0.0;
            TerminationReason reason = TerminationReason.None;
            int steps = 0;

            for (int step = 1; step <= maxSteps; step++)
            {
                GridObservation observation = environment.Observe();
                int action = ChooseLocalAction(
                    environment,
                    observation,
                    visitCounts,
                    random);

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

                int x = transition.Position.X;
                int y = transition.Position.Y;
                if (x >= 0 && y >= 0 && x < level.Width && y < level.Height)
                    visitCounts[x, y]++;

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

        private static int ChooseLocalAction(
            GridEnvironment environment,
            GridObservation observation,
            int[,] visitCounts,
            Random random)
        {
            double bestScore = double.PositiveInfinity;
            int[] bestActions = new int[ActionAvailability.ActionCount];
            int bestCount = 0;

            for (int action = 0; action < ActionAvailability.ActionCount; action++)
            {
                if (!ActionAvailability.IsAvailable(observation, action))
                    continue;

                MoveAction move = (MoveAction)action;
                GridPosition next = GridRules.Apply(observation.Position, move);
                CellType terrain = GridRules.GetNeighbourCell(
                    environment.Level,
                    observation.Position,
                    move);

                int distance =
                    Math.Abs(next.X - observation.Goal.X) +
                    Math.Abs(next.Y - observation.Goal.Y);

                double score = distance +
                               RevisitPenalty * visitCounts[next.X, next.Y];

                if (terrain == CellType.Trap)
                    score += TrapPenalty;
                else if (terrain == CellType.Mud)
                    score += MudPenalty;

                if (score < bestScore - 1e-12)
                {
                    bestScore = score;
                    bestActions[0] = action;
                    bestCount = 1;
                }
                else if (Math.Abs(score - bestScore) <= 1e-12)
                {
                    bestActions[bestCount++] = action;
                }
            }

            if (bestCount == 0)
                return -1;

            return bestActions[random.Next(bestCount)];
        }
    }
}
