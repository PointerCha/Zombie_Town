using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public sealed class FloorController : MonoBehaviour
{
    [SerializeField] private BuildingController buildingController;
    [SerializeField] private VisibilityTargetGroup underWallGroup;
    [SerializeField] private Collider2D[] floorColliders;
    [SerializeField] private Renderer[] floorRenderers;
    [SerializeField] private Tilemap[] floorTilemaps;
    [SerializeField] private Tilemap[] sortingTilemaps;
    [SerializeField] private SpriteRenderer[] floorSpriteRenderers;
    [SerializeField] private VisibilityTargetGroup[] floorVisibilityGroups;
    [SerializeField] private float visualTransitionDuration = 0.5f;

    private readonly HashSet<UnitVisibilityAgent> occupants = new HashSet<UnitVisibilityAgent>();
    private Color[] initialTilemapColors;
    private Color[] initialSpriteColors;
    private Tilemap[] cachedSortingTilemaps;
    private TilemapRenderer[] cachedSortingRenderers;
    private Collider2D[] managedCollisionColliders;
    private Coroutine visualFadeCoroutine;
    private Coroutine backOcclusionFadeCoroutine;
    private float visualAlpha = 1f;
    private float targetVisualAlpha = 1f;
    private float backOcclusionAlpha = 1f;
    private float backOcclusionGroundTilemapAlpha = 1f;
    private float targetBackOcclusionAlpha = 1f;
    private float targetBackOcclusionGroundTilemapAlpha = 1f;
    private bool desiredVisible = true;

    public BuildingController BuildingController => buildingController;

    private void Awake()
    {
        if (buildingController == null)
        {
            buildingController = GetComponentInParent<BuildingController>();
        }

        CacheVisualTargets();
        CacheSortingTargets();
        CacheCollisionTargets();
    }

    private void OnValidate()
    {
        visualTransitionDuration = Mathf.Max(0f, visualTransitionDuration);
    }

    private void OnEnable()
    {
        ApplyVisualAlpha();
    }

    private void OnDisable()
    {
        visualFadeCoroutine = null;
        backOcclusionFadeCoroutine = null;
    }

    public void RegisterOccupant(UnitVisibilityAgent agent)
    {
        if (agent == null || !occupants.Add(agent))
        {
            return;
        }

        if (occupants.Count == 1 && underWallGroup != null)
        {
            underWallGroup.RequestReveal();
        }

        if (buildingController != null)
        {
            buildingController.NotifyFloorEntered(this);
        }
    }

    public void UnregisterOccupant(UnitVisibilityAgent agent)
    {
        if (agent == null || !occupants.Remove(agent))
        {
            return;
        }

        if (occupants.Count == 0 && underWallGroup != null)
        {
            underWallGroup.ReleaseReveal();
        }

        if (occupants.Count == 0 && buildingController != null)
        {
            buildingController.NotifyFloorExited(this);
        }
    }

    public void SetCollisionAccess(UnitFloorAgent agent, bool canCollide)
    {
        if (agent == null)
        {
            return;
        }

        EnsureCollisionTargets();

        Collider2D[] unitColliders = agent.Colliders;
        if (unitColliders == null)
        {
            return;
        }

        for (int unitIndex = 0; unitIndex < unitColliders.Length; unitIndex++)
        {
            Collider2D unitCollider = unitColliders[unitIndex];
            if (unitCollider == null)
            {
                continue;
            }

            for (int floorIndex = 0; floorIndex < managedCollisionColliders.Length; floorIndex++)
            {
                Collider2D floorCollider = managedCollisionColliders[floorIndex];
                if (floorCollider == null || floorCollider == unitCollider)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(unitCollider, floorCollider, !canCollide);
            }
        }
    }

    public void SetFloorCollisionEnabled(bool enabled)
    {
        EnsureCollisionTargets();

        for (int i = 0; i < managedCollisionColliders.Length; i++)
        {
            Collider2D floorCollider = managedCollisionColliders[i];
            if (floorCollider == null)
            {
                continue;
            }

            floorCollider.enabled = enabled;
        }
    }

    public bool ContainsCollisionCollider(Collider2D target)
    {
        if (target == null)
        {
            return false;
        }

        EnsureCollisionTargets();
        for (int i = 0; i < managedCollisionColliders.Length; i++)
        {
            if (managedCollisionColliders[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    public int GetHighestTilemapSortingOrderAt(Vector3 worldPosition, int fallbackSortingOrder)
    {
        int highestSortingOrder = fallbackSortingOrder;
        EnsureSortingTargets();
        if (cachedSortingTilemaps == null)
        {
            return highestSortingOrder;
        }

        for (int i = 0; i < cachedSortingTilemaps.Length; i++)
        {
            Tilemap tilemap = cachedSortingTilemaps[i];
            if (tilemap == null || !tilemap.HasTile(tilemap.WorldToCell(worldPosition)))
            {
                continue;
            }

            TilemapRenderer tilemapRenderer = cachedSortingRenderers[i];
            if (tilemapRenderer == null || !tilemapRenderer.enabled)
            {
                continue;
            }

            highestSortingOrder = Mathf.Max(highestSortingOrder, tilemapRenderer.sortingOrder);
        }

        return highestSortingOrder;
    }

    private void EnsureSortingTargets()
    {
        if (cachedSortingTilemaps == null || cachedSortingRenderers == null)
        {
            CacheSortingTargets();
        }
    }

    private void EnsureCollisionTargets()
    {
        if (managedCollisionColliders == null)
        {
            CacheCollisionTargets();
        }
    }

    private void CacheCollisionTargets()
    {
        if (floorColliders == null || floorColliders.Length == 0)
        {
            managedCollisionColliders = System.Array.Empty<Collider2D>();
            return;
        }

        List<Collider2D> targets = new List<Collider2D>(floorColliders.Length * 2);
        HashSet<Collider2D> uniqueTargets = new HashSet<Collider2D>();

        for (int i = 0; i < floorColliders.Length; i++)
        {
            Collider2D floorCollider = floorColliders[i];
            if (floorCollider == null)
            {
                continue;
            }

            AddCollisionTarget(floorCollider, targets, uniqueTargets);

            Collider2D[] siblingColliders = floorCollider.GetComponents<Collider2D>();
            for (int siblingIndex = 0; siblingIndex < siblingColliders.Length; siblingIndex++)
            {
                AddCollisionTarget(siblingColliders[siblingIndex], targets, uniqueTargets);
            }
        }

        managedCollisionColliders = targets.ToArray();
    }

    private void CacheSortingTargets()
    {
        List<Tilemap> tilemaps = new List<Tilemap>();
        HashSet<Tilemap> uniqueTilemaps = new HashSet<Tilemap>();

        if (sortingTilemaps != null && sortingTilemaps.Length > 0)
        {
            AddSortingTilemaps(sortingTilemaps, tilemaps, uniqueTilemaps);
        }
        else
        {
            AddSortingTilemaps(floorTilemaps, tilemaps, uniqueTilemaps);

            if (floorRenderers != null)
            {
                for (int i = 0; i < floorRenderers.Length; i++)
                {
                    Renderer floorRenderer = floorRenderers[i];
                    if (floorRenderer == null || !(floorRenderer is TilemapRenderer))
                    {
                        continue;
                    }

                    AddSortingTilemap(floorRenderer.GetComponent<Tilemap>(), tilemaps, uniqueTilemaps);
                }
            }
        }

        cachedSortingTilemaps = tilemaps.ToArray();
        cachedSortingRenderers = new TilemapRenderer[cachedSortingTilemaps.Length];
        for (int i = 0; i < cachedSortingTilemaps.Length; i++)
        {
            cachedSortingRenderers[i] = cachedSortingTilemaps[i] != null
                ? cachedSortingTilemaps[i].GetComponent<TilemapRenderer>()
                : null;
        }
    }

    private static void AddSortingTilemaps(Tilemap[] source, List<Tilemap> targets, HashSet<Tilemap> uniqueTargets)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Length; i++)
        {
            AddSortingTilemap(source[i], targets, uniqueTargets);
        }
    }

    private static void AddSortingTilemap(Tilemap tilemap, List<Tilemap> targets, HashSet<Tilemap> uniqueTargets)
    {
        if (tilemap == null || !uniqueTargets.Add(tilemap))
        {
            return;
        }

        targets.Add(tilemap);
    }

    private static void AddCollisionTarget(Collider2D collider, List<Collider2D> targets, HashSet<Collider2D> uniqueTargets)
    {
        if (collider == null || !uniqueTargets.Add(collider))
        {
            return;
        }

        targets.Add(collider);
    }

    public void SetVisualsVisible(bool visible)
    {
        desiredVisible = visible;
        ApplyRequestedVisualState();
    }

    private void ApplyRequestedVisualState()
    {
        SetVisualAlpha(desiredVisible ? 1f : 0f, visualTransitionDuration);
    }

    public void SetVisualAlpha(float alpha, float duration)
    {
        alpha = Mathf.Clamp01(alpha);
        duration = Mathf.Max(0f, duration);

        if (Mathf.Approximately(targetVisualAlpha, alpha))
        {
            return;
        }

        targetVisualAlpha = alpha;

        if (visualFadeCoroutine != null)
        {
            StopCoroutine(visualFadeCoroutine);
            visualFadeCoroutine = null;
        }

        if (!isActiveAndEnabled || duration <= 0f)
        {
            visualAlpha = alpha;
            ApplyVisualAlpha();
            return;
        }

        visualFadeCoroutine = StartCoroutine(FadeVisuals(alpha, duration));
    }

    public void SetBackOcclusionAlpha(float alpha, float groundTilemapAlpha, float duration)
    {
        alpha = Mathf.Clamp01(alpha);
        groundTilemapAlpha = Mathf.Clamp01(groundTilemapAlpha);
        duration = Mathf.Max(0f, duration);

        if (Mathf.Approximately(targetBackOcclusionAlpha, alpha)
            && Mathf.Approximately(targetBackOcclusionGroundTilemapAlpha, groundTilemapAlpha))
        {
            return;
        }

        targetBackOcclusionAlpha = alpha;
        targetBackOcclusionGroundTilemapAlpha = groundTilemapAlpha;

        if (backOcclusionFadeCoroutine != null)
        {
            StopCoroutine(backOcclusionFadeCoroutine);
            backOcclusionFadeCoroutine = null;
        }

        if (!isActiveAndEnabled || duration <= 0f)
        {
            backOcclusionAlpha = alpha;
            backOcclusionGroundTilemapAlpha = groundTilemapAlpha;
            ApplyVisualAlpha();
            return;
        }

        backOcclusionFadeCoroutine = StartCoroutine(FadeBackOcclusion(alpha, groundTilemapAlpha, duration));
    }

    private void CacheVisualTargets()
    {
        initialTilemapColors = new Color[floorTilemaps != null ? floorTilemaps.Length : 0];
        for (int i = 0; i < initialTilemapColors.Length; i++)
        {
            initialTilemapColors[i] = floorTilemaps[i] != null ? floorTilemaps[i].color : Color.white;
        }

        initialSpriteColors = new Color[floorSpriteRenderers != null ? floorSpriteRenderers.Length : 0];
        for (int i = 0; i < initialSpriteColors.Length; i++)
        {
            initialSpriteColors[i] = floorSpriteRenderers[i] != null ? floorSpriteRenderers[i].color : Color.white;
        }
    }

    private IEnumerator FadeVisuals(float targetAlpha, float duration)
    {
        if (initialTilemapColors == null || initialSpriteColors == null)
        {
            CacheVisualTargets();
        }

        float startAlpha = visualAlpha;

        if (duration <= 0f)
        {
            visualAlpha = targetAlpha;
            ApplyVisualAlpha();
            visualFadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            visualAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            ApplyVisualAlpha();
            yield return null;
        }

        visualAlpha = targetAlpha;
        ApplyVisualAlpha();
        visualFadeCoroutine = null;
    }

    private IEnumerator FadeBackOcclusion(float targetAlpha, float targetGroundTilemapAlpha, float duration)
    {
        if (initialTilemapColors == null || initialSpriteColors == null)
        {
            CacheVisualTargets();
        }

        float startAlpha = backOcclusionAlpha;
        float startGroundTilemapAlpha = backOcclusionGroundTilemapAlpha;

        if (duration <= 0f)
        {
            backOcclusionAlpha = targetAlpha;
            backOcclusionGroundTilemapAlpha = targetGroundTilemapAlpha;
            ApplyVisualAlpha();
            backOcclusionFadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            backOcclusionAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            backOcclusionGroundTilemapAlpha = Mathf.Lerp(startGroundTilemapAlpha, targetGroundTilemapAlpha, t);
            ApplyVisualAlpha();
            yield return null;
        }

        backOcclusionAlpha = targetAlpha;
        backOcclusionGroundTilemapAlpha = targetGroundTilemapAlpha;
        ApplyVisualAlpha();
        backOcclusionFadeCoroutine = null;
    }

    private void ApplyVisualAlpha()
    {
        if (initialTilemapColors == null || initialSpriteColors == null)
        {
            CacheVisualTargets();
        }

        if (floorTilemaps != null)
        {
            for (int i = 0; i < floorTilemaps.Length; i++)
            {
                Tilemap tilemap = floorTilemaps[i];
                if (tilemap == null)
                {
                    continue;
                }

                Color color = initialTilemapColors[i];
                color.a *= visualAlpha * GetBackOcclusionAlphaForTilemap(tilemap);
                tilemap.color = color;
            }
        }

        if (floorSpriteRenderers != null)
        {
            for (int i = 0; i < floorSpriteRenderers.Length; i++)
            {
                SpriteRenderer spriteRenderer = floorSpriteRenderers[i];
                if (spriteRenderer == null)
                {
                    continue;
                }

                Color color = initialSpriteColors[i];
                color.a *= visualAlpha * backOcclusionAlpha;
                spriteRenderer.color = color;
            }
        }

        if (floorVisibilityGroups == null)
        {
            return;
        }

        for (int i = 0; i < floorVisibilityGroups.Length; i++)
        {
            VisibilityTargetGroup targetGroup = floorVisibilityGroups[i];
            if (targetGroup == null)
            {
                continue;
            }

            targetGroup.SetFloorVisibilityAlpha(visualAlpha, 0f);
            targetGroup.SetBackOcclusionAlpha(backOcclusionAlpha, 0f);
        }
    }

    private float GetBackOcclusionAlphaForTilemap(Tilemap tilemap)
    {
        return tilemap != null && tilemap.gameObject.name == "Ground"
            ? backOcclusionGroundTilemapAlpha
            : backOcclusionAlpha;
    }
}
