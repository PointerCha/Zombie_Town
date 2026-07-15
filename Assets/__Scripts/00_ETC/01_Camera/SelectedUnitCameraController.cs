using UnityEngine;

[DisallowMultipleComponent]
public sealed class SelectedUnitCameraController : MonoBehaviour
{
    [SerializeField] private UnitInputController inputController;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private bool snapOnFirstTarget = true;

    private Vector3 followVelocity;
    private bool hasTarget;

    private void Awake()
    {
        if (inputController == null)
        {
            inputController = FindObjectOfType<UnitInputController>();
        }

        if (target == null && inputController != null && inputController.SelectedUnit != null)
        {
            SetTarget(inputController.SelectedUnit.transform, snapOnFirstTarget);
        }
    }

    private void OnEnable()
    {
        if (inputController != null)
        {
            inputController.SelectedUnitChanged += HandleSelectedUnitChanged;
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            inputController.SelectedUnitChanged -= HandleSelectedUnitChanged;
        }
    }

    private void OnValidate()
    {
        smoothTime = Mathf.Max(0f, smoothTime);
        maxSpeed = Mathf.Max(0f, maxSpeed);
    }

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
            maxSpeed);
    }

    private void HandleSelectedUnitChanged(UnitController selectedUnit)
    {
        SetTarget(selectedUnit != null ? selectedUnit.transform : null, false);
    }

    private void SetTarget(Transform nextTarget, bool snap)
    {
        if (target == nextTarget)
        {
            return;
        }

        target = nextTarget;
        followVelocity = Vector3.zero;
        hasTarget = !snap;

        if (snap && target != null)
        {
            transform.position = target.position + followOffset;
            hasTarget = true;
        }
    }
}
