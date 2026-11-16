using System;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Learning;
using Valoris.Core.Training;
using Xunit;

namespace Valoris.Core.Tests.Training
{
    public sealed class NormalModeLoopPreventionTests
    {
        [Fact]
        public void NormalEvaluationBreaksAnImmediateBackAndForthLoopWhenAnAlternativeExists()
        {
            CreateOscillatingScenario(out GridEnvironment environment, out TransferPolicy policy);

            EvaluationResult result = new TransferPolicyEvaluator().Evaluate(
                environment,
                policy,
                maxSteps: 4,
                timeoutPenalty: -25.0,
                preventShortLoops: true);

            Assert.True(result.ReachedGoal);
            Assert.Equal(TerminationReason.ReachedGoal, result.TerminationReason);
            Assert.Equal(2, result.StepsTaken);
            Assert.Equal(new GridPosition(2, 2), result.Path[result.Path.Count - 1]);
        }

        [Fact]
        public void HardEvaluationKeepsTheOriginalGreedyBehaviour()
        {
            CreateOscillatingScenario(out GridEnvironment environment, out TransferPolicy policy);

            EvaluationResult result = new TransferPolicyEvaluator().Evaluate(
                environment,
                policy,
                maxSteps: 4,
                timeoutPenalty: -25.0);

            Assert.False(result.ReachedGoal);
            Assert.Equal(TerminationReason.StepLimit, result.TerminationReason);
            Assert.Equal(4, result.StepsTaken);
        }

        private static void CreateOscillatingScenario(
            out GridEnvironment environment,
            out TransferPolicy policy)
        {
            const int width = 24;
            const int height = 24;
            var cells = new int[width * height];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = (int)CellType.Wall;

            // The only safe route is Start -> Right -> Down -> Goal.
            cells[1 * width + 1] = (int)CellType.Empty;
            cells[1 * width + 2] = (int)CellType.Empty;
            cells[2 * width + 2] = (int)CellType.Empty;

            var level = new LevelDefinition(
                1, 1, 77, width, height, cells,
                new GridPosition(1, 1),
                new GridPosition(2, 2));

            environment = new GridEnvironment();
            environment.Reset(level);

            var spatial = new QTable();
            var contextual = new QTable();
            var encoder = new StateEncoder();

            GridObservation start = environment.Observe();
            spatial.Update(
                encoder.Encode(start),
                (int)MoveAction.Right,
                100.0, 0.0, 1.0, 0.0, true);

            environment.Step(MoveAction.Right);
            GridObservation corridor = environment.Observe();

            // Deliberately make the learned policy prefer walking back to Start.
            spatial.Update(
                encoder.Encode(corridor),
                (int)MoveAction.Left,
                100.0, 0.0, 1.0, 0.0, true);

            policy = new TransferPolicy(
                spatial,
                contextual,
                encoder,
                new ContextEncoder(),
                new Random(7),
                contextWeight: 0.30);
        }
    }
}
