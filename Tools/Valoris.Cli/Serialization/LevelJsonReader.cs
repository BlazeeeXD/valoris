using System.Text.Json;
using System.Text.Json.Serialization;
using Valoris.Core.Contracts;

namespace Valoris.Core.Cli.Serialization
{
    public static class LevelJsonReader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static LevelDefinition Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A map path is required.", nameof(path));
            string json = File.ReadAllText(path);
            LevelFileDto dto = JsonSerializer.Deserialize<LevelFileDto>(json, Options);
            if (dto == null) throw new InvalidDataException("Map JSON is empty or invalid.");
            if (dto.Cells == null) throw new InvalidDataException("Map JSON is missing cells.");
            if (dto.Start == null || dto.Goal == null) throw new InvalidDataException("Map JSON must include start and goal coordinates.");
            return new LevelDefinition(dto.SchemaVersion, dto.LevelNumber, dto.Seed,
                dto.Width, dto.Height, dto.Cells,
                new GridPosition(dto.Start.X, dto.Start.Y),
                new GridPosition(dto.Goal.X, dto.Goal.Y));
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
