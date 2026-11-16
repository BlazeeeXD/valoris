using UnityEngine;
using UnityEngine.Tilemaps;

public class CellHighlight : MonoBehaviour
{
    public Tilemap overlayTilemap;
    public TileBase highlightTile;

    public void Show(Vector2Int position)
    {
        if (overlayTilemap == null || highlightTile == null) return;
        overlayTilemap.SetTile(new Vector3Int(position.x, -position.y, 0), highlightTile);
    }

    public void Clear()
    {
        if (overlayTilemap != null) overlayTilemap.ClearAllTiles();
    }
}
