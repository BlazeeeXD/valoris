
using System;
using UnityEngine;

/// <summary>Creates reproducible, limited sabotage loadouts.</summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Available Items")]
    public ItemDefinition[] items;

    private int[] remainingQuantities;

    public ItemDefinition SelectedItem { get; private set; }

    private const byte Wall = 1;
    private const byte Trap = 2;
    private const byte Mud = 3;

    private void Awake()
    {
        if (items != null && items.Length > 0)
            ResetInventory(Environment.TickCount);
    }

    /// <summary>
    /// First-level tutorial loadout.
    /// Player receives exactly two walls and no other items.
    /// </summary>
    public void ResetFirstRunInventory()
    {
        if (items == null || items.Length == 0)
        {
            remainingQuantities = new int[0];
            SelectedItem = null;
            Debug.LogWarning("InventoryManager: no items assigned.");
            return;
        }

        remainingQuantities = new int[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                continue;

            switch (items[i].cellType)
            {
                case Wall:
                    remainingQuantities[i] = 2;
                    break;

                case Trap:
                case Mud:
                    remainingQuantities[i] = 0;
                    break;

                default:
                    remainingQuantities[i] = 0;
                    break;
            }
        }

        SelectedItem = null;

        Debug.Log("Tutorial inventory initialized: 2 walls, 0 traps, 0 mud.");
    }

    /// <summary>Deterministic randomized inventory for subsequent levels.</summary>
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
            int maximum = items[i] == null
                ? 0
                : Mathf.Max(0, items[i].startingQuantity);

            remainingQuantities[i] =
                maximum == 0 ? 0 : random.Next(1, maximum + 1);
        }

        SelectedItem = null;
    }

    public void ResetInventory()
    {
        ResetInventory(Environment.TickCount);
    }

    public bool SelectItem(ItemDefinition item)
    {
        if (item == null || GetItemIndex(item) < 0)
            return false;

        SelectedItem = item;
        return true;
    }

    public bool SelectItemByCellType(byte cellType)
    {
        if (items == null)
            return false;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].cellType == cellType)
                return SelectItem(items[i]);
        }

        return false;
    }

    public bool TryPlaceSelectedItem(
        GeneratedLevelData level,
        Vector2Int position)
    {
        if (level == null || SelectedItem == null || remainingQuantities == null)
            return false;

        int index = GetItemIndex(SelectedItem);

        if (index < 0 || remainingQuantities[index] <= 0)
            return false;

        if (!PlacementRules.PlaceItem(level, position, SelectedItem.cellType))
            return false;

        remainingQuantities[index]--;
        return true;
    }

    public bool TryRemoveItem(
        GeneratedLevelData level,
        Vector2Int position)
    {
        if (level == null || remainingQuantities == null)
            return false;

        if (position.x < 0 || position.y < 0 ||
            position.x >= level.width || position.y >= level.height)
            return false;

        byte cellType = level.GetCell(position.x, position.y);

        if (!PlacementRules.RemoveItem(level, position))
            return false;

        int index = GetItemIndexByCellType(cellType);

        if (index >= 0)
            remainingQuantities[index]++;

        return true;
    }

    public int GetRemainingQuantity(ItemDefinition item)
    {
        int index = GetItemIndex(item);

        return index < 0 || remainingQuantities == null
            ? 0
            : remainingQuantities[index];
    }

    private int GetItemIndex(ItemDefinition item)
    {
        if (items == null || item == null)
            return -1;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
                return i;
        }

        return -1;
    }

    private int GetItemIndexByCellType(byte cellType)
    {
        if (items == null)
            return -1;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].cellType == cellType)
                return i;
        }

        return -1;
    }
}
