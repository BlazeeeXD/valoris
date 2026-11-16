using NUnit.Framework;
using UnityEngine;

public class InventoryLoadoutTests
{
    [Test]
    public void SameLevelSeedProducesSameLimitedQuantities()
    {
        var gameObject = new GameObject("InventoryLoadoutTest");
        var wall = ScriptableObject.CreateInstance<ItemDefinition>();
        var trap = ScriptableObject.CreateInstance<ItemDefinition>();
        var mud = ScriptableObject.CreateInstance<ItemDefinition>();
        try
        {
            wall.cellType = 1; wall.startingQuantity = 5; wall.itemName = "Wall";
            trap.cellType = 2; trap.startingQuantity = 5; trap.itemName = "Trap";
            mud.cellType = 3; mud.startingQuantity = 5; mud.itemName = "Mud";

            var inventory = gameObject.AddComponent<InventoryManager>();
            inventory.items = new[] { wall, trap, mud };
            inventory.ResetInventory(2468);
            int[] first = {
                inventory.GetRemainingQuantity(wall),
                inventory.GetRemainingQuantity(trap),
                inventory.GetRemainingQuantity(mud)
            };
            inventory.ResetInventory(2468);
            int[] second = {
                inventory.GetRemainingQuantity(wall),
                inventory.GetRemainingQuantity(trap),
                inventory.GetRemainingQuantity(mud)
            };

            CollectionAssert.AreEqual(first, second);
            foreach (int quantity in first)
                Assert.That(quantity, Is.InRange(1, 5));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(wall);
            Object.DestroyImmediate(trap);
            Object.DestroyImmediate(mud);
        }
    }

    [Test]
    public void KeyboardCellTypesSelectCorrectAssetRegardlessOfInspectorOrder()
    {
        var gameObject = new GameObject("InventorySelectionTest");
        var mud = ScriptableObject.CreateInstance<ItemDefinition>();
        var trap = ScriptableObject.CreateInstance<ItemDefinition>();
        var wall = ScriptableObject.CreateInstance<ItemDefinition>();
        try
        {
            mud.cellType = 3; mud.itemName = "Mud"; mud.startingQuantity = 5;
            trap.cellType = 2; trap.itemName = "Trap"; trap.startingQuantity = 5;
            wall.cellType = 1; wall.itemName = "Wall"; wall.startingQuantity = 5;

            var inventory = gameObject.AddComponent<InventoryManager>();
            inventory.items = new[] { mud, trap, wall }; // Deliberately matches the scene's non-keyboard order.
            inventory.ResetInventory(42);

            Assert.That(inventory.SelectItemByCellType(1), Is.True);
            Assert.That(inventory.SelectedItem, Is.SameAs(wall));
            Assert.That(inventory.SelectItemByCellType(2), Is.True);
            Assert.That(inventory.SelectedItem, Is.SameAs(trap));
            Assert.That(inventory.SelectItemByCellType(3), Is.True);
            Assert.That(inventory.SelectedItem, Is.SameAs(mud));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(mud);
            Object.DestroyImmediate(trap);
            Object.DestroyImmediate(wall);
        }
    }

}
