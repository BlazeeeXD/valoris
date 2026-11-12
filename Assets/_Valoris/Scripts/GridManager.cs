using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Min(1)] public int width = 24;
    [Min(1)] public int height = 24;

    [Header("Tilemap References")]
    public Tilemap groundTilemap;
    public Tilemap markerTilemap;
    public Tilemap itemTilemap;

    [Header("Base Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase startTile;
    public TileBase goalTile;

    [Header("Item Tiles")]
    public TileBase trapTile;
    public TileBase mudTile;

    public void GenerateGrid(bool[,] walls, Vector2Int start, Vector2Int goal)
    {
        if (groundTilemap == null || markerTilemap == null || itemTilemap == null ||
            floorTile == null || wallTile == null || startTile == null || goalTile == null ||
            trapTile == null || mudTile == null)
        {
            Debug.LogError("GridManager: one or more Tilemap/Tile references are missing.");
            return;
        }
        if (walls == null || walls.GetLength(0) != width || walls.GetLength(1) != height)
        {
            Debug.LogError("GridManager: wall array dimensions do not match the configured grid.");
            return;
        }
        if (!IsInsideGrid(start) || !IsInsideGrid(goal) || start == goal || walls[start.x, start.y] || walls[goal.x, goal.y])
        {
            Debug.LogError("GridManager: start/goal coordinates are invalid.");
            return;
        }

        groundTilemap.ClearAllTiles();
        markerTilemap.ClearAllTiles();
        itemTilemap.ClearAllTiles();

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Vector3Int cell = new Vector3Int(x, -y, 0);
            groundTilemap.SetTile(cell, walls[x, y] ? wallTile : floorTile);
        }
        markerTilemap.SetTile(ToCell(start), startTile);
        markerTilemap.SetTile(ToCell(goal), goalTile);
    }

    public void RenderItem(Vector2Int position, byte cellType)
    {
        if (itemTilemap == null || !IsInsideGrid(position)) return;
        TileBase tile = cellType == 1 ? wallTile : cellType == 2 ? trapTile : cellType == 3 ? mudTile : null;
        if (tile == null) return;
        itemTilemap.SetTile(ToCell(position), tile);
    }

    public void ClearItem(Vector2Int position)
    {
        if (itemTilemap != null && IsInsideGrid(position)) itemTilemap.SetTile(ToCell(position), null);
    }

    public Vector3 GetCellCenterWorld(Vector2Int position)
    {
        if (groundTilemap == null) return Vector3.zero;
        return groundTilemap.GetCellCenterWorld(ToCell(position));
    }

    public bool IsInsideGrid(Vector2Int position)
        => position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;

    private static Vector3Int ToCell(Vector2Int position) => new Vector3Int(position.x, -position.y, 0);
}
