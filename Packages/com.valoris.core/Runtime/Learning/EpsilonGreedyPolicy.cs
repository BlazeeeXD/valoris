using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Learning
{
    public sealed class EpsilonGreedyPolicy
    {
        private readonly QTable _qTable;
        private readonly Random _random;

        public EpsilonGreedyPolicy(QTable qTable, Random random)
        {
            _qTable = qTable ?? throw new ArgumentNullException(nameof(qTable));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// Epsilon-greedy selection over legal actions only. Returns -1 only when the
        /// observation has no available move; valid campaign maps should not start that way.
        /// </summary>
        public int ChooseAction(int state, GridObservation observation, double epsilon)
        {
            if (epsilon < 0.0 || epsilon > 1.0)
                throw new ArgumentOutOfRangeException(nameof(epsilon));

            int availableCount = ActionAvailability.CountAvailable(observation);
            if (availableCount == 0) return -1;

            if (_random.NextDouble() < epsilon)
            {
                int selected = _random.Next(availableCount);
                return ActionAvailability.GetAvailableAt(observation, selected);
            }

            return _qTable.SelectGreedyAction(state, observation, _random);
        }

        /// <summary>Greedy evaluation with seeded random tie-breaking over legal actions.</summary>
        public int ChooseGreedyAction(int state, GridObservation observation)
        {
            return _qTable.SelectGreedyAction(state, observation, _random);
        }
    }
}
