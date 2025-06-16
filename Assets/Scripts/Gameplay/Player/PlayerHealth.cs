using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class PlayerHealth : PlayerComponent, ISaveData
{
    [SerializeField] private int initialHealth = 3;
    [SerializeField] private AudioSource hurtAudio;

    [Header("Detection Settings")]
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private LayerMask hurtLayer;
    [SerializeField] private SpriteRenderer sRenderer;

    public event Action<int> OnHurt;
    public int Health => currentHealth;

    private Collider2D col;
    private Sequence hurtSequence;

    private int currentHealth;
    private float lastHitTime;

    public override void Setup(InputManager input)
    {
        base.Setup(input);
        col = GetComponent<Collider2D>();
        isSet = true;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((hurtLayer & (1 << collision.gameObject.layer)) == 0) return;
        if ((lastHitTime != 0) && (Time.time < (lastHitTime + invincibilityTime))) return;

        currentHealth = Math.Max(0, currentHealth - 1);
        OnHurt.Invoke(currentHealth);
        lastHitTime = Time.time;

        hurtAudio.Play();
        HurtSequence();

        if (currentHealth == 0) gameObject.SetActive(false);
    }

    public void SaveData(ref SaveData data)
    {
        data.playerHealth = currentHealth;
    }

    public void LoadData(SaveData data)
    {
        currentHealth = (data.playerHealth > 0) ? data.playerHealth : initialHealth;
    }
}
