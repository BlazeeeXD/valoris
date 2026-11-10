namespace Valoris.Core.Learning
{
    public static class QLearningUpdate
    {
        public static double Calculate(double oldValue, double reward, double maxNextValue,
            double learningRate, double discountFactor, bool terminal)
        {
            double target = terminal ? reward : reward + discountFactor * maxNextValue;
            return oldValue + learningRate * (target - oldValue);
        }
    }
}
