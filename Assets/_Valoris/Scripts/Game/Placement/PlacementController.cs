
using System.Collections.Generic;
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

    private const byte Wall = 1;
    private const byte Trap = 2;

    private void Awake()
    {
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        if (levelGenerator == null)
            levelGenerator = FindObjectOfType<ProceduralLevelGenerator>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        _flow = GetComponent<GameFlowController>();

        if (_flow == null)
            _flow = gameObject.AddComponent<GameFlowController>();
    }

    private void Update()
    {
        if (!placementEnabled || placementState.IsBoardLocked)
            return;

        HandleItemSelection();
        HandlePlacement();
        HandleRemoval();
    }

    private void HandleItemSelection()
    {
        if (Keyboard.current == null ||
            inventoryManager == null ||
            inventoryManager.items == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectItemByCellType(Wall);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectItemByCellType(Trap);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectItemByCellType(3);
    }

    private void SelectItemByCellType(byte cellType)
    {
        if (inventoryManager != null &&
            inventoryManager.SelectItemByCellType(cellType))
        {
            NotifyInventoryChanged();
        }
    }

    private void HandlePlacement()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (!TryGetGridPosition(out Vector2Int position) ||
            levelGenerator == null ||
            levelGenerator.CurrentLevel == null ||
            inventoryManager == null)
            return;

        GeneratedLevelData level = levelGenerator.CurrentLevel;

        if (!inventoryManager.TryPlaceSelectedItem(level, position))
            return;

        // A placement is valid if the AI has a normal route
        // or can restore one by breaking a single player-placed wall.
        if (!HasSafePath(level) && !HasPathWithOneWallBreak(level))
        {
            inventoryManager.TryRemoveItem(level, position);

            NotifyInventoryChanged();

            Debug.LogWarning(
                "Placement rejected: the AI must retain a route that can be restored with one Wall Breaker use."
            );

            return;
        }

        if (gridManager != null)
        {
            gridManager.RenderItem(
                position,
                level.GetCell(position.x, position.y)
            );
        }

        NotifyInventoryChanged();
    }

    private void HandleRemoval()
    {
        if (Mouse.current == null ||
            !Mouse.current.rightButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (!TryGetGridPosition(out Vector2Int position) ||
            levelGenerator == null ||
            levelGenerator.CurrentLevel == null ||
            inventoryManager == null)
            return;

        if (inventoryManager.TryRemoveItem(
            levelGenerator.CurrentLevel, position))
        {
            if (gridManager != null)
                gridManager.ClearItem(position);

            NotifyInventoryChanged();
        }
    }

    // Checks whether a normal safe route exists.
    private bool HasSafePath(GeneratedLevelData level)
    {
        return HasSafePath(level, new Vector2Int(-1, -1));
    }

    // Checks whether a safe route exists after hypothetically
    // removing one player-placed wall.
    private bool HasPathWithOneWallBreak(GeneratedLevelData level)
    {
        if (level == null ||
            level.cells == null ||
            level.playerPlacedItems == null)
            return false;

        if (level.playerPlacedItems.Length != level.cells.Length)
            return false;

        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                int index = y * level.width + x;

                if (!level.playerPlacedItems[index])
                    continue;

                if (level.GetCell(x, y) != Wall)
                    continue;

                if (HasSafePath(level, new Vector2Int(x, y)))
                    return true;
            }
        }

        return false;
    }

    // Runs BFS while treating the optional ignored wall as Empty.
    private bool HasSafePath(
        GeneratedLevelData level,
        Vector2Int ignoredWall)
    {
        if (level == null || level.cells == null)
            return false;

        Vector2Int start = level.start;
        Vector2Int goal = level.goal;

        if (!IsInsideGrid(level, start) ||
            !IsInsideGrid(level, goal))
            return false;

        if (IsBlocked(level, start, ignoredWall) ||
            IsBlocked(level, goal, ignoredWall))
            return false;

        bool[,] visited = new bool[level.width, level.height];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal)
                return true;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (!IsInsideGrid(level, next))
                    continue;

                if (visited[next.x, next.y])
                    continue;

                if (IsBlocked(level, next, ignoredWall))
                    continue;

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return false;
    }

    private bool IsBlocked(
        GeneratedLevelData level,
        Vector2Int position,
        Vector2Int ignoredWall)
    {
        if (position == ignoredWall)
            return false;

        byte cellType = level.GetCell(position.x, position.y);

        return cellType == Wall || cellType == Trap;
    }

    private bool IsInsideGrid(
        GeneratedLevelData level,
        Vector2Int position)
    {
        return position.x >= 0 &&
               position.x < level.width &&
               position.y >= 0 &&
               position.y < level.height;
    }

    private bool TryGetGridPosition(out Vector2Int position)
    {
        position = Vector2Int.zero;

        if (mainCamera == null ||
            gridManager == null ||
            gridManager.groundTilemap == null ||
            Mouse.current == null)
            return false;

        Vector2 mouse = Mouse.current.position.ReadValue();

        Vector3 world = mainCamera.ScreenToWorldPoint(
            new Vector3(mouse.x, mouse.y, 0f)
        );

        Vector3Int cell = gridManager.groundTilemap.WorldToCell(world);

        position = new Vector2Int(cell.x, -cell.y);

        return position.x >= 0 &&
               position.x < gridManager.width &&
               position.y >= 0 &&
               position.y < gridManager.height;
    }

    public void LockBoard()
    {
        if (_flow == null)
        {
            _flow = GetComponent<GameFlowController>()
                ?? gameObject.AddComponent<GameFlowController>();
        }

        _flow.TryLockAndRun();
    }

    public bool TrySetLocked(bool locked)
    {
        if (locked)
        {
            if (!placementState.LockBoard())
                return false;

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
        if (_flow != null)
            _flow.NotifyInventoryChanged();
    }
}
