using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    private CampaignHUDController _hud;
    private InventoryManager _inventory;

    public void Initialize(CampaignHUDController hud, InventoryManager inventory)
    {
        _hud = hud;
        _inventory = inventory;
        Refresh();
    }

    public void Refresh()
    {
        if (_hud == null || _inventory == null || _inventory.items == null) return;
        var parts = new System.Collections.Generic.List<string>();
        for (int i = 0; i < _inventory.items.Length; i++)
        {
            ItemDefinition item = _inventory.items[i];
            if (item == null) continue;
            string selected = _inventory.SelectedItem == item ? " [SELECTED]" : "";
            string key = item.cellType == 1 ? "1" : item.cellType == 2 ? "2" : item.cellType == 3 ? "3" : "?";
            parts.Add(key + ": " + item.itemName + " x" + _inventory.GetRemainingQuantity(item) + selected);
        }
        _hud.SetInventory(string.Join("   ", parts));
    }
}
