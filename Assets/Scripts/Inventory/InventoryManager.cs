using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private PickableItem pickablePrefab;
    [SerializeField] private InventoryDisplayItem displayPrefab;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private List<RectTransform> slots;
    [SerializeField] private int maxStackSize = 99;

    List<PickableItem> worldPickableItems;
    private Dictionary<int, InventoryItem> inventoryItems = new();
    private Dictionary<int, InventoryDisplayItem> displayItems = new();

    private void Start()
    {
        var pooledItems = CompositeObjectPooler.Instance.GetNewPool(pickablePrefab, 30);
        worldPickableItems = pooledItems.ConvertAll(item => item as PickableItem);

        foreach (PickableItem item in worldPickableItems)
        {
            item.OnPickUp += AddItemToInventory;
        }

        //Initialize empty slots
        for (int i = 0; i < slots.Count; i++)
        {
            if (!inventoryItems.ContainsKey(i))
            {
                inventoryItems[i] = null;
            }
        }
    }

    private void AddItemToInventory(InventoryItem item)
    {
        // Try to stack items if possibly
        if (item.isStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (inventoryItems[i] != null &&
                    inventoryItems[i].id == item.id &&
                    inventoryItems[i].amount < maxStackSize)
                {
                    int total = inventoryItems[i].amount + item.amount;

                    if (total <= maxStackSize)
                    {
                        inventoryItems[i].amount = total;
                        displayItems[i].UpdateAmount(total);
                        return;
                    }
                    else
                    {
                        inventoryItems[i].amount = maxStackSize;
                        displayItems[i].UpdateAmount(maxStackSize);
                        item.amount = total - maxStackSize;
                    }
                }
            }
        }

        // Find empty slot
        for (int i = 0; i < slots.Count; i++)
        {
            if (inventoryItems[i] == null)
            {
                AddItemToSlot(item, i);
                return;
            }
        }

        Debug.LogWarning("Inventory is full!");
    }

    private void AddItemToSlot(InventoryItem item, int slotIndex)
    {
        inventoryItems[slotIndex] = item;
        
        var displayItem = Instantiate(displayPrefab, slots[slotIndex]);
        displayItem.Setup(item, slotIndex);
        displayItems[slotIndex] = displayItem;
    }

    public void RemoveItem(int slotIndex)
    {
        if (inventoryItems.ContainsKey(slotIndex) && inventoryItems[slotIndex] != null)
        {
            Destroy(displayItems[slotIndex].gameObject);
            inventoryItems[slotIndex] = null;
            displayItems.Remove(slotIndex);
        }
    }

    public void MoveItem(int fromSlot, int toSlot)
    {
        if (inventoryItems[fromSlot] == null) return;

        // If target slot is empty, just move the item
        if (inventoryItems[toSlot] == null)
        {
            inventoryItems[toSlot] = inventoryItems[fromSlot];
            displayItems[toSlot] = displayItems[fromSlot];

            displayItems[toSlot].transform.SetParent(slots[toSlot]);
            displayItems[toSlot].transform.localPosition = Vector3.zero;
            displayItems[toSlot].Setup(inventoryItems[toSlot], toSlot);

            inventoryItems[fromSlot] = null;
            displayItems.Remove(fromSlot);
        }
        // If items are the same and stackable, try to merge
        else if (inventoryItems[fromSlot].id == inventoryItems[toSlot].id &&
                 inventoryItems[fromSlot].isStackable)
        {
            int total = inventoryItems[fromSlot].amount + inventoryItems[toSlot].amount;

            if (total <= maxStackSize)
            {
                inventoryItems[toSlot].amount = total;
                displayItems[toSlot].UpdateAmount(total);
                RemoveItem(fromSlot);
            }
            else
            {
                inventoryItems[toSlot].amount = maxStackSize;
                displayItems[toSlot].UpdateAmount(maxStackSize);
                inventoryItems[fromSlot].amount = total - maxStackSize;
                displayItems[fromSlot].UpdateAmount(total - maxStackSize);
            }
        }
        // If different items or not stackable, swap them
        else
        {
            SwapItems(fromSlot, toSlot);
        }
    }

    private void SwapItems(int slotA, int slotB)
    {
        InventoryItem tempItem = inventoryItems[slotA];
        InventoryDisplayItem tempDisplay = displayItems.ContainsKey(slotA) ? displayItems[slotA] : null;

        inventoryItems[slotA] = inventoryItems[slotB];
        displayItems[slotA] = displayItems.ContainsKey(slotB) ? displayItems[slotB] : null;

        inventoryItems[slotB] = tempItem;
        displayItems[slotB] = tempDisplay;

        // Update display items parent and position
        if (displayItems[slotA] != null)
        {
            displayItems[slotA].transform.SetParent(slots[slotA]);
            displayItems[slotA].transform.localPosition = Vector3.zero;
            displayItems[slotA].Setup(inventoryItems[slotA], slotA);
        }

        if (displayItems[slotB] != null)
        {
            displayItems[slotB].transform.SetParent(slots[slotB]);
            displayItems[slotB].transform.localPosition = Vector3.zero;
            displayItems[slotB].Setup(inventoryItems[slotB], slotB);
        }
    }

    private void OnDestroy()
    {
        if (worldPickableItems == null) return;

        foreach (PickableItem item in worldPickableItems)
        {
            if (item == null) return;
            item.OnPickUp -= AddItemToInventory;
        }
    }
}
