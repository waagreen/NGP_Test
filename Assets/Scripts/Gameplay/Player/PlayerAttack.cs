using UnityEngine;

public class PlayerAttack : PlayerComponent
{
    [Header("Shoot settings")]
    [SerializeField] private Projectile projectilePrefab;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource shootAudio;

    private Vector2 shootDirection;
    private float lastShootTime;

    public override void Setup(InputManager input)
    {
        base.Setup(input);
        CompositeObjectPooler.Instance.InitializeNewQueue(projectilePrefab, 30);
        isSet = true;
    }

    private void HandleShoots()
    {
        if (shootDirection == Vector2.zero) return;
        if ((lastShootTime != 0) && (Time.time < (lastShootTime + projectilePrefab.Cooldown))) return;

        lastShootTime = Time.time;
        Projectile p = CompositeObjectPooler.Instance.GetObject(projectilePrefab) as Projectile;
        p.transform.position = transform.position;
        p.ApplyImpulse(shootDirection);
        shootAudio.Play();
    }

    private void Update()
    {
        shootDirection = input.Shoot;
    }

    private void FixedUpdate()
    {
        HandleShoots();
    }
}
