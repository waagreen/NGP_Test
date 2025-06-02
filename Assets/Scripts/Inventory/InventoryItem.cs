using UnityEngine;

[CreateAssetMenu(fileName = "Inventory Item", menuName = "Inventory System/New Inventory Item")]
public class InventoryItem : ScriptableObject
{
    public Sprite sprite;
    public string id;
    public string description;
    public bool isStackable = true;
    public int maxStackSize = 99;
}
