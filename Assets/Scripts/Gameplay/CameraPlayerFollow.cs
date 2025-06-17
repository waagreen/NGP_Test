using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Camera))]
public class CameraPlayerFollow : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float smoothTime = 1f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float cameraDistance = 10f;

    private Transform target;
    private Vector3 velocity;
    private Camera cam;
    private Sequence shakeSequence;

    public Camera Cam => cam;

    public void Setup(Transform target)
    {
        this.target = target;
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new(target.position.x, target.position.y, -cameraDistance);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, maxSpeed);
    }

    public void Shake(int life)
    {
        shakeSequence?.Kill();
        shakeSequence = DOTween.Sequence();

        shakeSequence.Append(cam.DOShakePosition(0.04f, 0.3f, 2, 45f));
        shakeSequence.SetEase(Ease.InCubic);
        shakeSequence.Play();
    }
}