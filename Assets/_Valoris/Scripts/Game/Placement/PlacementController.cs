using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlacementController : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public ProceduralLevelGenerator levelGenerator;
    public GridManager gridManager;
    public Camera mainCamera;

    [Header("Placement Settings")]
    public bool placementEnabled = true;

    private CampaignState placementState = new CampaignState();
    private GameFlowController _flow;

    private void Awake()
    {
        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>();
        if (levelGenerator == null) levelGenerator = FindObjectOfType<ProceduralLevelGenerator>();
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (mainCamera == null) mainCamera = Camera.main;
        _flow = GetComponent<GameFlowController>();
        if (_flow == null) _flow = gameObject.AddComponent<GameFlowController>();
    }

    private void Update()
    {
        if (!placementEnabled || placementState.IsBoardLocked) return;
        HandleItemSelection();
        HandlePlacement();
        HandleRemoval();
    }

    private void HandleItemSelection()
    {
        if (Keyboard.current == null || inventoryManager == null || inventoryManager.items == null) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectItemByCellType(1); // Wall
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectItemByCellType(2); // Trap
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectItemByCellType(3); // Mud
    }

    private void SelectItemByCellType(byte cellType)
    {
        if (inventoryManager != null && inventoryManager.SelectItemByCellType(cellType))
            NotifyInventoryChanged();
    }

    private void HandlePlacement()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (!TryGetGridPosition(out Vector2Int position) || levelGenerator == null || levelGenerator.CurrentLevel == null || inventoryManager == null) return;
        if (inventoryManager.TryPlaceSelectedItem(levelGenerator.CurrentLevel, position))
        {
            if (gridManager != null)
                gridManager.RenderItem(position, levelGenerator.CurrentLevel.GetCell(position.x, position.y));
            NotifyInventoryChanged();
        }
    }

    private void HandleRemoval()
    {
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (!TryGetGridPosition(out Vector2Int position) || levelGenerator == null || levelGenerator.CurrentLevel == null || inventoryManager == null) return;
        if (inventoryManager.TryRemoveItem(levelGenerator.CurrentLevel, position))
        {
            if (gridManager != null) gridManager.ClearItem(position);
            NotifyInventoryChanged();
        }
    }

    private bool TryGetGridPosition(out Vector2Int position)
    {
        position = Vector2Int.zero;
        if (mainCamera == null || gridManager == null || gridManager.groundTilemap == null || Mouse.current == null) return false;
        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 0f));
        Vector3Int cell = gridManager.groundTilemap.WorldToCell(world);
        position = new Vector2Int(cell.x, -cell.y);
        return position.x >= 0 && position.x < gridManager.width && position.y >= 0 && position.y < gridManager.height;
    }

    /// <summary>Called by the scene button. Validation happens before the board is frozen.</summary>
    public void LockBoard()
    {
        if (_flow == null) _flow = GetComponent<GameFlowController>() ?? gameObject.AddComponent<GameFlowController>();
        _flow.TryLockAndRun();
    }

    public bool TrySetLocked(bool locked)
    {
        if (locked)
        {
            if (!placementState.LockBoard()) return false;
            placementEnabled = false;
            return true;
        }
        placementState.ResetBoard();
        placementEnabled = true;
        return true;
    }

    public void ResetForNextLevel()
    {
        TrySetLocked(false);
    }

    private void NotifyInventoryChanged()
    {
        if (_flow != null) _flow.NotifyInventoryChanged();
    }
}
