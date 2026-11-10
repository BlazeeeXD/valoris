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
    }
}
