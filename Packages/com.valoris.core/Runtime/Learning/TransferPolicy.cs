using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    public sealed class TransferPolicy
    {
        private readonly QTable _spatialQTable;
        private readonly QTable _contextQTable;

        private readonly StateEncoder _spatialEncoder;
        private readonly ContextEncoder _contextEncoder;

        private readonly Random _random;

        public double ContextWeight { get; }

        public double SpatialWeight => 1.0 - ContextWeight;

        public TransferPolicy(
            QTable spatialQTable,
            QTable contextQTable,
            StateEncoder spatialEncoder,
            ContextEncoder contextEncoder,
            Random random,
            double contextWeight = 0.30)
        {
            _spatialQTable = spatialQTable
                ?? throw new ArgumentNullException(nameof(spatialQTable));

            _contextQTable = contextQTable
                ?? throw new ArgumentNullException(nameof(contextQTable));

            _spatialEncoder = spatialEncoder
                ?? throw new ArgumentNullException(nameof(spatialEncoder));

            _contextEncoder = contextEncoder
                ?? throw new ArgumentNullException(nameof(contextEncoder));

            _random = random
                ?? throw new ArgumentNullException(nameof(random));

            if (contextWeight < 0.0 || contextWeight > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contextWeight));
            }

            ContextWeight = contextWeight;
        }

        public int EncodeSpatial(GridObservation observation)
        {
            return _spatialEncoder.Encode(observation);
        }

        public int EncodeContext(GridObservation observation)
        {
            return _contextEncoder.Encode(observation);
        }

        /// <summary>
        /// Combines spatial and contextual Q-values for an action.
        /// Only legal actions are considered.
        /// </summary>
        public double GetCombinedValue(
            GridObservation observation,
            int action)
        {
            int spatialState = EncodeSpatial(observation);
            int contextState = EncodeContext(observation);

            double spatialValue =
                _spatialQTable.GetValue(spatialState, action);

            double contextValue =
                _contextQTable.GetValue(contextState, action);

            return SpatialWeight * spatialValue
                 + ContextWeight * contextValue;
        }

        public int ChooseAction(
            GridObservation observation,
            double epsilon)
        {
            if (epsilon < 0.0 || epsilon > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(epsilon));
            }

            int availableCount =
                ActionAvailability.CountAvailable(observation);

            if (availableCount == 0)
                return -1;

            // Exploration.
            if (_random.NextDouble() < epsilon)
            {
                return ActionAvailability.GetAvailableAt(
                    observation,
                    _random.Next(availableCount));
            }

            return ChooseGreedyAction(observation);
        }

        public int ChooseGreedyAction(GridObservation observation)
        {
            int bestAction = -1;
            double bestValue = double.NegativeInfinity;

            int[] tiedActions = new int[4];
            int tieCount = 0;

            for (int action = 0; action < 4; action++)
            {
                if (!ActionAvailability.IsAvailable(
                    observation, action))
                {
                    continue;
                }

                double value =
                    GetCombinedValue(observation, action);

                if (value > bestValue + 1e-12)
                {
                    bestValue = value;
                    bestAction = action;

                    tiedActions[0] = action;
                    tieCount = 1;
                }
                else if (Math.Abs(value - bestValue) <= 1e-12)
                {
                    tiedActions[tieCount++] = action;
                }
            }

            if (tieCount == 0)
                return -1;

            return tiedActions[_random.Next(tieCount)];
        }

        /// <summary>
        /// Updates both tables independently using standard Q-learning.
        /// Spatial learning is map-specific.
        /// Contextual learning is transferable.
        /// </summary>
        public void Learn(
            GridObservation currentObservation,
            int action,
            double reward,
            GridObservation nextObservation,
            bool terminal,
            double learningRate,
            double discountFactor)
        {
            int spatialState =
                EncodeSpatial(currentObservation);

            int contextState =
                EncodeContext(currentObservation);

            int nextSpatialState =
                EncodeSpatial(nextObservation);

            int nextContextState =
                EncodeContext(nextObservation);

            double spatialMaxNext = terminal
                ? 0.0
                : _spatialQTable.GetMaxValue(
                    nextSpatialState, nextObservation);

            double contextMaxNext = terminal
                ? 0.0
                : _contextQTable.GetMaxValue(
                    nextContextState, nextObservation);

            _spatialQTable.Update(
                spatialState,
                action,
                reward,
                spatialMaxNext,
                learningRate,
                discountFactor,
                terminal);

            _contextQTable.Update(
                contextState,
                action,
                reward,
                contextMaxNext,
                learningRate,
                discountFactor,
                terminal);
        }
    }
}