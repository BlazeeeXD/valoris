
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
        private readonly double _contextDecayVisits;

        // This is the maximum contextual influence, not a fixed weight.
        public double ContextWeight { get; }

        // Spatial influence when the spatial action has zero visits.
        // Actual spatial influence increases adaptively as visits accumulate.
        public double SpatialWeight => 1.0 - ContextWeight;

        public double ContextDecayVisits => _contextDecayVisits;

        public TransferPolicy(
            QTable spatialQTable,
            QTable contextQTable,
            StateEncoder spatialEncoder,
            ContextEncoder contextEncoder,
            Random random,
            double contextWeight = 0.30,
            double contextDecayVisits = 20.0)
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

            if (double.IsNaN(contextWeight) ||
                double.IsInfinity(contextWeight) ||
                contextWeight < 0.0 ||
                contextWeight > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(contextWeight));
            }

            if (double.IsNaN(contextDecayVisits) ||
                double.IsInfinity(contextDecayVisits) ||
                contextDecayVisits <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contextDecayVisits));
            }

            ContextWeight = contextWeight;
            _contextDecayVisits = contextDecayVisits;
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
        /// Returns the contextual influence for this specific
        /// spatial state-action pair.
        ///
        /// No spatial experience: full configured contextual influence.
        /// More spatial experience: progressively less contextual influence.
        /// </summary>
        public double GetContextWeight(
            GridObservation observation,
            int action)
        {
            int spatialState = EncodeSpatial(observation);

            int visits = _spatialQTable.GetVisitCount(
                spatialState,
                action);

            return ContextWeight /
                   (1.0 + visits / _contextDecayVisits);
        }

        /// <summary>
        /// Combines spatial and contextual Q-values using
        /// adaptive, action-specific contextual weighting.
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

            double contextWeight =
                GetContextWeight(observation, action);

            double spatialWeight = 1.0 - contextWeight;

            return spatialWeight * spatialValue
                 + contextWeight * contextValue;
        }

        public int ChooseAction(
            GridObservation observation,
            double epsilon)
        {
            if (epsilon < 0.0 || epsilon > 1.0)
                throw new ArgumentOutOfRangeException(nameof(epsilon));

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
                    observation,
                    action))
                {
                    continue;
                }

                double value = GetCombinedValue(observation, action);

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
            int spatialState = EncodeSpatial(currentObservation);
            int contextState = EncodeContext(currentObservation);

            int nextSpatialState = EncodeSpatial(nextObservation);
            int nextContextState = EncodeContext(nextObservation);

            double spatialMaxNext = terminal
                ? 0.0
                : _spatialQTable.GetMaxValue(
                    nextSpatialState,
                    nextObservation);

            double contextMaxNext = terminal
                ? 0.0
                : _contextQTable.GetMaxValue(
                    nextContextState,
                    nextObservation);

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
