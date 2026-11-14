
using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementController : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public ProceduralLevelGenerator levelGenerator;
    public GridManager gridManager;
    public Camera mainCamera;
    public InventoryUIController inventoryUIController;

    [Header("Placement Settings")]
    public bool placementEnabled = true;

    private CampaignState campaignState = new CampaignState();

    // Independent copy of the final locked level.
    public GeneratedLevelData LockedLevelSnapshot { get; private set; }

    private void Update()
    {
        if (!placementEnabled || campaignState.IsBoardLocked)
            return;

        HandleItemSelection();
        HandlePlacement();
        HandleRemoval();
    }

    private void HandleItemSelection()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectItem(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectItem(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectItem(2);
    }

    private void SelectItem(int index)
    {
        if (inventoryManager == null ||
            inventoryManager.items == null ||
            index >= inventoryManager.items.Length)
            return;

        inventoryManager.SelectItem(inventoryManager.items[index]);
    }

    private void HandlePlacement()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (!TryGetGridPosition(out Vector2Int position))
            return;

        if (levelGenerator == null ||
            levelGenerator.CurrentLevel == null ||
            inventoryManager == null)
            return;

        bool placed = inventoryManager.TryPlaceSelectedItem(
            levelGenerator.CurrentLevel,
            position
        );

        if (placed)
        {
            byte cellType = levelGenerator.CurrentLevel.GetCell(
                position.x, position.y
            );

            gridManager.RenderItem(position, cellType);

             if (inventoryUIController != null)
            inventoryUIController.RefreshInventory();
        }
    }

    private void HandleRemoval()
    {
        if (Mouse.current == null ||
            !Mouse.current.rightButton.wasPressedThisFrame)
            return;

        if (!TryGetGridPosition(out Vector2Int position))
            return;

        if (levelGenerator == null ||
            levelGenerator.CurrentLevel == null ||
            inventoryManager == null ||
            gridManager == null)
            return;

        bool removed = inventoryManager.TryRemoveItem(
            levelGenerator.CurrentLevel,
            position
        );

        if (removed)
        {
            gridManager.ClearItem(position);
            if (inventoryUIController != null)
            inventoryUIController.RefreshInventory();
            Debug.Log($"Removed player-placed item at {position}");
        }
    }

    private bool TryGetGridPosition(out Vector2Int position)
    {
        position = Vector2Int.zero;

        if (mainCamera == null || gridManager == null ||
            gridManager.groundTilemap == null ||
            Mouse.current == null)
            return false;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, 0)
        );

        Vector3Int cell = gridManager.groundTilemap.WorldToCell(
            worldPosition
        );

        position = new Vector2Int(cell.x, -cell.y);

        return position.x >= 0 && position.x < gridManager.width &&
               position.y >= 0 && position.y < gridManager.height;
    }

    public void LockBoard()
    {
        if (campaignState.IsBoardLocked)
        {
            Debug.LogWarning("Board is already locked.");
            return;
        }

        if (levelGenerator == null ||
            levelGenerator.CurrentLevel == null)
        {
            Debug.LogError("Cannot lock board: Current level is missing!");
            return;
        }

        // Create the snapshot before locking the board.
        LockedLevelSnapshot = LevelSnapshotFactory.CreateSnapshot(
            levelGenerator.CurrentLevel
        );

        if (LockedLevelSnapshot == null)
        {
            Debug.LogError("Cannot lock board: Snapshot creation failed!");
            return;
        }

        if (campaignState.LockBoard())
        {
            placementEnabled = false;

            Debug.Log("Board locked successfully.");
            Debug.Log("Final level snapshot created.");
        }
    }

    public void ResetBoard()
    {
        campaignState.ResetBoard();
        placementEnabled = true;
        LockedLevelSnapshot = null;
    }
}
