using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Enemy : PoolableObject
{
    [SerializeField] private int health = 2;

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

    private Transform target;
    private Vector2 currentDirection;
    private Vector2 velocity;
    private float currentMaxSpeed;

    public System.Action<Enemy> OnDeath;

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

    private void OnEnable()
    {
        currentDirection = new(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        currentMaxSpeed = maxSpeed;

        if ((rb != null) || (col != null)) return;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

        rb.gravityScale = 0f;
        col.radius = perceptionRadius;
        col.isTrigger = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((obstacleMask & 1 << (collision.gameObject.layer)) != 0)
        {
            BounceOnObstacle(collision);
        }
        else if ((hurtMask & 1 << (collision.gameObject.layer)) != 0)
        {
            health--;
            if (health < 1)
            {
                Return();
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
        UpdateMovementVariables();
        AdjustVelocity();
        
        rb.linearVelocity = velocity;
    }
}
