using System;
using Valoris.Core.Contracts;
using Valoris.Core.Learning;
using Xunit;

namespace Valoris.Core.Tests.Learning
{
    public sealed class TransferPolicyTests
    {
        private static GridObservation OpenObservation()
        {
            return new GridObservation(
                new GridPosition(5, 5),
                new GridPosition(22, 22),
                CellType.Empty,
                CellType.Empty,
                CellType.Empty,
                CellType.Empty);
        }

        private static TransferPolicy CreatePolicy(
            QTable spatial,
            QTable contextual)
        {
            return new TransferPolicy(
                spatial,
                contextual,
                new StateEncoder(),
                new ContextEncoder(),
                new Random(42),
                0.30);
        }

        [Fact]
        public void ContextualKnowledgeInfluencesActionSelection()
        {
            var spatial = new QTable();
            var contextual = new QTable();

            var observation = OpenObservation();

            int contextState =
                new ContextEncoder().Encode(observation);

            contextual.Update(
                contextState,
                (int)MoveAction.Right,
                10.0,
                0.0,
                1.0,
                0.0,
                true);

            var policy = CreatePolicy(spatial, contextual);

            int selected =
                policy.ChooseGreedyAction(observation);

            Assert.Equal(
                (int)MoveAction.Right,
                selected);
        }

        [Fact]
        public void StrongSpatialKnowledgeCanOverrideContextualPreference()
        {
            var spatial = new QTable();
            var contextual = new QTable();

            var observation = OpenObservation();

            int spatialState =
                new StateEncoder().Encode(observation);

            int contextState =
                new ContextEncoder().Encode(observation);

            contextual.Update(
                contextState,
                (int)MoveAction.Right,
                100.0,
                0.0,
                1.0,
                0.0,
                true);

            spatial.Update(
                spatialState,
                (int)MoveAction.Left,
                1000.0,
                0.0,
                1.0,
                0.0,
                true);

            var policy = CreatePolicy(spatial, contextual);

            int selected =
                policy.ChooseGreedyAction(observation);

            Assert.Equal(
                (int)MoveAction.Left,
                selected);
        }

        [Fact]
        public void WallActionsAreNeverSelected()
        {
            var spatial = new QTable();
            var contextual = new QTable();

            var observation = new GridObservation(
                new GridPosition(5, 5),
                new GridPosition(22, 22),
                CellType.Wall,
                CellType.Empty,
                CellType.Empty,
                CellType.Empty);

            int state =
                new StateEncoder().Encode(observation);

            spatial.Update(
                state,
                (int)MoveAction.Up,
                10000.0,
                0.0,
                1.0,
                0.0,
                true);

            spatial.Update(
                state,
                (int)MoveAction.Right,
                10.0,
                0.0,
                1.0,
                0.0,
                true);

            var policy = CreatePolicy(spatial, contextual);

            int selected =
                policy.ChooseGreedyAction(observation);

            Assert.NotEqual(
                (int)MoveAction.Up,
                selected);

            Assert.Equal(
                (int)MoveAction.Right,
                selected);
        }


        [Fact]
        public void ContextualInfluenceDecaysPerStateActionVisitCount()
        {
            var spatial = new QTable();
            var contextual = new QTable();
            var observation = OpenObservation();
            var policy = CreatePolicy(spatial, contextual);
            int spatialState = new StateEncoder().Encode(observation);
            int action = (int)MoveAction.Right;

            Assert.Equal(0.30, policy.GetContextWeight(observation, action), 8);

            for (int i = 0; i < 20; i++)
                spatial.Update(spatialState, action, 0.0, 0.0, 1.0, 0.0, true);

            Assert.Equal(0.15, policy.GetContextWeight(observation, action), 8);
            Assert.Equal(0.30, policy.GetContextWeight(observation, (int)MoveAction.Left), 8);

            for (int i = 0; i < 40; i++)
                spatial.Update(spatialState, action, 0.0, 0.0, 1.0, 0.0, true);

            Assert.Equal(0.075, policy.GetContextWeight(observation, action), 8);
        }

        [Fact]
        public void LearningUpdatesBothTables()
        {
            var spatial = new QTable();
            var contextual = new QTable();

            var observation = OpenObservation();

            var policy = CreatePolicy(spatial, contextual);

            policy.Learn(
                observation,
                (int)MoveAction.Right,
                10.0,
                observation,
                true,
                0.15,
                0.95);

            Assert.True(spatial.LearnedStateCount > 0);
            Assert.True(contextual.LearnedStateCount > 0);
        }
    }
}