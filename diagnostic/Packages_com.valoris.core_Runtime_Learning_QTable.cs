using System.Collections.Generic;

namespace Valoris.Core.Learning
{
    public sealed class QTable
    {
        private readonly Dictionary<int, double[]> _values = new Dictionary<int, double[]>();
        public int LearnedStateCount => _values.Count;
        public int EntryCount => _values.Count * 4;

        public double GetValue(int state, int action)
        {
            ValidateAction(action);
            return _values.TryGetValue(state, out double[] values) ? values[action] : 0.0;
        }

        public double GetMaxValue(int state)
        {
            if (!_values.TryGetValue(state, out double[] values)) return 0.0;
            double max = values[0];
            for (int i = 1; i < values.Length; i++) if (values[i] > max) max = values[i];
            return max;
        }

        public double Update(int state, int action, double reward, double maxNextValue,
            double learningRate, double discountFactor, bool terminal)
        {
            ValidateAction(action);
            if (learningRate <= 0 || learningRate > 1) throw new System.ArgumentOutOfRangeException(nameof(learningRate));
            if (discountFactor < 0 || discountFactor > 1) throw new System.ArgumentOutOfRangeException(nameof(discountFactor));
            if (!_values.TryGetValue(state, out double[] values))
            {
                values = new double[4];
                _values.Add(state, values);
            }

            double oldValue = values[action];
            double target = terminal ? reward : reward + discountFactor * maxNextValue;
            double updated = oldValue + learningRate * (target - oldValue);
            values[action] = updated;
            return updated;
        }

        public int SelectGreedyAction(int state, System.Random random, bool randomizeTies)
        {
            if (random == null) throw new System.ArgumentNullException(nameof(random));
            if (!_values.TryGetValue(state, out double[] values)) return randomizeTies ? random.Next(4) : 0;

            double max = values[0];
            for (int i = 1; i < values.Length; i++) if (values[i] > max) max = values[i];

            int[] ties = new int[4];
            int count = 0;
            for (int i = 0; i < values.Length; i++)
                if (System.Math.Abs(values[i] - max) < 1e-12) ties[count++] = i;

            return randomizeTies ? ties[random.Next(count)] : ties[0];
        }

        public void Clear() => _values.Clear();

        private static void ValidateAction(int action)
        {
            if (action < 0 || action >= 4) throw new System.ArgumentOutOfRangeException(nameof(action));
        }
    }
}
