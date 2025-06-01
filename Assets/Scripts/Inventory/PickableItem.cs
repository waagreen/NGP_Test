using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PickableItem : PoolableObject
{
    [SerializeField] private LayerMask detectionMask;
    [SerializeField] private float pickableRadius = 1.5f;
    [SerializeField] private SpriteRenderer sRenderer;

    private CircleCollider2D pickableArea;
    private Sequence pickupSequence;
    private InventoryItem item;
    private Transform target;

    public void Setup(InventoryItem item)
    {
        this.item = item;
        sRenderer.sprite = item.sprite;
    }

    private void Start()
    {
        pickableArea = GetComponent<CircleCollider2D>();
        pickableArea.isTrigger = true;
        pickableArea.radius = pickableRadius;
    }

    private void PickUp()
    {
        pickupSequence?.Kill();
        pickupSequence = DOTween.Sequence();

        pickupSequence.Append(transform.DOScale(Vector3.one, 0.4f));
        pickupSequence.Append(transform.DOScale(Vector3.zero, 0.07f));
        pickupSequence.OnComplete(() => CompositeObjectPooler.Instance.ReturnObject(this));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((detectionMask & (1 << collision.gameObject.layer)) == 0) return;
        PickUp();
    }
}
