using System.Text;
using Valoris.Core.Contracts;

namespace Valoris.Core.Cli.Rendering
{
    public static class AsciiGridRenderer
    {
        public static string Render(LevelDefinition level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            var builder = new StringBuilder();
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    GridPosition p = new GridPosition(x, y);
                    char glyph;
                    if (p == level.Start) glyph = 'S';
                    else if (p == level.Goal) glyph = 'G';
                    else
                    {
                        switch (level.GetCell(x, y))
                        {
                            case CellType.Wall: glyph = '#'; break;
                            case CellType.Trap: glyph = '^'; break;
                            case CellType.Mud: glyph = '~'; break;
                            default: glyph = '.'; break;
                        }
                    }
                    builder.Append(glyph);
                }
                if (y < level.Height - 1) builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
