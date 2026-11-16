using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Valoris.Core.Contracts;
using Valoris.Core.Generation;
using Valoris.Core.Grid;
using Valoris.Core.Training;
using Valoris.Core.Cli.Serialization;

namespace Valoris.Core.Cli.Commands
{
    /// <summary>
    /// Paired, multi-seed comparison of a fresh agent per level against one agent
    /// whose contextual memory survives level transitions. Every pair sees the
    /// same immutable map, options and run seed.
    /// </summary>
    public static class BenchmarkCommand
    {
        public static int Run(Dictionary<string, string> args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            int episodes = GetInt(args, "episodes", 3000);
            int maxSteps = GetInt(args, "max-steps", 300);
            int evaluationSteps = GetInt(args, "eval-steps", maxSteps);
            int generatedMapCount = GetInt(args, "generated-maps", 0);
            int seedCount = GetInt(args, "seed-count", 5);

            if (episodes <= 0)
                throw new ArgumentOutOfRangeException("episodes", "Episodes must be positive.");
            if (maxSteps <= 0 || evaluationSteps <= 0)
                throw new ArgumentOutOfRangeException("max-steps", "Step limits must be positive.");
            if (generatedMapCount < 0)
                throw new ArgumentOutOfRangeException("generated-maps", "Generated map count cannot be negative.");
            if (seedCount <= 0 || seedCount > 1000)
                throw new ArgumentOutOfRangeException("seed-count", "Seed count must be between 1 and 1000.");

            int[] seeds = ParseSeeds(args, seedCount);
            MapDifficulty difficulty = ParseDifficulty(GetString(args, "difficulty") ?? "hard");

            List<BenchmarkMap> fixtureMaps = generatedMapCount == 0
                ? LoadFixtureMaps(GetString(args, "maps") ?? Path.Combine("Data", "Maps", "Fixtures"))
                : null;

            var rows = new List<BenchmarkRow>();
            bool collectEpisodeRows = !string.IsNullOrWhiteSpace(GetString(args, "episodes-csv"));
            List<BenchmarkEpisodeRow> episodeRows = collectEpisodeRows ? new List<BenchmarkEpisodeRow>() : null;
            int totalMapRuns = 0;

            Console.WriteLine("ORALIS / MEMORY TRANSFER BENCHMARK");
            Console.WriteLine("Episodes per level: " + episodes);
            Console.WriteLine("Training step cap: " + maxSteps + " | Evaluation step cap: " + evaluationSteps);
            Console.WriteLine("Learning rate: " + Format(GetDouble(args, "alpha", 0.15))
                + " | Discount: " + Format(GetDouble(args, "gamma", 0.95))
                + " | Epsilon: " + Format(GetDouble(args, "epsilon", 1.0))
                + " -> " + Format(GetDouble(args, "min-epsilon", 0.05))
                + " (decay " + Format(GetDouble(args, "epsilon-decay", 0.9995)) + ")");
            Console.WriteLine("Run seeds: " + string.Join(", ", seeds));
            Console.WriteLine(generatedMapCount > 0
                ? "Map source: " + generatedMapCount + " generated " + difficulty + " maps per seed"
                : "Map source: " + fixtureMaps.Count + " fixture(s), ordered by level number");
            Console.WriteLine();
            Console.WriteLine("SEED | LEVEL | MAP | RESET | R-STEPS | R-TRAIN% | PERSISTENT | P-STEPS | P-TRAIN% | CONTEXT STATES");

            foreach (int runSeed in seeds)
            {
                IReadOnlyList<BenchmarkMap> maps = generatedMapCount > 0
                    ? GenerateMaps(generatedMapCount, runSeed, difficulty)
                    : fixtureMaps;

                // One persistent agent represents exactly one campaign/run seed.
                var persistentAgent = new ValorisAgent();
                persistentAgent.BeginCampaign(runSeed);

                foreach (BenchmarkMap map in maps)
                {
                    TrainingOptions options = CreateTrainingOptions(
                        args, runSeed, episodes, maxSteps, evaluationSteps);

                    // Reset arm: fresh contextual and spatial memory for this map.
                    var resetAgent = new ValorisAgent();
                    resetAgent.BeginCampaign(runSeed);
                    TrainingResult resetResult = resetAgent.TrainLevel(map.Level, options);

                    // Persistent arm: TrainLevel clears spatial memory but retains context.
                    int contextBefore = persistentAgent.GetMemorySummary().ContextLearnedStateCount;
                    TrainingResult persistentResult = persistentAgent.TrainLevel(map.Level, options);

                    TrainingSummary resetTraining = SummarizeTraining(resetResult.EpisodeMetrics);
                    TrainingSummary persistentTraining = SummarizeTraining(persistentResult.EpisodeMetrics);

                    var row = new BenchmarkRow
                    {
                        RunSeed = runSeed,
                        MapName = map.Name,
                        MapLevelNumber = map.Level.LevelNumber,
                        MapSeed = map.Level.Seed,
                        EpisodesPerLevel = episodes,
                        MaxStepsPerEpisode = maxSteps,
                        EvaluationMaxSteps = evaluationSteps,
                        LearningRate = options.LearningRate,
                        DiscountFactor = options.DiscountFactor,
                        InitialEpsilon = options.InitialEpsilon,
                        MinimumEpsilon = options.MinimumEpsilon,
                        EpsilonDecay = options.EpsilonDecay,
                        ResetSuccess = resetResult.FinalEvaluation.ReachedGoal,
                        ResetSteps = resetResult.FinalEvaluation.StepsTaken,
                        ResetReward = resetResult.FinalEvaluation.TotalReward,
                        ResetTermination = resetResult.FinalEvaluation.TerminationReason.ToString(),
                        ResetTrainingSuccessRate = resetTraining.SuccessRate,
                        ResetTrainingAverageReward = resetTraining.AverageReward,
                        PersistentSuccess = persistentResult.FinalEvaluation.ReachedGoal,
                        PersistentSteps = persistentResult.FinalEvaluation.StepsTaken,
                        PersistentReward = persistentResult.FinalEvaluation.TotalReward,
                        PersistentTermination = persistentResult.FinalEvaluation.TerminationReason.ToString(),
                        PersistentTrainingSuccessRate = persistentTraining.SuccessRate,
                        PersistentTrainingAverageReward = persistentTraining.AverageReward,
                        ContextStatesBefore = contextBefore,
                        ContextStatesAfter = persistentResult.MemoryAfterTraining.ContextLearnedStateCount
                    };

                    rows.Add(row);
                    AddEpisodeRows(episodeRows, runSeed, map, "reset", resetResult.EpisodeMetrics);
                    AddEpisodeRows(episodeRows, runSeed, map, "persistent", persistentResult.EpisodeMetrics);
                    totalMapRuns++;

                    Console.WriteLine(
                        $"{row.RunSeed,4} | {row.MapLevelNumber,5} | {row.MapName,-18} | {FormatResult(row.ResetSuccess),-6} | " +
                        $"{row.ResetSteps,7} | {row.ResetTrainingSuccessRate,7:F1}% | {FormatResult(row.PersistentSuccess),-10} | " +
                        $"{row.PersistentSteps,7} | {row.PersistentTrainingSuccessRate,7:F1}% | {row.ContextStatesAfter,14}");
                }
            }

            PrintSummary("RESET MEMORY", rows, persistent: false);
            PrintSummary("PERSISTENT CONTEXT MEMORY", rows, persistent: true);

            int resetWins = rows.Count(row => row.ResetSuccess);
            int persistentWins = rows.Count(row => row.PersistentSuccess);
            double successDelta = rows.Count == 0 ? 0.0 : (persistentWins - resetWins) * 100.0 / rows.Count;
            Console.WriteLine();
            Console.WriteLine("PERSISTENT - RESET SUCCESS-RATE DELTA: " + successDelta.ToString("+0.0;-0.0;0.0", CultureInfo.InvariantCulture) + " percentage points");
            Console.WriteLine("Paired rows use the same map, run seed and training options. A positive delta is evidence for this tested set only.");

            string csvPath = GetString(args, "csv");
            if (!string.IsNullOrWhiteSpace(csvPath))
            {
                ExportSummaryCsv(rows, csvPath);
                Console.WriteLine("Benchmark summary CSV exported: " + csvPath);
            }

            string episodeCsvPath = GetString(args, "episodes-csv");
            if (episodeRows != null && !string.IsNullOrWhiteSpace(episodeCsvPath))
            {
                ExportEpisodeCsv(episodeRows, episodeCsvPath);
                Console.WriteLine("Benchmark episode CSV exported: " + episodeCsvPath);
            }

            Console.WriteLine("Total paired map runs: " + totalMapRuns);
            return 0;
        }

