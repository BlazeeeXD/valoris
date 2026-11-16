using Valoris.Core.Contracts;
using Valoris.Core.Training;

namespace Valoris.Core.Cli.Reporting
{
    public static class ConsoleReporter
    {
        public static void PrintTraining(TrainingResult result, int requestedEpisodes)
        {
            const int windowSize = 100;
            int interval = System.Math.Max(1, requestedEpisodes / 10);
            IReadOnlyList<EpisodeMetric> metrics = result.EpisodeMetrics;

            Console.WriteLine("EPISODE | REWARD | STEPS | EPSILON | WINDOW SUCCESS | WINDOW AVG REWARD | RESULT");
            for (int i = 0; i < metrics.Count; i++)
            {
                EpisodeMetric metric = metrics[i];
                if (metric.EpisodeNumber == 1 || metric.EpisodeNumber % interval == 0 || metric.EpisodeNumber == requestedEpisodes)
                {
                    int start = System.Math.Max(0, i - windowSize + 1);
                    int count = i - start + 1;
                    double successRate = MetricsCollector.SuccessRate(metrics, start, count);
                    double averageReward = MetricsCollector.AverageReward(metrics, start, count);
                    Console.WriteLine($"{metric.EpisodeNumber,7} | {metric.TotalReward,7:F2} | {metric.Steps,5} | {metric.Epsilon,7:F3} | {successRate,13:P1} | {averageReward,17:F2} | {metric.TerminationReason}");
                }
            }

            EvaluationResult eval = result.FinalEvaluation;
            Console.WriteLine();
            Console.WriteLine("FINAL EVALUATION");
            Console.WriteLine("Result: " + (eval.ReachedGoal ? "SUCCESS" : "FAILURE"));
            Console.WriteLine("Termination: " + eval.TerminationReason);
            Console.WriteLine("Steps: " + eval.StepsTaken);
            Console.WriteLine("Reward: " + eval.TotalReward.ToString("F2"));
            Console.WriteLine("Path positions: " + eval.Path.Count);
            Console.WriteLine();
            Console.WriteLine("MEMORY");
            Console.WriteLine("Learned states: " + result.MemoryAfterTraining.LearnedStateCount);
            Console.WriteLine("Q-table entries: " + result.MemoryAfterTraining.QTableEntryCount);
            Console.WriteLine("Q updates: " + result.MemoryAfterTraining.UpdateCount);
            Console.WriteLine("Campaign levels: " + result.MemoryAfterTraining.CampaignLevelCount);
        }
    }
}
