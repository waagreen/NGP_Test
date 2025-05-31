using UnityEngine;

public class CameraPlayerFollow : MonoBehaviour
{
    [SerializeField] private float smoothTime = 2f;
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