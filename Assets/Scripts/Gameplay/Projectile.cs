using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : PoolableObject
{
    [SerializeField, Range(0f, 5f)] private float attackCooldown = 2f;
    [SerializeField, Range(1f, 50f)] private float projectileSpeed = 2f;
    [SerializeField] private LayerMask collisionMask = -1;

    private Rigidbody2D rb;

    public float Cooldown => attackCooldown;

    private void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
    }

    public void ApplyImpulse(Vector2 direction)
    {
        rb.AddForce(direction * projectileSpeed, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // First shift 1 left by the value of collision layer
        // Then use the AND operator to check if the layer isn't included on the mask
        if ((collisionMask & (1 << collision.gameObject.layer)) == 0) return;
        Return();
    }
}
