using UnityEngine;

public sealed class UnitController : MonoBehaviour
{
    private const float DirectionStep = 1f / 7f;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int IdleStateHash = Animator.StringToHash("IdleState");
    private static readonly int MoveStateHash = Animator.StringToHash("MoveState");

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.02f;
    [SerializeField] private float collisionSkin = 0.01f;

    [Header("Optional References")]
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private GameObject initialFloor;

    private Animator animator;
    private Rigidbody2D body;
    private UnitFloorAgent floorAgent;
    private Vector2 destination;
    private ContactFilter2D movementContactFilter;
    private readonly RaycastHit2D[] movementHits = new RaycastHit2D[8];
    private float stoppingDistanceSqr;
    private float lastFacingState;
    private bool isSelected;
    private bool hasDestination;

    public bool IsSelected => isSelected;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        floorAgent = GetComponent<UnitFloorAgent>();
        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        movementContactFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = false
        };

        destination = body != null ? body.position : (Vector2)transform.position;
        stoppingDistanceSqr = Mathf.Max(0f, stoppingDistance * stoppingDistance);

        ApplySelectionState(false);
        UpdateIdleAnimation(lastFacingState);
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        stoppingDistance = Mathf.Max(0f, stoppingDistance);
        collisionSkin = Mathf.Max(0f, collisionSkin);
        stoppingDistanceSqr = stoppingDistance * stoppingDistance;
    }

    private void FixedUpdate()
    {
        if (!hasDestination || body == null)
        {
            return;
        }

        Vector2 currentPosition = body.position;
        Vector2 toDestination = destination - currentPosition;
        float remainingDistanceSqr = toDestination.sqrMagnitude;

        if (remainingDistanceSqr <= stoppingDistanceSqr)
        {
            body.MovePosition(destination);
            hasDestination = false;
            UpdateIdleAnimation(lastFacingState);
            return;
        }

        Vector2 direction = toDestination.normalized;
        float directionState = DirectionToState(direction);
        lastFacingState = directionState;

        UpdateMoveAnimation(directionState);

        float maxDistanceDelta = moveSpeed * Time.fixedDeltaTime;
        float travelDistance = Mathf.Min(maxDistanceDelta, toDestination.magnitude);
        Vector2 nextPosition = GetCollisionAwarePosition(currentPosition, direction, travelDistance);
        if ((nextPosition - currentPosition).sqrMagnitude <= Mathf.Epsilon)
        {
            hasDestination = false;
            UpdateIdleAnimation(lastFacingState);
            return;
        }

        body.MovePosition(nextPosition);
    }

    public void SetSelected(bool selected)
    {
        if (isSelected == selected)
        {
            return;
        }

        isSelected = selected;
        ApplySelectionState(selected);
    }

    public void SetDestination(Vector2 targetPosition)
    {
        destination = targetPosition;
        hasDestination = true;

        Vector2 currentPosition = body != null ? body.position : (Vector2)transform.position;
        Vector2 toDestination = destination - currentPosition;

        if (toDestination.sqrMagnitude <= stoppingDistanceSqr)
        {
            hasDestination = false;
            UpdateIdleAnimation(lastFacingState);
            return;
        }

        float directionState = DirectionToState(toDestination);
        lastFacingState = directionState;
        UpdateMoveAnimation(directionState);
    }

    public void Stop()
    {
        hasDestination = false;
        UpdateIdleAnimation(lastFacingState);
    }

    private void ApplySelectionState(bool selected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
    }

    private void UpdateMoveAnimation(float directionState)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(MoveStateHash, directionState);
        animator.SetBool(MoveHash, true);
    }

    private void UpdateIdleAnimation(float directionState)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(IdleStateHash, directionState);
        animator.SetBool(MoveHash, false);
    }

    private Vector2 GetCollisionAwarePosition(Vector2 currentPosition, Vector2 direction, float distance)
    {
        if (body == null || distance <= 0f)
        {
            return currentPosition;
        }

        float allowedDistance = distance;
        Collider2D[] movementColliders = floorAgent != null ? floorAgent.Colliders : null;
        if (movementColliders != null && movementColliders.Length > 0)
        {
            for (int colliderIndex = 0; colliderIndex < movementColliders.Length; colliderIndex++)
            {
                Collider2D movementCollider = movementColliders[colliderIndex];
                if (movementCollider == null || movementCollider.isTrigger || !movementCollider.enabled)
                {
                    continue;
                }

                allowedDistance = GetAllowedDistanceFromCast(
                    movementCollider.Cast(direction, movementContactFilter, movementHits, distance + collisionSkin),
                    allowedDistance);
            }

            return currentPosition + direction * allowedDistance;
        }

        allowedDistance = GetAllowedDistanceFromCast(
            body.Cast(direction, movementContactFilter, movementHits, distance + collisionSkin),
            allowedDistance);

        return currentPosition + direction * allowedDistance;
    }

    private float GetAllowedDistanceFromCast(int hitCount, float allowedDistance)
    {
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = movementHits[i];
            if (hit.collider == null || hit.collider.isTrigger)
            {
                continue;
            }

            if (floorAgent != null && !floorAgent.CanCollideWith(hit.collider))
            {
                continue;
            }

            allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, hit.distance - collisionSkin));
        }

        return allowedDistance;
    }

    private static float DirectionToState(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return 0f;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f)
        {
            return 0f;
        }

        if (angle >= -67.5f && angle < -22.5f)
        {
            return DirectionStep;
        }

        if (angle >= -112.5f && angle < -67.5f)
        {
            return DirectionStep * 2f;
        }

        if (angle >= -157.5f && angle < -112.5f)
        {
            return DirectionStep * 3f;
        }

        if (angle >= 157.5f || angle < -157.5f)
        {
            return DirectionStep * 4f;
        }

        if (angle >= 112.5f && angle < 157.5f)
        {
            return DirectionStep * 5f;
        }

        if (angle >= 67.5f && angle < 112.5f)
        {
            return DirectionStep * 6f;
        }

        return 1f;
    }
}
