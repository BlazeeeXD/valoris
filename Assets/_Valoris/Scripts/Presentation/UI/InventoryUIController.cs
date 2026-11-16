
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    public InventoryManager inventoryManager;
    public InventorySlot[] slots;

    private CampaignHUDController _hud;

    public void Initialize(CampaignHUDController hud, InventoryManager inventory)
    {
        _hud = hud;
        inventoryManager = inventory;

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryUIController: InventoryManager missing.");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            int slotIndex = i;

            if (slots[i] != null && slots[i].button != null)
            {
                slots[i].button.onClick.RemoveListener(
                    () => SelectSlot(slotIndex));

                slots[i].button.onClick.AddListener(
                    () => SelectSlot(slotIndex));
            }
        }

        Refresh();
    }

    private void SelectSlot(int index)
    {
        if (inventoryManager == null ||
            inventoryManager.items == null ||
            index < 0 ||
            index >= inventoryManager.items.Length)
            return;

        ItemDefinition item = inventoryManager.items[index];

        if (item != null && inventoryManager.SelectItem(item))
            Refresh();
    }

    public void Refresh()
    {
        if (inventoryManager == null ||
            inventoryManager.items == null ||
            slots == null)
            return;

        int count = Mathf.Min(slots.Length, inventoryManager.items.Length);

        var parts = new List<string>();

        for (int i = 0; i < count; i++)
        {
            InventorySlot slot = slots[i];
            ItemDefinition item = inventoryManager.items[i];

            if (slot == null || item == null)
                continue;

            int quantity = inventoryManager.GetRemainingQuantity(item);
            bool selected = inventoryManager.SelectedItem == item;

            if (slot.itemNameText != null)
                slot.itemNameText.text = item.itemName;

            if (slot.quantityText != null)
                slot.quantityText.text = "x" + quantity;

            if (slot.button != null)
                slot.button.interactable = quantity > 0;

            string key = item.cellType == 1 ? "1" :
                         item.cellType == 2 ? "2" :
                         item.cellType == 3 ? "3" : "?";

            string selectedText = selected ? " [SELECTED]" : "";

            parts.Add(key + ": " + item.itemName +
                      " x" + quantity + selectedText);
        }

        if (_hud != null)
            _hud.SetInventory(string.Join("   ", parts));
    }
}