        private static List<BenchmarkMap> LoadFixtureMaps(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(directory);

            string[] files = Directory.GetFiles(directory, "*.json");
            var validator = new LevelValidator();
            var maps = new List<BenchmarkMap>(files.Length);

            foreach (string file in files)
            {
                LevelDefinition level = LevelJsonReader.Load(file);
                LevelValidationResult validation = validator.Validate(level);
                if (!validation.IsValid || !validation.HasSafePath)
                {
                    throw new InvalidDataException(
                        "Invalid fixture '" + file + "': " + string.Join("; ", validation.Messages));
                }

                maps.Add(new BenchmarkMap
                {
                    Name = Path.GetFileName(file),
                    Level = level
                });
            }

            if (maps.Count == 0)
                throw new InvalidOperationException("No JSON maps found in " + directory);

            // Campaign memory transfer must follow the actual level progression,
            // not lexicographic file names (easy_01, hard_01, medium_01).
            return maps
                .OrderBy(map => map.Level.LevelNumber)
                .ThenBy(map => map.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static IReadOnlyList<BenchmarkMap> GenerateMaps(
            int count,
            int runSeed,
            MapDifficulty difficulty)
        {
            var generator = new ProceduralMapGenerator();
            var maps = new List<BenchmarkMap>(count);

            for (int i = 0; i < count; i++)
            {
                int mapSeed = CombineSeed(runSeed, i + 1);
                MapGenerationResult result = generator.Generate(new MapGenerationOptions
                {
                    Seed = mapSeed,
                    LevelNumber = i + 1,
                    Difficulty = difficulty
                });

                maps.Add(new BenchmarkMap
                {
                    Name = "generated_" + (i + 1).ToString("D2", CultureInfo.InvariantCulture),
                    Level = result.Level
                });
            }

            return maps;
        }

        private static TrainingOptions CreateTrainingOptions(
            Dictionary<string, string> args,
            int seed,
            int episodes,
            int maxSteps,
            int evaluationSteps)
        {
            var options = new TrainingOptions
            {
                EpisodesPerLevel = episodes,
                MaxStepsPerEpisode = maxSteps,
                EvaluationMaxSteps = evaluationSteps,
                RandomSeed = seed,
                LearningRate = GetDouble(args, "alpha", 0.15),
                DiscountFactor = GetDouble(args, "gamma", 0.95),
                InitialEpsilon = GetDouble(args, "epsilon", 1.0),
                MinimumEpsilon = GetDouble(args, "min-epsilon", 0.05),
                EpsilonDecay = GetDouble(args, "epsilon-decay", 0.9995)
            };
            options.Validate();
            return options;
        }

        private static int[] ParseSeeds(Dictionary<string, string> args, int seedCount)
        {
            string explicitSeeds = GetString(args, "seeds");
            if (!string.IsNullOrWhiteSpace(explicitSeeds))
            {
                int[] parsed = explicitSeeds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => int.Parse(value.Trim(), CultureInfo.InvariantCulture))
                    .ToArray();
                if (parsed.Length == 0)
                    throw new ArgumentException("--seeds must contain at least one integer seed.");
                if (parsed.Distinct().Count() != parsed.Length)
                    throw new ArgumentException("--seeds must not contain duplicate values.");
                return parsed;
            }

            int first = GetInt(args, "seed", 1);
            var seeds = new int[seedCount];
            for (int i = 0; i < seedCount; i++)
                seeds[i] = unchecked(first + i);
            return seeds;
        }

        private static MapDifficulty ParseDifficulty(string value)
        {
            if (!Enum.TryParse(value, true, out MapDifficulty difficulty) ||
                !Enum.IsDefined(typeof(MapDifficulty), difficulty))
            {
                throw new ArgumentException("Unknown difficulty '" + value + "'. Use easy, medium or hard.");
            }
            return difficulty;
        }

        private static TrainingSummary SummarizeTraining(IReadOnlyList<EpisodeMetric> metrics)
        {
            if (metrics == null || metrics.Count == 0)
                return new TrainingSummary();

            int successes = metrics.Count(metric => metric.ReachedGoal);
            return new TrainingSummary
            {
                SuccessRate = successes * 100.0 / metrics.Count,
                AverageReward = metrics.Average(metric => metric.TotalReward)
            };
        }

        private static void AddEpisodeRows(
            List<BenchmarkEpisodeRow> destination,
            int runSeed,
            BenchmarkMap map,
            string arm,
            IReadOnlyList<EpisodeMetric> metrics)
        {
            if (destination == null)
                return;

            foreach (EpisodeMetric metric in metrics)
            {
                destination.Add(new BenchmarkEpisodeRow
                {
                    RunSeed = runSeed,
                    MapName = map.Name,
                    MapLevelNumber = map.Level.LevelNumber,
                    MapSeed = map.Level.Seed,
                    Arm = arm,
                    Episode = metric.EpisodeNumber,
                    TotalReward = metric.TotalReward,
                    Steps = metric.Steps,
                    ReachedGoal = metric.ReachedGoal,
                    Epsilon = metric.Epsilon,
                    TerminationReason = metric.TerminationReason.ToString()
                });
            }
        }

        private static void PrintSummary(string title, IReadOnlyList<BenchmarkRow> rows, bool persistent)
        {
            int successes = rows.Count(row => persistent ? row.PersistentSuccess : row.ResetSuccess);
            double averageSteps = rows.Count == 0 ? 0.0 : rows.Average(row => persistent ? row.PersistentSteps : row.ResetSteps);
            double averageReward = rows.Count == 0 ? 0.0 : rows.Average(row => persistent ? row.PersistentReward : row.ResetReward);
            double rate = rows.Count == 0 ? 0.0 : successes * 100.0 / rows.Count;
            double trainingRate = rows.Count == 0 ? 0.0 : rows.Average(row => persistent ? row.PersistentTrainingSuccessRate : row.ResetTrainingSuccessRate);
            double trainingReward = rows.Count == 0 ? 0.0 : rows.Average(row => persistent ? row.PersistentTrainingAverageReward : row.ResetTrainingAverageReward);

            Console.WriteLine();
            Console.WriteLine(title);
            Console.WriteLine("Final evaluation successes: " + successes + "/" + rows.Count + " (" + Format(rate) + "%)");
            Console.WriteLine("Average final-evaluation steps: " + Format(averageSteps));
            Console.WriteLine("Average final-evaluation reward: " + Format(averageReward));
            Console.WriteLine("Average training episode success: " + Format(trainingRate) + "%");
            Console.WriteLine("Average training episode reward: " + Format(trainingReward));
        }

        private static void ExportSummaryCsv(IReadOnlyList<BenchmarkRow> rows, string path)
        {
            using (var writer = CreateWriter(path))
            {
                writer.WriteLine("runSeed,map,mapLevelNumber,mapSeed,episodesPerLevel,maxStepsPerEpisode,evaluationMaxSteps,learningRate,discountFactor,initialEpsilon,minimumEpsilon,epsilonDecay,resetSuccess,resetTermination,resetEvaluationSteps,resetEvaluationReward,resetTrainingSuccessRate,resetTrainingAverageReward,persistentSuccess,persistentTermination,persistentEvaluationSteps,persistentEvaluationReward,persistentTrainingSuccessRate,persistentTrainingAverageReward,contextStatesBefore,contextStatesAfter");
                foreach (BenchmarkRow row in rows)
                {
                    writer.WriteLine(string.Join(",",
                        row.RunSeed.ToString(CultureInfo.InvariantCulture),
                        EscapeCsv(row.MapName),
                        row.MapLevelNumber.ToString(CultureInfo.InvariantCulture),
                        row.MapSeed.ToString(CultureInfo.InvariantCulture),
                        row.EpisodesPerLevel.ToString(CultureInfo.InvariantCulture),
                        row.MaxStepsPerEpisode.ToString(CultureInfo.InvariantCulture),
                        row.EvaluationMaxSteps.ToString(CultureInfo.InvariantCulture),
                        Format(row.LearningRate),
                        Format(row.DiscountFactor),
                        Format(row.InitialEpsilon),
                        Format(row.MinimumEpsilon),
                        Format(row.EpsilonDecay),
                        (row.ResetSuccess ? "true" : "false"),
                        EscapeCsv(row.ResetTermination),
                        row.ResetSteps.ToString(CultureInfo.InvariantCulture),
                        Format(row.ResetReward),
                        Format(row.ResetTrainingSuccessRate),
                        Format(row.ResetTrainingAverageReward),
                        (row.PersistentSuccess ? "true" : "false"),
                        EscapeCsv(row.PersistentTermination),
                        row.PersistentSteps.ToString(CultureInfo.InvariantCulture),
                        Format(row.PersistentReward),
                        Format(row.PersistentTrainingSuccessRate),
                        Format(row.PersistentTrainingAverageReward),
                        row.ContextStatesBefore.ToString(CultureInfo.InvariantCulture),
                        row.ContextStatesAfter.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        private static void ExportEpisodeCsv(IReadOnlyList<BenchmarkEpisodeRow> rows, string path)
        {
            using (var writer = CreateWriter(path))
            {
                writer.WriteLine("runSeed,map,mapLevelNumber,mapSeed,arm,episode,totalReward,steps,reachedGoal,epsilon,terminationReason");
                foreach (BenchmarkEpisodeRow row in rows)
                {
                    writer.WriteLine(string.Join(",",
                        row.RunSeed.ToString(CultureInfo.InvariantCulture),
                        EscapeCsv(row.MapName),
                        row.MapLevelNumber.ToString(CultureInfo.InvariantCulture),
                        row.MapSeed.ToString(CultureInfo.InvariantCulture),
                        EscapeCsv(row.Arm),
                        row.Episode.ToString(CultureInfo.InvariantCulture),
                        Format(row.TotalReward),
                        row.Steps.ToString(CultureInfo.InvariantCulture),
                        (row.ReachedGoal ? "true" : "false"),
                        Format(row.Epsilon),
                        EscapeCsv(row.TerminationReason)));
                }
            }
        }

        private static StreamWriter CreateWriter(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A CSV output path is required.", nameof(path));

            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            return new StreamWriter(fullPath, false, new UTF8Encoding(false));
        }

        private static string EscapeCsv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static string Format(double value) => value.ToString("0.########", CultureInfo.InvariantCulture);
        private static string FormatResult(bool success) => success ? "SUCCESS" : "FAIL";

        private static int CombineSeed(int seed, int mapIndex)
        {
            unchecked
            {
                int value = seed;
                value = (value * 397) ^ (mapIndex * 7919);
                return value;
            }
        }

        private static string GetString(Dictionary<string, string> args, string key)
            => args.TryGetValue(key, out string value) ? value : null;

        private static int GetInt(Dictionary<string, string> args, string key, int fallback)
            => args.TryGetValue(key, out string value)
                ? int.Parse(value, CultureInfo.InvariantCulture)
                : fallback;

        private static double GetDouble(Dictionary<string, string> args, string key, double fallback)
            => args.TryGetValue(key, out string value)
                ? double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture)
                : fallback;

        private sealed class BenchmarkMap
        {
            public string Name { get; set; }
            public LevelDefinition Level { get; set; }
        }

        private sealed class TrainingSummary
        {
            public double SuccessRate { get; set; }
            public double AverageReward { get; set; }
        }

        private sealed class BenchmarkRow
        {
            public int RunSeed { get; set; }
            public string MapName { get; set; }
            public int MapLevelNumber { get; set; }
            public int MapSeed { get; set; }
            public int EpisodesPerLevel { get; set; }
            public int MaxStepsPerEpisode { get; set; }
            public int EvaluationMaxSteps { get; set; }
            public double LearningRate { get; set; }
            public double DiscountFactor { get; set; }
            public double InitialEpsilon { get; set; }
            public double MinimumEpsilon { get; set; }
            public double EpsilonDecay { get; set; }
            public bool ResetSuccess { get; set; }
            public int ResetSteps { get; set; }
            public double ResetReward { get; set; }
            public string ResetTermination { get; set; }
            public double ResetTrainingSuccessRate { get; set; }
            public double ResetTrainingAverageReward { get; set; }
            public bool PersistentSuccess { get; set; }
            public int PersistentSteps { get; set; }
            public double PersistentReward { get; set; }
            public string PersistentTermination { get; set; }
            public double PersistentTrainingSuccessRate { get; set; }
            public double PersistentTrainingAverageReward { get; set; }
            public int ContextStatesBefore { get; set; }
            public int ContextStatesAfter { get; set; }
        }

        private sealed class BenchmarkEpisodeRow
        {
            public int RunSeed { get; set; }
            public string MapName { get; set; }
            public int MapLevelNumber { get; set; }
            public int MapSeed { get; set; }
            public string Arm { get; set; }
            public int Episode { get; set; }
            public double TotalReward { get; set; }
            public int Steps { get; set; }
            public bool ReachedGoal { get; set; }
            public double Epsilon { get; set; }
            public string TerminationReason { get; set; }
        }
    }
}
