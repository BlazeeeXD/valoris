
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewItem",
    menuName = "Oralis/Item Definition"
)]
public class ItemDefinition : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;
    public Sprite icon;

    [Header("Item Settings")]
    [Range(1, 3)]
    public byte cellType;

    [Min(0)]
    public int startingQuantity = 5;
}
