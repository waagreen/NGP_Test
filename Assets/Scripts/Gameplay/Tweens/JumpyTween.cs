using UnityEngine;
using DG.Tweening;

public class JumpyCharacterTween : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float jumpHeight = 0.3f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float tiltAngle = 5f;

    Sequence jumpSequence;

    private void Start()
    {
        StartJumpAnimation();
    }

    private void StartJumpAnimation()
    {
        jumpSequence = DOTween.Sequence();
        
        jumpSequence.Append(transform.DOLocalMoveY(transform.position.y + jumpHeight, jumpDuration/2).SetEase(Ease.OutQuad));
        jumpSequence.Join(transform.DOLocalRotate(new Vector3(0, 0, tiltAngle), jumpDuration/2).SetEase(Ease.OutQuad));
        
        jumpSequence.Append(transform.DOLocalMoveY(transform.position.y, jumpDuration/2).SetEase(Ease.InQuad));
        jumpSequence.Join(transform.DOLocalRotate(Vector3.zero, jumpDuration/2).SetEase(Ease.InQuad));
        
        jumpSequence.SetLoops(-1);
    }

    private void OnDestroy()
    {
        jumpSequence?.Kill();
    }
}