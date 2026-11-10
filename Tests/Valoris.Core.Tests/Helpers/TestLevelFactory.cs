using Valoris.Core.Contracts;

namespace Valoris.Core.Tests.Helpers
{
    public static class TestLevelFactory
    {
        public static LevelDefinition OpenLevel(int levelNumber = 1, int seed = 1001)
        {
            int width = 24, height = 24;
            var cells = new int[width * height];
            for (int x = 0; x < width; x++)
            {
                cells[x] = (int)CellType.Wall;
                cells[(height - 1) * width + x] = (int)CellType.Wall;
            }
            for (int y = 0; y < height; y++)
            {
                cells[y * width] = (int)CellType.Wall;
                cells[y * width + width - 1] = (int)CellType.Wall;
            }
            return new LevelDefinition(1, levelNumber, seed, width, height, cells,
                new GridPosition(1, 1), new GridPosition(22, 22));
        }

        public static LevelDefinition DirectGoalLevel()
        {
            const int width = 24;
            const int height = 24;
            var cells = new int[width * height];
            for (int i = 0; i < cells.Length; i++) cells[i] = (int)CellType.Wall;
            cells[1 * width + 1] = (int)CellType.Empty;
            cells[1 * width + 2] = (int)CellType.Empty;
            return new LevelDefinition(1, 1, 9001, width, height, cells,
                new GridPosition(1, 1), new GridPosition(2, 1));
        }

        public static LevelDefinition LoadHardFixture()
        {
            string path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Fixtures", "hard_01.json");
            using (System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(path)))
            {
                System.Text.Json.JsonElement root = document.RootElement;
                int[] cells = System.Linq.Enumerable.ToArray(
                    System.Linq.Enumerable.Select(root.GetProperty("cells").EnumerateArray(), item => item.GetInt32()));
                System.Text.Json.JsonElement start = root.GetProperty("start");
                System.Text.Json.JsonElement goal = root.GetProperty("goal");
                return new LevelDefinition(
                    root.GetProperty("schemaVersion").GetInt32(),
                    root.GetProperty("levelNumber").GetInt32(),
                    root.GetProperty("seed").GetInt32(),
                    root.GetProperty("width").GetInt32(),
                    root.GetProperty("height").GetInt32(),
                    cells,
                    new GridPosition(start.GetProperty("x").GetInt32(), start.GetProperty("y").GetInt32()),
                    new GridPosition(goal.GetProperty("x").GetInt32(), goal.GetProperty("y").GetInt32()));
            }
        }
    }
}
