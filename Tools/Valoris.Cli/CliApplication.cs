using System.Globalization;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Training;
using Valoris.Core.Cli.Serialization;
using Valoris.Core.Cli.Commands;
using Valoris.Core.Cli.Rendering;
using Valoris.Core.Cli.Reporting;

namespace Valoris.Core.Cli
{
    public static class CliApplication
    {
        public static int Run(string[] args)
        {
            if (args == null || args.Length == 0) { PrintUsage(); return 2; }
            string command = args[0].ToLowerInvariant();
            Dictionary<string, string> options = ParseOptions(args.Skip(1).ToArray());
            try
            {
                if (command == "inspect") return Inspect(options);
                if (command == "train") return Train(options);
                if (command == "evaluate")
                {
                    Console.WriteLine("Evaluate runs the configured training budget followed by the final greedy evaluation; saved-policy loading is not part of this MVP.");
                    return Train(options);
                }
                if (command == "benchmark") return BenchmarkCommand.Run(options);
                if (command == "generate") return GenerateCommand.Run(options);
                PrintUsage();
                return 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                return 1;
            }
        }

        private static int Inspect(Dictionary<string, string> args)
        {
            LevelDefinition level = LoadRequired(args);
            LevelValidationResult validation = new LevelValidator().Validate(level);
            Console.WriteLine("ORALIS / LEVEL INSPECTOR");
            Console.WriteLine($"Level: {level.LevelNumber} | Seed: {level.Seed} | Size: {level.Width}x{level.Height}");
            Console.WriteLine($"Start: {level.Start} | Goal: {level.Goal}");
            Console.WriteLine(AsciiGridRenderer.Render(level));
            Console.WriteLine($"Valid: {validation.IsValid} | Safe path: {validation.HasSafePath}");
            foreach (string message in validation.Messages) Console.WriteLine(" - " + message);
            return validation.IsValid ? 0 : 1;
        }

        private static int Train(Dictionary<string, string> args)
        {
            LevelDefinition level = LoadRequired(args);
            int episodes = GetInt(args, "episodes", 3000);
            int seed = GetInt(args, "seed", 1);
            int maxSteps = GetInt(args, "max-steps", 300);
            var trainingOptions = new TrainingOptions
            {
                EpisodesPerLevel = episodes,
                MaxStepsPerEpisode = maxSteps,
                EvaluationMaxSteps = GetInt(args, "eval-steps", maxSteps),
                RandomSeed = seed,
                LearningRate = GetDouble(args, "alpha", 0.15),
                DiscountFactor = GetDouble(args, "gamma", 0.95),
                InitialEpsilon = GetDouble(args, "epsilon", 1.0),
                MinimumEpsilon = GetDouble(args, "min-epsilon", 0.05),
                EpsilonDecay = GetDouble(args, "epsilon-decay", 0.9995)
            };

            var agent = new ValorisAgent();
            agent.BeginCampaign(seed);
            Console.WriteLine("ORALIS / Q-LEARNING");
            Console.WriteLine($"Level {level.LevelNumber} | Map seed {level.Seed} | Run seed {seed} | Episodes {episodes}");
            Console.WriteLine($"Steps: train {trainingOptions.MaxStepsPerEpisode}, evaluate {trainingOptions.EvaluationMaxSteps}");
            Console.WriteLine($"Q-learning: alpha {trainingOptions.LearningRate.ToString(CultureInfo.InvariantCulture)}, " +
                $"gamma {trainingOptions.DiscountFactor.ToString(CultureInfo.InvariantCulture)}, " +
                $"epsilon {trainingOptions.InitialEpsilon.ToString(CultureInfo.InvariantCulture)} -> " +
                $"{trainingOptions.MinimumEpsilon.ToString(CultureInfo.InvariantCulture)} " +
                $"(decay {trainingOptions.EpsilonDecay.ToString(CultureInfo.InvariantCulture)})");
            Console.WriteLine($"Rewards: goal {trainingOptions.Rewards.GoalReward}, trap {trainingOptions.Rewards.TrapPenalty}, " +
                $"wall {trainingOptions.Rewards.WallPenalty}, mud {trainingOptions.Rewards.MudPenalty}, " +
                $"step {trainingOptions.Rewards.StepPenalty}, timeout {trainingOptions.Rewards.TimeoutPenalty}");

            TrainingResult result = agent.TrainLevel(level, trainingOptions);
            ConsoleReporter.PrintTraining(result, episodes);

            string output = GetString(args, "csv");
            if (!string.IsNullOrWhiteSpace(output))
            {
                CsvExporter.Export(result.EpisodeMetrics, output, level, trainingOptions, result.FinalEvaluation);
                Console.WriteLine("CSV exported: " + output);
            }
            // A game loss is a valid evaluation result, not a CLI execution error.
            return 0;
        }

        private static LevelDefinition LoadRequired(Dictionary<string, string> args)
        {
            string path = GetString(args, "map");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Required option: --map <file.json>");
            return LevelJsonReader.Load(path);
        }

        private static Dictionary<string, string> ParseOptions(string[] args)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
                string key = args[i].Substring(2);
                string value = "true";
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)) value = args[++i];
                result[key] = value;
            }
            return result;
        }

        private static string GetString(Dictionary<string, string> args, string key)
            => args.TryGetValue(key, out string value) ? value : null;

        private static int GetInt(Dictionary<string, string> args, string key, int fallback)
            => args.TryGetValue(key, out string value) ? int.Parse(value, CultureInfo.InvariantCulture) : fallback;

        private static double GetDouble(Dictionary<string, string> args, string key, double fallback)
            => args.TryGetValue(key, out string value) ? double.Parse(value, CultureInfo.InvariantCulture) : fallback;

        private static void PrintUsage()
        {
            Console.WriteLine("Oralis CLI");
            Console.WriteLine("  inspect   --map <file.json>");
            Console.WriteLine("  train     --map <file.json> [--episodes 3000] [--max-steps 300] [--eval-steps 300] [--seed 1]");
            Console.WriteLine("            [--alpha 0.15] [--gamma 0.95]");
            Console.WriteLine("            [--epsilon 1.0] [--epsilon-decay 0.9995] [--min-epsilon 0.05] [--csv episodes.csv]");
            Console.WriteLine("  evaluate  --map <file.json> [--episodes 3000] [--max-steps 300]");
            Console.WriteLine("  benchmark --maps <directory> [--episodes 3000] [--seed 1] [--seed-count 5] [--csv summary.csv]");
            Console.WriteLine("            [--episodes-csv episodes.csv] or --generated-maps 10 [--difficulty hard] [--seeds 1,2,3]");
            Console.WriteLine("  generate  [--seed 1] [--level 1] [--difficulty easy|medium|hard] [--out map.json]");
            Console.WriteLine("            [--passages N] [--traps N] [--mud N]");
        }
    }
}
