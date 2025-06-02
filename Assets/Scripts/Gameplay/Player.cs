using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private int health = 3;
    
    [Header("Movement settings")]
    [SerializeField, Range(1f, 50f)] private float maxSpeed = 10f;
    [SerializeField, Range(10f, 100f)] private float acceleration = 1f;

    [Header("Shoot setting")]
    [SerializeField] private Projectile projectilePrefab;

    [Header("Detection Setting")]
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private LayerMask hurtLayer;
    [SerializeField] private SpriteRenderer sRenderer;

    // Assigned once per instance
    private Rigidbody2D rb;
    private Collider2D col;
    private InputManager input;
    private bool isSet = false;

    // Updated at runtime
    private Vector2 velocity;
    private Vector2 desiredVelocity;
    private Vector2 shootDirection;
    private float lastShootTime;
    private float lastHitTime;

    private Sequence hurtSequence;

    public event Action<int> OnHurt;

    public void Setup(InputManager input)
    {
        if (isSet) return;

        this.input = input;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        col = GetComponent<Collider2D>();
        CompositeObjectPooler.Instance.InitializeNewQueue(projectilePrefab, 30);

        isSet = true;
    }

    private void AdjustVelocity()
    {
        velocity = rb.linearVelocity;

        float t = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(velocity.x, desiredVelocity.x, t);
        float newY = Mathf.MoveTowards(velocity.y, desiredVelocity.y, t);

        velocity = new(newX, newY);
    }

    private void HandleShoots()
    {
        if (shootDirection == Vector2.zero) return;
        if ((lastShootTime != 0) && (Time.time < (lastShootTime + projectilePrefab.Cooldown))) return;

        lastShootTime = Time.time;
        Projectile p = CompositeObjectPooler.Instance.GetObject(projectilePrefab) as Projectile;
        p.transform.position = transform.position;
        p.ApplyImpulse(shootDirection);
    }

    private void HurtSequence()
    {
        hurtSequence?.Kill();
        hurtSequence = DOTween.Sequence();

        hurtSequence.Append(transform.DOPunchScale(Vector3.one * 0.4f, 0.15f));
        hurtSequence.Join(sRenderer.DOColor(Color.red, 0.15f));
        hurtSequence.Join(transform.DORotate(Vector3.forward * 20f, 0.15f));
        hurtSequence.Append(sRenderer.DOColor(Color.white, 0.15f));
        hurtSequence.Join(transform.DORotate(Vector3.zero, 0.15f));

        hurtSequence.Play();
    }

    private void Update()
    {
        desiredVelocity = input.Movement * maxSpeed;
        shootDirection = input.Shoot;
    }

    private void FixedUpdate()
    {
        HandleShoots();

        AdjustVelocity();
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((hurtLayer & (1 << collision.gameObject.layer)) == 0) return;
        if ((lastHitTime != 0) && Time.time < (lastHitTime + invincibilityTime)) return;

        health = Math.Max(0, health - 1);
        OnHurt.Invoke(health);
        lastHitTime = Time.time;

        HurtSequence();

        if (health == 0) gameObject.SetActive(false);
    }
}
