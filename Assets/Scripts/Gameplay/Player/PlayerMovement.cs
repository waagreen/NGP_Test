using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerMovement : PlayerComponent
{
    [Header("Movement settings")]
    [SerializeField, Range(1f, 50f)] private float maxSpeed = 10f;
    [SerializeField, Range(10f, 100f)] private float acceleration = 1f;
    [SerializeField] private SpriteRenderer sRenderer;

    // Assigned once per instance
    private Rigidbody2D rb;

    // Updated at runtime
    private Vector2 velocity;
    private Vector2 desiredVelocity;

    public override void Setup(InputManager input)
    {
        base.Setup(input);

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

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

        if (desiredVelocity.x > 0.1f)
        {
            sRenderer.flipX = false;
        }
        else if (desiredVelocity.x < -0.1f)
        {
            sRenderer.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        AdjustVelocity();
        rb.linearVelocity = velocity;
    }
}
