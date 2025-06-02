using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDisplayItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image spriteHolder;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Camera cam;
    private Transform originalParent;
    private Transform inventoryTransform;
    private InventoryItem itemDefinition;
    private ItemData itemData;
    private int slotIndex;

    public event System.Action<InventoryDisplayItem, PointerEventData> OnDragRelease;
    public event System.Action<InventoryItem> OnPointerEnterEvent;
    public event System.Action OnPointerExitEvent;

    public int SlotIndex => slotIndex;

    private void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("UiCamera").GetComponent<Camera>();
    }

    public void UpdateAmount(int newAmount)
    {
        amountText.SetText(newAmount.ToString());
    }

    public void Setup(InventoryItem itemDefinition, ItemData itemData, Transform inventoryTransform)
    {
        this.itemDefinition = itemDefinition;
        this.itemData = itemData;
        this.inventoryTransform = inventoryTransform;
        slotIndex = itemData.slotIndex;

        spriteHolder.sprite = itemDefinition.sprite;
        UpdateAmount(itemData.amount);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(inventoryTransform);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform rect = transform as RectTransform;
        // For world space canvases
        RectTransformUtility.ScreenPointToWorldPointInRectangle
        (
            rect,
            eventData.position,
            cam,
            out Vector3 worldPoint
        );
        rect.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        OnDragRelease?.Invoke(this, eventData);
    }

    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke(itemDefinition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
}
