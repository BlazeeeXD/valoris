
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Available Items")]
    public ItemDefinition[] items;

    private int[] remainingQuantities;

    public ItemDefinition SelectedItem { get; private set; }

    private void Awake()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogError("InventoryManager: No items assigned!");
            return;
        }

        remainingQuantities = new int[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                Debug.LogError($"InventoryManager: Item at index {i} is missing!");
                continue;
            }

            remainingQuantities[i] = items[i].startingQuantity;
        }

        Debug.Log("Inventory initialized successfully.");
    }

    public bool SelectItem(ItemDefinition item)
    {
        if (item == null || items == null)
            return false;

        foreach (ItemDefinition availableItem in items)
        {
            if (availableItem == item)
            {
                SelectedItem = item;
                Debug.Log($"Selected item: {item.itemName}");
                return true;
            }
        }

        return false;
    }

    public bool TryPlaceSelectedItem(
        GeneratedLevelData level,
        Vector2Int position)
    {
        if (SelectedItem == null || remainingQuantities == null)
            return false;

        int index = GetItemIndex(SelectedItem);

        if (index < 0 || remainingQuantities[index] <= 0)
            return false;

        bool placed = PlacementRules.PlaceItem(
            level, position, SelectedItem.cellType
        );

        if (!placed)
            return false;

        remainingQuantities[index]--;

        Debug.Log(
            $"{SelectedItem.itemName} placed. Remaining: {remainingQuantities[index]}"
        );

        return true;
    }

    public bool TryRemoveItem(
        GeneratedLevelData level,
        Vector2Int position)
    {
        if (level == null || remainingQuantities == null)
            return false;

        byte cellType = level.GetCell(position.x, position.y);

        bool removed = PlacementRules.RemoveItem(level, position);

        if (!removed)
            return false;

        int index = GetItemIndexByCellType(cellType);

        if (index >= 0)
        {
            remainingQuantities[index]++;

            Debug.Log(
                $"{items[index].itemName} removed. Remaining: {remainingQuantities[index]}"
            );
        }

        return true;
    }

    public int GetRemainingQuantity(ItemDefinition item)
    {
        int index = GetItemIndex(item);

        if (index < 0 || remainingQuantities == null)
            return 0;

        return remainingQuantities[index];
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
