
using System;
using System.Collections.Generic;
using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    public sealed class QTable
    {
        private readonly Dictionary<int, double[]> _values =
            new Dictionary<int, double[]>();

        private readonly Dictionary<int, int[]> _visits =
            new Dictionary<int, int[]>();

        public int LearnedStateCount => _values.Count;
        public int EntryCount => _values.Count * 4;

        public double GetValue(int state, int action)
        {
            ValidateAction(action);
            return _values.TryGetValue(state, out double[] values)
                ? values[action]
                : 0.0;
        }

        public int GetVisitCount(int state, int action)
        {
            ValidateAction(action);
            return _visits.TryGetValue(state, out int[] counts)
                ? counts[action]
                : 0;
        }

        /// <summary>
        /// Returns the maximum value over all actions.
        /// Use the observation overload during grid learning.
        /// </summary>
        public double GetMaxValue(int state)
        {
            if (!_values.TryGetValue(state, out double[] values))
                return 0.0;

            double max = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            return max;
        }

        /// <summary>
        /// Returns the maximum Q value among actions that do not
        /// enter walls or leave the board.
        /// </summary>
        public double GetMaxValue(
            int state,
            GridObservation observation)
        {
            if (!_values.TryGetValue(state, out double[] values))
                return 0.0;

            double max = double.NegativeInfinity;
            bool found = false;

            for (int action = 0; action < 4; action++)
            {
                if (!ActionAvailability.IsAvailable(observation, action))
                    continue;

                if (!found || values[action] > max)
                {
                    max = values[action];
                    found = true;
                }
            }

            return found ? max : 0.0;
        }

        public double Update(
            int state,
            int action,
            double reward,
            double maxNextValue,
            double learningRate,
            double discountFactor,
            bool terminal)
        {
            ValidateAction(action);

            if (learningRate <= 0 || learningRate > 1)
                throw new ArgumentOutOfRangeException(nameof(learningRate));

            if (discountFactor < 0 || discountFactor > 1)
                throw new ArgumentOutOfRangeException(nameof(discountFactor));

            if (!_values.TryGetValue(state, out double[] values))
            {
                values = new double[4];
                _values.Add(state, values);
            }

            if (!_visits.TryGetValue(state, out int[] counts))
            {
                counts = new int[4];
                _visits.Add(state, counts);
            }

            double oldValue = values[action];

            double updated = QLearningUpdate.Calculate(
                oldValue,
                reward,
                maxNextValue,
                learningRate,
                discountFactor,
                terminal);

            values[action] = updated;
            counts[action]++;

            return updated;
        }

        /// <summary>
        /// Selects randomly among equal-valued legal actions.
        /// </summary>
        public int SelectGreedyAction(
            int state,
            GridObservation observation,
            Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            int count = ActionAvailability.CountAvailable(observation);

            if (count == 0)
                return -1;

            _values.TryGetValue(state, out double[] values);

            double max = double.NegativeInfinity;
            int[] ties = new int[4];
            int tieCount = 0;

            for (int action = 0; action < 4; action++)
            {
                if (!ActionAvailability.IsAvailable(observation, action))
                    continue;

                double value = values == null ? 0.0 : values[action];

                if (value > max + 1e-12)
                {
                    max = value;
                    ties[0] = action;
                    tieCount = 1;
                }
                else if (Math.Abs(value - max) <= 1e-12)
                {
                    ties[tieCount++] = action;
                }
            }

            return tieCount == 0
                ? -1
                : ties[random.Next(tieCount)];
        }

        // Kept for diagnostics and tests that inspect an unmasked action vector.
        public int SelectGreedyAction(
            int state,
            Random random,
            bool randomizeTies)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            if (!_values.TryGetValue(state, out double[] values))
                return randomizeTies ? random.Next(4) : 0;

            double max = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            int selected = -1;
            int tieCount = 0;

            for (int action = 0; action < values.Length; action++)
            {
                if (Math.Abs(values[action] - max) > 1e-12)
                    continue;

                tieCount++;

                if (selected < 0 ||
                    (randomizeTies && random.Next(tieCount) == 0))
                {
                    selected = action;
                }
            }

            return selected;
        }

        public void Clear()
        {
            _values.Clear();
            _visits.Clear();
        }

        private static void ValidateAction(int action)
        {
            if (action < 0 || action >= 4)
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}
