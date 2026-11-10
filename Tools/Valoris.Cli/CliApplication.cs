using System.Globalization;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;
using Valoris.Core.Training;
using Valoris.Core.Cli.Serialization;
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
                if (command == "train" || command == "evaluate") return Train(options);
                if (command == "benchmark") return Benchmark(options);
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
            Console.WriteLine("VALORIS / LEVEL INSPECTOR");
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
            int episodes = GetInt(args, "episodes", 1000);
            int seed = GetInt(args, "seed", 1);
            int maxSteps = GetInt(args, "max-steps", 300);
            var trainingOptions = new TrainingOptions
            {
                EpisodesPerLevel = episodes,
                MaxStepsPerEpisode = maxSteps,
                EvaluationMaxSteps = GetInt(args, "eval-steps", maxSteps),
                RandomSeed = seed
            };

            var agent = new ValorisAgent();
            agent.BeginCampaign(seed);
            Console.WriteLine("VALORIS / Q-LEARNING");
            Console.WriteLine($"Level {level.LevelNumber} | Seed {level.Seed} | Episodes {episodes}");
            TrainingResult result = agent.TrainLevel(level, trainingOptions);
            ConsoleReporter.PrintTraining(result, episodes);

            string output = GetString(args, "csv");
            if (!string.IsNullOrWhiteSpace(output))
            {
                CsvExporter.Export(result.EpisodeMetrics, output);
                Console.WriteLine("CSV exported: " + output);
            }
            // A game loss is a valid evaluation result, not a CLI execution error.
            return 0;
        }

        private static int Benchmark(Dictionary<string, string> args)
        {
            string directory = GetString(args, "maps") ?? "Data/Maps/Fixtures";
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
            string[] files = Directory.GetFiles(directory, "*.json").OrderBy(x => x).ToArray();
            if (files.Length == 0) throw new InvalidOperationException("No JSON maps found in " + directory);

            int episodes = GetInt(args, "episodes", 1000);
            int seed = GetInt(args, "seed", 1);
            var options = new TrainingOptions { EpisodesPerLevel = episodes, RandomSeed = seed };
            var persistentAgent = new ValorisAgent();
            persistentAgent.BeginCampaign(seed);
            int persistentWins = 0;
            int resetWins = 0;
            Console.WriteLine("MAP,RESET_MEMORY,PERSISTENT_MEMORY,STEPS_RESET,STEPS_PERSISTENT");

            foreach (string file in files)
            {
                LevelDefinition level = LevelJsonReader.Load(file);
                var resetAgent = new ValorisAgent();
                resetAgent.BeginCampaign(seed);
                TrainingResult resetResult = resetAgent.TrainLevel(level, options);
                TrainingResult persistentResult = persistentAgent.TrainLevel(level, options);
                if (resetResult.FinalEvaluation.ReachedGoal) resetWins++;
                if (persistentResult.FinalEvaluation.ReachedGoal) persistentWins++;
                Console.WriteLine($"{Path.GetFileName(file)},{resetResult.FinalEvaluation.ReachedGoal},{persistentResult.FinalEvaluation.ReachedGoal},{resetResult.FinalEvaluation.StepsTaken},{persistentResult.FinalEvaluation.StepsTaken}");
            }

            Console.WriteLine();
            Console.WriteLine($"Maps: {files.Length}");
            Console.WriteLine($"Reset-memory successes: {resetWins}/{files.Length}");
            Console.WriteLine($"Persistent-memory successes: {persistentWins}/{files.Length}");
            Console.WriteLine("Note: use identical map fixtures and training settings for a fair comparison.");
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

        private static void PrintUsage()
        {
            Console.WriteLine("Valoris CLI");
            Console.WriteLine("  inspect   --map <file.json>");
            Console.WriteLine("  train     --map <file.json> [--episodes 1000] [--max-steps 300] [--seed 1] [--csv results.csv]");
            Console.WriteLine("  evaluate  --map <file.json> [--episodes 1000] [--max-steps 300]");
            Console.WriteLine("  benchmark --maps <directory> [--episodes 1000] [--seed 1]");
        }
    }
}
