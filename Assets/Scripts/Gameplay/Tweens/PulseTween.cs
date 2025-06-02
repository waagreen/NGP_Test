using UnityEngine;
using DG.Tweening;

public class PulseTween : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 0.8f;
    [SerializeField] private Ease easeType = Ease.InOutSine;

    private Tween pulse;

    private void Start()
    {
        StartPulseAnimation();
    }

    private void StartPulseAnimation()
    {
        pulse = transform.DOScale(transform.localScale * pulseScale, pulseDuration)
        .SetEase(easeType)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        pulse?.Kill();
    }
}