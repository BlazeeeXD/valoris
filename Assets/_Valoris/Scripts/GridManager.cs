
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

    public void GenerateGrid(
        bool[,] walls,
        Vector2Int start,
        Vector2Int goal)
    {
        if (groundTilemap == null || markerTilemap == null ||
            itemTilemap == null || floorTile == null ||
            wallTile == null || startTile == null || goalTile == null ||
            trapTile == null || mudTile == null)
        {
            Debug.LogError("GridManager: A reference is missing!");
            return;
        }

        if (walls == null || walls.GetLength(0) != width ||
            walls.GetLength(1) != height)
        {
            Debug.LogError("GridManager: Invalid wall data!");
            return;
        }

        if (!IsInsideGrid(start) || !IsInsideGrid(goal))
        {
            Debug.LogError("GridManager: Start or Goal is outside the grid!");
            return;
        }

        if (walls[start.x, start.y] || walls[goal.x, goal.y])
        {
            Debug.LogError("GridManager: Start or Goal is on a wall!");
            return;
        }

        groundTilemap.ClearAllTiles();
        markerTilemap.ClearAllTiles();
        itemTilemap.ClearAllTiles();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int cell = new Vector3Int(x, -y, 0);

                groundTilemap.SetTile(
                    cell,
                    walls[x, y] ? wallTile : floorTile
                );
            }
        }

        markerTilemap.SetTile(
            new Vector3Int(start.x, -start.y, 0), startTile
        );

        markerTilemap.SetTile(
            new Vector3Int(goal.x, -goal.y, 0), goalTile
        );

        Debug.Log("Grid, Start and Goal generated successfully.");
    }

    public void RenderItem(Vector2Int position, byte cellType)
    {
        if (itemTilemap == null)
            return;

        TileBase tile = GetItemTile(cellType);

        if (tile == null)
        {
            Debug.LogError("GridManager: Item tile is missing!");
            return;
        }

        Vector3Int cell = new Vector3Int(position.x, -position.y, 0);
        itemTilemap.SetTile(cell, tile);
    }

    public void ClearItem(Vector2Int position)
    {
        if (itemTilemap == null)
            return;

        Vector3Int cell = new Vector3Int(position.x, -position.y, 0);
        itemTilemap.SetTile(cell, null);
    }

    private TileBase GetItemTile(byte cellType)
    {
        switch (cellType)
        {
            case 1:
                return wallTile;

            case 2:
                return trapTile;

            case 3:
                return mudTile;

            default:
                return null;
        }
    }

    private bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < width &&
               position.y >= 0 && position.y < height;
    }
}
