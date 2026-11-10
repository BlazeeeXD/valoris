using System.Text.Json;
using Valoris.Core.Contracts;

namespace Valoris.Core.Cli.Serialization
{
    public static class LevelJsonWriter
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

        public static void Save(LevelDefinition level, string path)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("An output path is required.", nameof(path));

            var dto = new LevelFileDto
            {
                SchemaVersion = level.SchemaVersion,
                LevelNumber = level.LevelNumber,
                Seed = level.Seed,
                Width = level.Width,
                Height = level.Height,
                Cells = level.CopyCells(),
                Start = new PositionDto { X = level.Start.X, Y = level.Start.Y },
                Goal = new PositionDto { X = level.Goal.X, Y = level.Goal.Y }
            };

            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(path, JsonSerializer.Serialize(dto, Options));
        }

        public sealed class LevelFileDto
        {
            public int SchemaVersion { get; set; }
            public int LevelNumber { get; set; }
            public int Seed { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int[] Cells { get; set; }
            public PositionDto Start { get; set; }
            public PositionDto Goal { get; set; }
        }

        public sealed class PositionDto
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
