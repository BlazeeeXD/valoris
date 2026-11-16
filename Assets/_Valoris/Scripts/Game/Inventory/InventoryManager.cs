using System;
using UnityEngine;

/// <summary>Creates a reproducible, limited sabotage loadout for each level.</summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Available Items")]
    [Tooltip("Each item's startingQuantity is the maximum random quantity for a level.")]
    public ItemDefinition[] items;

    private int[] remainingQuantities;
    public ItemDefinition SelectedItem { get; private set; }

    private void Awake()
    {
        // EditMode tests and runtime composition may assign item definitions after AddComponent/Awake.
        // The campaign flow performs the authoritative deterministic reset once the level seed is known.
        if (items != null && items.Length > 0)
            ResetInventory(Environment.TickCount);
    }

    /// <summary>Uses the level seed so the same campaign/level always gets the same loadout.</summary>
    public void ResetInventory(int seed)
    {
        if (items == null || items.Length == 0)
        {
            remainingQuantities = new int[0];
            SelectedItem = null;
            Debug.LogWarning("InventoryManager: no sabotage items are assigned.");
            return;
        }

        var random = new System.Random(seed);
        remainingQuantities = new int[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            int maximum = items[i] == null ? 0 : Mathf.Max(0, items[i].startingQuantity);
            // A non-zero item definition contributes between one and its configured maximum.
            remainingQuantities[i] = maximum == 0 ? 0 : random.Next(1, maximum + 1);
        }
        SelectedItem = null;
    }

    /// <summary>Fallback for editor/manual callers; runtime campaign flow passes a deterministic seed.</summary>
    public void ResetInventory() => ResetInventory(Environment.TickCount);

    public bool SelectItem(ItemDefinition item)
    {
        if (item == null || GetItemIndex(item) < 0) return false;
        SelectedItem = item;
        return true;
    }

    /// <summary>Selects an item by the shared Core CellType value, independent of Inspector array order.</summary>
    public bool SelectItemByCellType(byte cellType)
    {
        if (items == null) return false;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].cellType == cellType)
                return SelectItem(items[i]);
        }
        return false;
    }

    public bool TryPlaceSelectedItem(GeneratedLevelData level, Vector2Int position)
    {
        if (level == null || SelectedItem == null || remainingQuantities == null) return false;
        int index = GetItemIndex(SelectedItem);
        if (index < 0 || remainingQuantities[index] <= 0) return false;
        if (!PlacementRules.PlaceItem(level, position, SelectedItem.cellType)) return false;
        remainingQuantities[index]--;
        return true;
    }

    public bool TryRemoveItem(GeneratedLevelData level, Vector2Int position)
    {
        if (level == null || remainingQuantities == null) return false;
        if (position.x < 0 || position.y < 0 || position.x >= level.width || position.y >= level.height) return false;
        byte cellType = level.GetCell(position.x, position.y);
        if (!PlacementRules.RemoveItem(level, position)) return false;
        int index = GetItemIndexByCellType(cellType);
        if (index >= 0) remainingQuantities[index]++;
        return true;
    }

    public int GetRemainingQuantity(ItemDefinition item)
    {
        int index = GetItemIndex(item);
        return index < 0 || remainingQuantities == null ? 0 : remainingQuantities[index];
    }

    private int GetItemIndex(ItemDefinition item)
    {
        if (items == null || item == null) return -1;
        for (int i = 0; i < items.Length; i++) if (items[i] == item) return i;
        return -1;
    }

    private int GetItemIndexByCellType(byte cellType)
    {
        if (items == null) return -1;
        for (int i = 0; i < items.Length; i++)
            if (items[i] != null && items[i].cellType == cellType) return i;
        return -1;
    }
}
