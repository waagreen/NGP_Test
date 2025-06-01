using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Enemy : PoolableObject
{
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private List<InventoryItem> drops;
    [SerializeField] private SpriteRenderer sRenderer;
    [SerializeField] private PickableItem pickablePrefab;

    [Header("Movement setting")]
    [SerializeField, Range(1f, 5f)] private float maxSpeed = 2f;
    [SerializeField, Range(1f, 50f)] private float acceleration = 25f;
    [SerializeField, Range(1f, 3f)] private float chaseSpeedMultiplier = 1f;

    [Header("Perception Setting")]
    [SerializeField] private LayerMask targetMask = -1;
    [SerializeField] private LayerMask obstacleMask = -1;
    [SerializeField] private LayerMask hurtMask = -1;
    [SerializeField] private float perceptionRadius = 4f;

    private Rigidbody2D rb;
    private CircleCollider2D col;

    private Sequence hurtSequence;
    private Transform target;
    private Vector2 currentDirection;
    private Vector2 velocity;
    private float currentMaxSpeed;
    private int currentHealth;
    private int originalLayer = -1;
    private bool isActive = false;

    public System.Action<Enemy> OnDeath;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        if (col == null)
        {
            col = GetComponent<CircleCollider2D>();
            col.radius = perceptionRadius;
            col.isTrigger = true;
        }
        if (originalLayer == -1) originalLayer = gameObject.layer;

        FreezeBehaviour();
    }

    private void FreezeBehaviour()
    {
        // Remove from enemies layer and freeze behaviour
        gameObject.layer = 0;
        isActive = false;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void InitializeBehaviour()
    {
        // This will be called via the animation timeline
        // The ideia is to prevent suddenly being attack by a enemy that just spawned
        isActive = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        gameObject.layer = originalLayer;
    }

    private void BounceOnObstacle(Collision2D collision)
    {
        // Upon hiting a obstacle, turn in the oposite direction with a little randomness
        rb.linearVelocity = Vector2.zero;
        Vector2 newDirection = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            newDirection += collision.GetContact(i).normal;
        }

        if (Mathf.Abs(newDirection.x) < 0.01f) newDirection.x = Random.Range(-0.5f, 0.5f);
        if (Mathf.Abs(newDirection.y) < 0.01f) newDirection.y = Random.Range(-0.5f, 0.5f);

        currentDirection = newDirection.normalized;
    }

    private void HurtSequence()
    {
        hurtSequence?.Kill();
        hurtSequence = DOTween.Sequence();

        hurtSequence.Append(transform.DOPunchScale(Vector3.one * 0.4f, 0.1f));
        hurtSequence.Join(sRenderer.DOColor(Color.red, 0.1f));
        hurtSequence.Join(transform.DORotate(Vector3.forward * 20f, 0.1f));
        hurtSequence.Append(sRenderer.DOColor(Color.white, 0.08f));
        hurtSequence.Join(transform.DORotate(Vector3.zero, 0.08f));
        hurtSequence.OnComplete(() =>
        {
            // Checking if should return only after the feedback to keep the visuals consistent
            if (currentHealth < 1) Return();
        });

        hurtSequence.Play();
    }

    private void TryDropLoot()
    {
        int diceRoll = Random.Range(0, 6 + 1);

        // 1/6 chance to drop a item
        if (diceRoll < 6) return;

        int randomDropIndex = Random.Range(0, drops.Count - 1);
        PickableItem loot = CompositeObjectPooler.Instance.GetObject(pickablePrefab) as PickableItem;
        loot.Setup(drops[randomDropIndex]);
        loot.transform.position = transform.position;
    }

    private void OnEnable()
    {
        // Reset state variables
        currentHealth = maxHealth;
        currentDirection = new(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        currentMaxSpeed = maxSpeed;
    }

    private void OnDisable()
    {
        FreezeBehaviour();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((obstacleMask & 1 << (collision.gameObject.layer)) != 0)
        {
            BounceOnObstacle(collision);
        }
        else if ((hurtMask & 1 << (collision.gameObject.layer)) != 0)
        {
            currentHealth--;
            HurtSequence();
            if (currentHealth < 1)
            {
                TryDropLoot();
                OnDeath.Invoke(this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((targetMask & 1 << (collision.gameObject.layer)) == 0) return;

        target = collision.transform;
    }

    void OnTriggerExit2D(Collider2D collision)
    {        
        if ((targetMask & 1 << (collision.gameObject.layer)) == 0) return;

        target = null;
    }

    private void UpdateMovementVariables()
    {
        if (target == null)
        {
            currentMaxSpeed = maxSpeed;
        }
        else
        {
            currentMaxSpeed = maxSpeed * chaseSpeedMultiplier;
            currentDirection = (target.position - transform.position).normalized;
        }
    }

    private void AdjustVelocity()
    {
        velocity = rb.linearVelocity;
        Vector2 desiredVelocity = currentMaxSpeed * currentDirection;

        float t = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(velocity.x, desiredVelocity.x, t);
        float newY = Mathf.MoveTowards(velocity.y, desiredVelocity.y, t);

        velocity = new(newX, newY);
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        UpdateMovementVariables();
        AdjustVelocity();

        rb.linearVelocity = velocity;
    }
}
