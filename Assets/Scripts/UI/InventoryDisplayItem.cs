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
    private Vector3 originalPosition;
    private Camera cam;

    public event System.Action<InventoryDisplayItem, PointerEventData> OnDragRelease;
    public event System.Action<InventoryItem> OnPointerEnterEvent;
    public event System.Action OnPointerExitEvent;

    public InventoryItem Item => item;
    public int SlotIndex => slotIndex;

    private void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("UiCamera").GetComponent<Camera>();
    }

    public void UpdateAmount(int newAmount)
    {
        amountText.SetText(newAmount.ToString());
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
        originalPosition = transform.localPosition;
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
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        OnDragRelease?.Invoke(this, eventData);
    }

    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
        // transform.position = originalPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
}
