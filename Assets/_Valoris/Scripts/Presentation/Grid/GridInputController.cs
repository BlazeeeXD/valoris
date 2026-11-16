using UnityEngine;

/// <summary>Shared mouse-to-grid conversion helper for optional UI input components.</summary>
public class GridInputController : MonoBehaviour
{
    public Camera inputCamera;
    public GridManager gridManager;

    public bool TryGetGridPosition(Vector2 screenPosition, out Vector2Int position)
    {
        position = Vector2Int.zero;
        if (gridManager == null || gridManager.groundTilemap == null) return false;
        if (inputCamera == null) inputCamera = Camera.main;
        if (inputCamera == null) return false;
        Vector3 world = inputCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
        Vector3Int cell = gridManager.groundTilemap.WorldToCell(world);
        position = new Vector2Int(cell.x, -cell.y);
        return position.x >= 0 && position.x < gridManager.width && position.y >= 0 && position.y < gridManager.height;
    }
}
