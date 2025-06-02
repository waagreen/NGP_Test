using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, ISaveData
{
    [SerializeField] private PickableItem pickablePrefab;
    [SerializeField] private InventoryDisplayItem displayPrefab;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject descriptionHolder;
    [SerializeField] private List<RectTransform> slots;

    private List<PickableItem> worldPickableItems;
    private List<ItemData> inventoryItemData; // This will be read from and to the json
    private readonly Dictionary<int, InventoryDisplayItem> displayItems = new();

    public void SaveData(ref SaveData data)
    {
        data.inventory = inventoryItemData.ToArray();
    }

    public void LoadData(SaveData data)
    {
        inventoryItemData = new List<ItemData>(data.inventory);
        RefreshInventoryDisplay();
    }

    private void Start()
    {
        var pooledItems = CompositeObjectPooler.Instance.GetNewPool(pickablePrefab, 30);
        worldPickableItems = pooledItems.ConvertAll(item => item as PickableItem);

        foreach (PickableItem item in worldPickableItems)
        {
            if (item != null)
            {
                item.OnPickUp += AddItemToInventory;
            }
        }
    }
    
    private ItemData GetItemDataInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return null;
        return inventoryItemData.Find(data => data.slotIndex == slotIndex);
    }

    private void ShowItemDescription(InventoryItem item)
    {
        if (item == null || descriptionHolder == null || descriptionText == null) return;
        
        descriptionText.text = item.description;
        descriptionHolder.SetActive(true);
    }

    private void HideItemDescription()
    {
        if (descriptionHolder == null || descriptionText == null) return;
        
        descriptionText.text = "";
        descriptionHolder.SetActive(false);
    }

    private void RefreshInventoryDisplay()
    {
        foreach (var itemData in inventoryItemData)
        {
            Debug.Log("Data pre");
            if (itemData == null) continue;
            Debug.Log("Data post " + itemData.id + ", slot " + itemData.slotIndex);  

            if (itemData.slotIndex >= 0 && itemData.slotIndex < slots.Count)
            {
                var itemDefinition = GetItemDefinition(itemData.id);
                Debug.Log("Item definition is " + (itemDefinition == null));
                if (itemDefinition != null)
                {
                    Debug.Log("Creating display");
                    CreateDisplayItem(itemDefinition, itemData);
                }
            }
        }
    }

    private bool IsSlotOccupied(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return true;
        return inventoryItemData.Exists(data => data.slotIndex == slotIndex);
    }

    private InventoryItem GetItemDefinition(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        
        InventoryItem loadedItem = Resources.Load<InventoryItem>($"InventoryDatabase/Items/{id}");
        if (loadedItem == null)
        {
            Debug.LogWarning($"Item definition not found for ID: {id}");
        }
        return loadedItem;
    }

    private void UpdateDisplayForItem(ItemData itemData)
    {
        if (itemData == null) return;
        
        if (displayItems.TryGetValue(itemData.slotIndex, out var displayItem) && displayItem != null)
        {
            displayItem.UpdateAmount(itemData.amount);
        }
    }

    private void AddItemToInventory(InventoryItem itemDefinition)
    {
        if (itemDefinition == null) return;

        if (itemDefinition.isStackable)
        {
            foreach (var existingData in inventoryItemData)
            {
                if (existingData == null) continue;
                
                var existingDefinition = GetItemDefinition(existingData.id);
                if (existingDefinition != null &&
                    existingData.id == itemDefinition.id &&
                    existingData.amount < itemDefinition.maxStackSize)
                {
                    existingData.amount += 1;
                    UpdateDisplayForItem(existingData);
                    return;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (!IsSlotOccupied(i))
            {
                var newItemData = new ItemData
                {
                    id = itemDefinition.id,
                    amount = 1,
                    slotIndex = i
                };
                
                inventoryItemData.Add(newItemData);
                CreateDisplayItem(itemDefinition, newItemData);
                return;
            }
        }

        Debug.LogWarning("Inventory is full!");
    }

    private void CreateDisplayItem(InventoryItem itemDefinition, ItemData itemData)
    {
        if (itemDefinition == null || itemData == null || 
            itemData.slotIndex < 0 || itemData.slotIndex >= slots.Count) 
            return;

        var displayItem = Instantiate(displayPrefab, slots[itemData.slotIndex]);
        if (displayItem == null) return;
        
        displayItem.Setup(itemDefinition, itemData, transform);
        displayItem.OnDragRelease += HandleDragRelease;
        displayItem.OnPointerEnterEvent += ShowItemDescription;
        displayItem.OnPointerExitEvent += HideItemDescription;
        displayItems[itemData.slotIndex] = displayItem;
    }

    private void HandleDragRelease(InventoryDisplayItem displayItem, PointerEventData eventData)
    {
        if (displayItem == null) return;

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        int newSlotIndex = -1;
        foreach (var result in results)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (result.gameObject == slots[i].gameObject)
                {
                    newSlotIndex = i;
                    break;
                }
            }
            if (newSlotIndex != -1) break;
        }

        if (newSlotIndex != -1)
        {
            MoveItem(displayItem.SlotIndex, newSlotIndex);
        }
        else
        {
            displayItem.ResetPosition();
        }
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        
        var itemData = GetItemDataInSlot(slotIndex);
        if (itemData == null) return;

        if (displayItems.TryGetValue(slotIndex, out var displayItem) && displayItem != null)
        {
            displayItem.OnDragRelease -= HandleDragRelease;
            displayItem.OnPointerEnterEvent -= ShowItemDescription;
            displayItem.OnPointerExitEvent -= HideItemDescription;
            Destroy(displayItem.gameObject);
            displayItems.Remove(slotIndex);
        }
        inventoryItemData.Remove(itemData);
    }

    public void MoveItem(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= slots.Count || 
            toSlot < 0 || toSlot >= slots.Count) return;

        var fromItemData = GetItemDataInSlot(fromSlot);
        if (fromItemData == null) return;

        var fromDefinition = GetItemDefinition(fromItemData.id);
        if (fromDefinition == null) return;

        var toItemData = GetItemDataInSlot(toSlot);
        
        if (toItemData == null)
        {
            fromItemData.slotIndex = toSlot;

            if (displayItems.TryGetValue(fromSlot, out var displayItem))
            {
                displayItems.Remove(fromSlot);
                displayItems[toSlot] = displayItem;
                displayItem.transform.SetParent(slots[toSlot]);
                displayItem.transform.localPosition = Vector3.zero;
                displayItem.Setup(fromDefinition, fromItemData, transform);
            }
        }
        else
        {
            var toDefinition = GetItemDefinition(toItemData.id);
            if (toDefinition == null) return;

            if (fromItemData.id == toItemData.id && fromDefinition.isStackable)
            {
                int total = fromItemData.amount + toItemData.amount;
                int maxStack = fromDefinition.maxStackSize;

                if (total <= maxStack)
                {
                    toItemData.amount = total;
                    UpdateDisplayForItem(toItemData);
                    RemoveItem(fromSlot);
                }
                else
                {
                    toItemData.amount = maxStack;
                    fromItemData.amount = total - maxStack;
                    UpdateDisplayForItem(toItemData);
                    UpdateDisplayForItem(fromItemData);
                }
            }
            else
            {
                SwapItems(fromSlot, toSlot);
            }
        }
    }

    private void SwapItems(int slotA, int slotB)
    {
        var itemA = GetItemDataInSlot(slotA);
        var itemB = GetItemDataInSlot(slotB);

        if (itemA != null) itemA.slotIndex = slotB;
        if (itemB != null) itemB.slotIndex = slotA;

        if (displayItems.TryGetValue(slotA, out var displayA) && displayA != null)
        {
            displayA.transform.SetParent(slots[slotB]);
            displayA.transform.localPosition = Vector3.zero;
            if (itemB != null)
            {
                var definitionB = GetItemDefinition(itemB.id);
                if (definitionB != null)
                {
                    displayA.Setup(definitionB, itemB, transform);
                }
            }
        }

        if (displayItems.TryGetValue(slotB, out var displayB) && displayB != null)
        {
            displayB.transform.SetParent(slots[slotA]);
            displayB.transform.localPosition = Vector3.zero;
            if (itemA != null)
            {
                var definitionA = GetItemDefinition(itemA.id);
                if (definitionA != null)
                {
                    displayB.Setup(definitionA, itemA, transform);
                }
            }
        }

        if (displayItems.TryGetValue(slotA, out var tempDisplay))
        {
            displayItems.Remove(slotA);
            if (itemB != null) displayItems[slotB] = tempDisplay;
        }

        if (displayItems.TryGetValue(slotB, out tempDisplay))
        {
            displayItems.Remove(slotB);
            if (itemA != null) displayItems[slotA] = tempDisplay;
        }
    }

    private void OnDestroy()
    {
        if (worldPickableItems != null)
        {
            foreach (PickableItem item in worldPickableItems)
            {
                if (item != null)
                {
                    item.OnPickUp -= AddItemToInventory;
                }
            }
        }

        foreach (var kvp in displayItems)
        {
            if (kvp.Value != null)
            {
                kvp.Value.OnDragRelease -= HandleDragRelease;
                kvp.Value.OnPointerEnterEvent -= ShowItemDescription;
                kvp.Value.OnPointerExitEvent -= HideItemDescription;
            }
        }
    }
}