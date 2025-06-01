using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDisplayItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image spriteHolder;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Transform originalParent;
    private InventoryItem item;
    private int slotIndex;
    private bool isDragging;

    public event System.Action<InventoryDisplayItem, PointerEventData> OnDragRelease;

    public void UpdateAmount(int newAmount)
    {
        amountText.SetText(newAmount > 1 ? newAmount.ToString() : "");
    }

    public void Setup(InventoryItem item, int slotIndex)
    {
        this.item = item;
        this.slotIndex = slotIndex;

        spriteHolder.sprite = item.sprite;
        UpdateAmount(item.amount);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        OnDragRelease?.Invoke(this, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}
