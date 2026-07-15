using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class GeneratedTestUnitController : MonoBehaviour
{
    private const int SelectionHitBufferSize = 8;
    private const float CollisionSkin = 0.01f;

    private static Sprite defaultUnitSprite;

    [Header("Input")]
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private bool selectedOnStart = true;
    [SerializeField] private float selectionRadius = 0.45f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.04f;

    [Header("Debug")]
    [SerializeField] private bool logTriggerChanges = true;
    [SerializeField] private bool logInputEvents = true;

    [Header("Visual")]
    [SerializeField] private Color selectedColor = new Color(0.1f, 1f, 0.25f, 1f);
    [SerializeField] private Color idleColor = new Color(0.25f, 0.8f, 1f, 1f);
    [SerializeField] private int spriteSortingOrder = 500;

    private readonly Collider2D[] selectionHits = new Collider2D[SelectionHitBufferSize];
    private readonly RaycastHit2D[] movementHits = new RaycastHit2D[8];
    private readonly HashSet<GeneratedRuntimeIdentity> activeTriggerIdentities = new HashSet<GeneratedRuntimeIdentity>();

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    private GeneratedFloorIsolationAgent floorIsolationAgent;
    private GeneratedVisibilityAgent visibilityAgent;
    private GeneratedBuildingContextAgent buildingContextAgent;
    private ContactFilter2D movementFilter;
    private Collider2D lastBlockingCollider;
    private Vector2 destination;
    private bool selected;
    private bool hasDestination;
    private bool warnedMissingCamera;

    public bool IsSelected => selected;
    public int ActiveTriggerCount => activeTriggerIdentities.Count;
    public int CurrentFloorIndex => floorIsolationAgent != null ? floorIsolationAgent.CurrentFloorIndex : 1;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        floorIsolationAgent = GetComponent<GeneratedFloorIsolationAgent>();
        visibilityAgent = GetComponent<GeneratedVisibilityAgent>();

        if (floorIsolationAgent == null)
        {
            floorIsolationAgent = gameObject.AddComponent<GeneratedFloorIsolationAgent>();
        }

        if (visibilityAgent == null)
        {
            visibilityAgent = gameObject.AddComponent<GeneratedVisibilityAgent>();
        }

        buildingContextAgent = GetComponent<GeneratedBuildingContextAgent>();

        if (buildingContextAgent == null)
        {
            buildingContextAgent = gameObject.AddComponent<GeneratedBuildingContextAgent>();
        }

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (bodyCollider != null)
        {
            bodyCollider.isTrigger = false;
        }

        EnsureVisual();

        movementFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = false
        };

        ResolveSceneCamera();

        destination = body.position;
        selected = selectedOnStart;
        ApplyVisualState();
    }

    private void Start()
    {
        floorIsolationAgent.RefreshFloorCollisionIsolation();
    }

    private void Update()
    {
        if (!ResolveSceneCamera())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = GetMouseWorldPoint();
            selected = IsPointerOnThisUnit(worldPoint);
            ApplyVisualState();

            if (logInputEvents)
            {
                Debug.Log($"{name}: left click world={worldPoint}, selected={selected}", this);
            }
        }

        if (selected && Input.GetMouseButtonDown(1))
        {
            Vector2 worldPoint = GetMouseWorldPoint();
            SetDestination(worldPoint);

            if (logInputEvents)
            {
                Debug.Log($"{name}: right click move destination={worldPoint}", this);
            }
        }
        else if (!selected && Input.GetMouseButtonDown(1) && logInputEvents)
        {
            Debug.Log($"{name}: right click ignored because this test Unit is not selected.", this);
        }
    }

    private void FixedUpdate()
    {
        if (!hasDestination)
        {
            return;
        }

        Vector2 currentPosition = body.position;
        Vector2 toDestination = destination - currentPosition;

        if (toDestination.sqrMagnitude <= stoppingDistance * stoppingDistance)
        {
            body.MovePosition(destination);
            hasDestination = false;
            return;
        }

        Vector2 direction = toDestination.normalized;
        float distance = Mathf.Min(moveSpeed * Time.fixedDeltaTime, toDestination.magnitude);
        Vector2 nextPosition = GetCollisionAwarePosition(currentPosition, direction, distance);

        if ((nextPosition - currentPosition).sqrMagnitude <= Mathf.Epsilon)
        {
            hasDestination = false;

            if (logInputEvents)
            {
                string blocker = lastBlockingCollider != null ? lastBlockingCollider.name : "unknown";
                GeneratedRuntimeIdentity identity = lastBlockingCollider != null
                    ? lastBlockingCollider.GetComponent<GeneratedRuntimeIdentity>()
                    : null;
                string identityText = identity != null
                    ? $" kind={identity.ObjectKind} floor={identity.FloorIndex} room={identity.RoomIndex}"
                    : string.Empty;
                Debug.Log($"{name}: movement blocked by {blocker}{identityText}", this);
            }

            return;
        }

        body.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        buildingContextAgent?.NotifyTriggerEnter(
            other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null
        );
        TryAddTriggerIdentity(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TryRemoveTriggerIdentity(other);
        buildingContextAgent?.NotifyTriggerExit(
            other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null
        );
    }

    public void SetDestination(Vector2 targetPosition)
    {
        destination = targetPosition;
        hasDestination = true;
    }

    public void SetSelected(bool newSelected)
    {
        selected = newSelected;
        ApplyVisualState();
    }

    public void SetCurrentFloorIndex(int newFloorIndex)
    {
        int previousFloorIndex = CurrentFloorIndex;
        visibilityAgent.ClearVisibilityRegistrations();
        floorIsolationAgent.SetCurrentFloorIndex(newFloorIndex);
        visibilityAgent.ApplyCurrentFloorVisuals(CurrentFloorIndex);
        activeTriggerIdentities.RemoveWhere(IsIdentityInactiveForCurrentFloor);

        if (logTriggerChanges && previousFloorIndex != CurrentFloorIndex)
        {
            Debug.Log($"{name}: current floor changed to {CurrentFloorIndex}.", this);
        }
    }

    private Vector2 GetCollisionAwarePosition(Vector2 currentPosition, Vector2 direction, float distance)
    {
        if (bodyCollider == null || distance <= 0f)
        {
            return currentPosition;
        }

        int hitCount = bodyCollider.Cast(direction, movementFilter, movementHits, distance + CollisionSkin);
        float allowedDistance = distance;
        lastBlockingCollider = null;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = movementHits[i];

            if (hit.collider == null || hit.collider.isTrigger)
            {
                continue;
            }

            if (!floorIsolationAgent.IsColliderActive(hit.collider))
            {
                continue;
            }

            float hitAllowedDistance = Mathf.Max(0f, hit.distance - CollisionSkin);

            if (hitAllowedDistance < allowedDistance)
            {
                allowedDistance = hitAllowedDistance;
                lastBlockingCollider = hit.collider;
            }
        }

        return currentPosition + direction * allowedDistance;
    }

    private bool IsPointerOnThisUnit(Vector2 worldPoint)
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(worldPoint, selectionRadius, selectionHits);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = selectionHits[i];
            selectionHits[i] = null;

            if (hit != null && hit.GetComponentInParent<GeneratedTestUnitController>() == this)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetMouseWorldPoint()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -sceneCamera.transform.position.z;
        return sceneCamera.ScreenToWorldPoint(mousePosition);
    }

    private bool ResolveSceneCamera()
    {
        if (sceneCamera != null)
        {
            return true;
        }

        sceneCamera = Camera.main;

        if (sceneCamera == null)
        {
            sceneCamera = FindObjectOfType<Camera>();
        }

        if (sceneCamera != null)
        {
            return true;
        }

        if (!warnedMissingCamera)
        {
            warnedMissingCamera = true;
            Debug.LogWarning($"{name}: no Camera found. Test Unit input is disabled until a Camera exists.", this);
        }

        return false;
    }

    private void EnsureVisual()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetDefaultUnitSprite();
        }

        spriteRenderer.sortingOrder = spriteSortingOrder;
    }

    private void ApplyVisualState()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = selected ? selectedColor : idleColor;
    }

    private static Sprite GetDefaultUnitSprite()
    {
        if (defaultUnitSprite != null)
        {
            return defaultUnitSprite;
        }

        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color fill = Color.white;
        Vector2 center = new Vector2(7.5f, 7.5f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= 7f ? fill : clear);
            }
        }

        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        defaultUnitSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        defaultUnitSprite.hideFlags = HideFlags.HideAndDontSave;
        return defaultUnitSprite;
    }

    private void TryAddTriggerIdentity(Collider2D other)
    {
        GeneratedRuntimeIdentity identity = other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null;

        if (identity == null ||
            !identity.TriggerObject ||
            !floorIsolationAgent.IsIdentityActive(identity) ||
            !activeTriggerIdentities.Add(identity))
        {
            return;
        }

        if (logTriggerChanges)
        {
            Debug.Log(
                $"{name}: entered trigger {identity.ObjectKind} id={identity.ObjectId} floor={identity.FloorIndex} room={identity.RoomIndex}",
                this
            );
        }
    }

    private void TryRemoveTriggerIdentity(Collider2D other)
    {
        GeneratedRuntimeIdentity identity = other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null;

        if (identity == null || !activeTriggerIdentities.Remove(identity))
        {
            return;
        }

        if (logTriggerChanges)
        {
            Debug.Log(
                $"{name}: exited trigger {identity.ObjectKind} id={identity.ObjectId} floor={identity.FloorIndex} room={identity.RoomIndex}",
                this
            );
        }
    }

    private bool IsIdentityInactiveForCurrentFloor(GeneratedRuntimeIdentity identity)
    {
        return !floorIsolationAgent.IsIdentityActive(identity);
    }
}
