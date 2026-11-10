using Valoris.Core.Contracts;

namespace Valoris.Core.Cli.Reporting
{
    public static class ConsoleReporter
    {
        public static void PrintTraining(TrainingResult result, int requestedEpisodes)
        {
            int interval = System.Math.Max(1, requestedEpisodes / 10);
            foreach (EpisodeMetric metric in result.EpisodeMetrics)
            {
                if (metric.EpisodeNumber == 1 || metric.EpisodeNumber % interval == 0 || metric.EpisodeNumber == requestedEpisodes)
                    Console.WriteLine($"Episode {metric.EpisodeNumber,5} | Reward {metric.TotalReward,8:F2} | Steps {metric.Steps,4} | Epsilon {metric.Epsilon:F3} | {metric.TerminationReason}");
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
