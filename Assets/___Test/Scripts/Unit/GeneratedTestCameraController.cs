using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class GeneratedTestCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -10f);
    [SerializeField, Min(0f)] private float smoothTime = 0.12f;
    [SerializeField, Min(0f)] private float maxSpeed = 150f;

    private Vector3 followVelocity;
    private bool hasTarget;

    public Transform Target => target;

    private void LateUpdate()
    {
        if (target == null)
        {
            hasTarget = false;
            followVelocity = Vector3.zero;
            return;
        }

        Vector3 desiredPosition = target.position + followOffset;

        if (!hasTarget)
        {
            transform.position = desiredPosition;
            hasTarget = true;
            return;
        }

        if (smoothTime <= 0f)
        {
            transform.position = desiredPosition;
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            smoothTime,
            maxSpeed
        );
    }

    public void SetTarget(Transform newTarget, bool snap)
    {
        if (target == newTarget)
        {
            return;
        }

        target = newTarget;
        followVelocity = Vector3.zero;
        hasTarget = !snap;

        if (snap && target != null)
        {
            transform.position = target.position + followOffset;
            hasTarget = true;
        }
    }
}
