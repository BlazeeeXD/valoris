using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Convenience renderer over the scene's GridManager tilemap references.</summary>
public class TilemapGridRenderer : MonoBehaviour
{
    public GridManager gridManager;

    public void Render(GeneratedLevelData level)
    {
        if (level == null || gridManager == null)
        {
            Debug.LogError("TilemapGridRenderer: level or GridManager is missing.");
            return;
        }
        var walls = new bool[level.width, level.height];
        for (int y = 0; y < level.height; y++)
        for (int x = 0; x < level.width; x++)
        {
            byte type = level.GetCell(x, y);
            walls[x, y] = type == 1;
        }
        gridManager.GenerateGrid(walls, level.start, level.goal);
        for (int y = 0; y < level.height; y++)
        for (int x = 0; x < level.width; x++)
        {
            byte type = level.GetCell(x, y);
            if (type == 2 || type == 3) gridManager.RenderItem(new Vector2Int(x, y), type);
        }
    }
}
