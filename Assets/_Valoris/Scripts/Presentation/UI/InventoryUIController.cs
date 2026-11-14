
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public Button button;
        public TMP_Text itemNameText;
        public TMP_Text quantityText;
        public Image icon;
    }

    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Inventory Slots")]
    [SerializeField] private InventorySlot[] slots;

    private void Start()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryUIController: InventoryManager is missing!");
            return;
        }

        SetupButtons();
        RefreshInventory();
    }

    private void SetupButtons()
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;

            if (slots[i] == null || slots[i].button == null)
                continue;

            slots[i].button.onClick.RemoveAllListeners();
            slots[i].button.onClick.AddListener(
                () => SelectInventoryItem(index)
            );
        }
    }

    public void SelectInventoryItem(int index)
    {
        if (inventoryManager == null ||
            inventoryManager.items == null ||
            index < 0 ||
            index >= inventoryManager.items.Length)
            return;

        inventoryManager.SelectItem(
            inventoryManager.items[index]
        );

        RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (inventoryManager == null ||
            inventoryManager.items == null ||
            slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            bool hasItem = i < inventoryManager.items.Length &&
                           inventoryManager.items[i] != null;

            if (!hasItem)
                continue;

            ItemDefinition item = inventoryManager.items[i];

            int quantity = inventoryManager.GetRemainingQuantity(item);

            if (slots[i].itemNameText != null)
                slots[i].itemNameText.text = item.itemName;

            if (slots[i].quantityText != null)
                slots[i].quantityText.text = "x" + quantity;

            if (slots[i].icon != null)
                slots[i].icon.sprite = item.icon;

            if (slots[i].button != null)
                slots[i].button.interactable = quantity > 0;
        }
    }
}
