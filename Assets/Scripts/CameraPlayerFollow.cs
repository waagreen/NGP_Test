using UnityEngine;

public class CameraPlayerFollow : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float smoothTime = 1f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float cameraDistance = 10f;

    private Transform target;
    private Vector3 velocity;

    private void Start()
    {
        target = FindFirstObjectByType<Player>().transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new(target.position.x, target.position.y, -cameraDistance);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, maxSpeed);
    }
}