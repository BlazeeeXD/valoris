using System;

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

        public int ChooseAction(int state, double epsilon)
        {
            if (_random.NextDouble() < epsilon) return _random.Next(4);
            return _qTable.SelectGreedyAction(state, _random, true);
        }

        public int ChooseGreedyAction(int state)
        {
            return _qTable.SelectGreedyAction(state, _random, false);
        }
    }
}
