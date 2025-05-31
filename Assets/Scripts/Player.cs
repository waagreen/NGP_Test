using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField, Range(1f, 50f)] private float maxSpeed = 10f;
    [SerializeField, Range(10f, 100f)] private float acceleration = 1f;

    // Assigned once per instance
    private Rigidbody2D rb;
    private Collider2D col;
    private InputManager input;
    private bool isSet = false;

    // Updated at runtime
    private Vector2 velocity;
    private Vector2 desiredVelocity;

    public void Setup(InputManager input)
    {
        if (isSet) return;

        this.input = input;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        col = GetComponent<Collider2D>();

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

    private void Update()
    {
        desiredVelocity = input.Movement * maxSpeed;
    }

    private void FixedUpdate()
    {
        AdjustVelocity();
        rb.linearVelocity = velocity;
    }
}
