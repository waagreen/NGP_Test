using UnityEngine;
using DG.Tweening;

public class UpDownTween : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float floatDuration = 1f;
    [SerializeField] private Ease easeType = Ease.InOutSine;

    private Tween movement;

    private void Start()
    {
        StartFloatAnimation();
    }

    private void StartFloatAnimation()
    {
        movement = transform.DOLocalMoveY(transform.position.y + floatHeight, floatDuration)
        .SetEase(easeType)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        movement?.Kill();
    }
}