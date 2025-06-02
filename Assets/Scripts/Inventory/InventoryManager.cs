using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, ISaveData
{
    [Header("Definition Prefabs")]
    [SerializeField] private PickableItem pickablePrefab;
    [SerializeField] private InventoryDisplayItem displayPrefab;

    [Header("Visuals")]
    [SerializeField] private Transform inventoryParent;
    [SerializeField] private GameObject descriptionHolder;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private List<RectTransform> slots;

    private List<PickableItem> worldPickableItems;
    private List<ItemData> inventoryItemData; // This will be read from and to the json
    private readonly Dictionary<int, InventoryDisplayItem> displayItems = new();

    private Sequence toggleSequence;
    private bool isVisible = true;
    

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
            if (itemData == null) continue;

            if (itemData.slotIndex >= 0 && itemData.slotIndex < slots.Count)
            {
                var itemDefinition = GetItemDefinition(itemData.id);
                if (itemDefinition != null)
                {
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
        
        displayItem.Setup(itemDefinition, itemData, inventoryParent);
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
        bool releasedOverSlot = false;
        
        foreach (var result in results)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (result.gameObject == slots[i].gameObject)
                {
                    newSlotIndex = i;
                    releasedOverSlot = true;
                    break;
                }
            }
            if (newSlotIndex != -1) break;
        }

        if (releasedOverSlot)
        {
            if (newSlotIndex != -1 && newSlotIndex != displayItem.SlotIndex)
            {
                if (IsSlotOccupied(newSlotIndex))
                {
                    SwapItems(displayItem.SlotIndex, newSlotIndex);
                }
                else
                {
                    MoveItem(displayItem.SlotIndex, newSlotIndex);
                }
            }
            else
            {
                displayItem.ResetPosition();
            }
        }
        else
        {
            // Show confirmation dialog or immediately delete
            DeleteItem(displayItem.SlotIndex);
        }
    }
    
    public void DeleteItem(int slotIndex)
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

    private void MoveItem(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= slots.Count || 
            toSlot < 0 || toSlot >= slots.Count) return;

        var fromItemData = GetItemDataInSlot(fromSlot);
        if (fromItemData == null) return;

        // Update the slot index in the data immediately
        fromItemData.slotIndex = toSlot;

        if (displayItems.TryGetValue(fromSlot, out var displayItem))
        {
            // Update the display item's slot index
            displayItem.Setup(GetItemDefinition(fromItemData.id), fromItemData, inventoryParent);
            
            // Move in the dictionary
            displayItems.Remove(fromSlot);
            displayItems[toSlot] = displayItem;
            
            // Update parent and position
            displayItem.transform.SetParent(slots[toSlot]);
            displayItem.transform.localPosition = Vector3.zero;
        }
    }

    private void SwapItems(int slotA, int slotB)
    {
        var itemA = GetItemDataInSlot(slotA);
        var itemB = GetItemDataInSlot(slotB);

        if (itemA == null || itemB == null) return;

        // Update slot indices in the data
        itemA.slotIndex = slotB;
        itemB.slotIndex = slotA;

        // Get the display items
        displayItems.TryGetValue(slotA, out var displayA);
        displayItems.TryGetValue(slotB, out var displayB);

        // Update display A
        if (displayA != null)
        {
            displayA.transform.SetParent(slots[slotB]);
            displayA.transform.localPosition = Vector3.zero;
            displayA.Setup(GetItemDefinition(itemA.id), itemA, inventoryParent);
        }

        // Update display B
        if (displayB != null)
        {
            displayB.transform.SetParent(slots[slotA]);
            displayB.transform.localPosition = Vector3.zero;
            displayB.Setup(GetItemDefinition(itemB.id), itemB, inventoryParent);
        }

        // Update the dictionary
        if (displayA != null) displayItems[slotB] = displayA;
        if (displayB != null) displayItems[slotA] = displayB;
    }

    public void ToggleSequence()
    {
        float rotation = isVisible ? 90 : 0;

        toggleSequence?.Kill();
        toggleSequence = DOTween.Sequence();

        toggleSequence.Append(inventoryParent.DORotate(Vector3.forward * rotation, 0.4f, RotateMode.Fast).SetEase(Ease.OutBack));
        toggleSequence.Insert(0.1f, inventoryParent.DOPunchScale(Vector3.one * 0.2f, 0.3f, vibrato: 5, elasticity: 0.5f));
        toggleSequence.OnComplete(() => isVisible = !isVisible);

        toggleSequence.Play();
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