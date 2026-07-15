using UnityEngine;

[DisallowMultipleComponent]
public sealed class UnitSortingController : MonoBehaviour
{
    private static readonly Vector2[] DefaultNormalizedSamplePoints =
    {
        new Vector2(0.5f, 0f),
        new Vector2(0.5f, 0.35f),
        new Vector2(0.5f, 0.7f),
        new Vector2(0.25f, 0.45f),
        new Vector2(0.75f, 0.45f)
    };

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Collider2D sampleCollider;
    [SerializeField] private Vector2 sampleOffset;
    [SerializeField] private Vector2[] normalizedSamplePoints = DefaultNormalizedSamplePoints;
    [SerializeField] private int sortingOffset = 1;
    [SerializeField] private int fallbackSortingOrder = 50;

    private UnitFloorAgent floorAgent;
    private int lastSortingOrder = int.MinValue;

    private void Awake()
    {
        floorAgent = GetComponent<UnitFloorAgent>();
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        if (sampleCollider == null)
        {
            sampleCollider = GetComponent<Collider2D>();
        }

        ApplySorting();
    }

    private void LateUpdate()
    {
        ApplySorting();
    }

    private void OnValidate()
    {
        sortingOffset = Mathf.Max(0, sortingOffset);
    }

    private void ApplySorting()
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        int targetSortingOrder = fallbackSortingOrder;
        FloorController currentFloor = floorAgent != null ? floorAgent.CurrentFloor : null;
        if (currentFloor != null)
        {
            targetSortingOrder = GetHighestSortingOrderFromSamples(currentFloor) + sortingOffset;
        }

        if (lastSortingOrder == targetSortingOrder)
        {
            return;
        }

        lastSortingOrder = targetSortingOrder;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.sortingOrder = targetSortingOrder;
        }
    }

    private int GetHighestSortingOrderFromSamples(FloorController currentFloor)
    {
        int highestSortingOrder = fallbackSortingOrder;
        Vector2[] samplePoints = normalizedSamplePoints != null && normalizedSamplePoints.Length > 0
            ? normalizedSamplePoints
            : DefaultNormalizedSamplePoints;

        if (sampleCollider == null)
        {
            return currentFloor.GetHighestTilemapSortingOrderAt(transform.position + (Vector3)sampleOffset, highestSortingOrder);
        }

        Bounds bounds = sampleCollider.bounds;
        for (int i = 0; i < samplePoints.Length; i++)
        {
            Vector2 normalizedPoint = samplePoints[i];
            Vector3 samplePosition = new Vector3(
                Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedPoint.x),
                Mathf.Lerp(bounds.min.y, bounds.max.y, normalizedPoint.y),
                transform.position.z);

            samplePosition += (Vector3)sampleOffset;
            highestSortingOrder = currentFloor.GetHighestTilemapSortingOrderAt(samplePosition, highestSortingOrder);
        }

        return highestSortingOrder;
    }
}
