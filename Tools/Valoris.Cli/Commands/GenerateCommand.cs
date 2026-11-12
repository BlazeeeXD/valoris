using System;
using System.Collections.Generic;
using System.Globalization;
using Valoris.Core.Generation;
using Valoris.Core.Cli.Rendering;
using Valoris.Core.Cli.Serialization;

namespace Valoris.Core.Cli.Commands
{
    /// <summary>Generates a deterministic, validated JSON level fixture.</summary>
    public static class GenerateCommand
    {
        public static int Run(Dictionary<string, string> args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            string difficultyText = GetString(args, "difficulty") ?? "easy";
            if (!Enum.TryParse(difficultyText, true, out MapDifficulty difficulty) ||
                !Enum.IsDefined(typeof(MapDifficulty), difficulty))
            {
                throw new ArgumentException("Unknown difficulty '" + difficultyText + "'. Use easy, medium or hard.");
            }

            var options = new MapGenerationOptions
            {
                Seed = GetInt(args, "seed", 1),
                LevelNumber = GetInt(args, "level", 1),
                Difficulty = difficulty
            };

            if (args.ContainsKey("passages"))
                options.AdditionalPassages = GetInt(args, "passages", -1);
            if (args.ContainsKey("traps"))
                options.TrapCount = GetInt(args, "traps", -1);
            if (args.ContainsKey("mud"))
                options.MudCount = GetInt(args, "mud", -1);

            options.Validate();

            MapGenerationResult result = new ProceduralMapGenerator().Generate(options);
            string outputPath = GetString(args, "out");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = System.IO.Path.Combine(
                    "Data", "Maps", "Generated",
                    "level_" + options.LevelNumber.ToString("D2", CultureInfo.InvariantCulture) +
                    "_seed_" + options.Seed.ToString(CultureInfo.InvariantCulture) + ".json");
            }

            LevelJsonWriter.Save(result.Level, outputPath);

            Console.WriteLine("ORALIS / MAP GENERATOR");
            Console.WriteLine("Level: " + result.Level.LevelNumber + " | Seed: " + result.Level.Seed);
            Console.WriteLine("Difficulty: " + result.Difficulty);
            Console.WriteLine("Opened passages: " + result.OpenedPassages);
            Console.WriteLine("Traps: " + result.ActualTrapCount + " | Mud: " + result.ActualMudCount);
            Console.WriteLine("Validation: " + (result.Validation.IsValid && result.Validation.HasSafePath ? "PASS" : "FAIL"));
            Console.WriteLine(AsciiGridRenderer.Render(result.Level));
            Console.WriteLine("JSON exported: " + outputPath);
            return 0;
        }

        private static string GetString(Dictionary<string, string> args, string key)
            => args.TryGetValue(key, out string value) ? value : null;

        private static int GetInt(Dictionary<string, string> args, string key, int fallback)
            => args.TryGetValue(key, out string value)
                ? int.Parse(value, CultureInfo.InvariantCulture)
                : fallback;
    }
}
