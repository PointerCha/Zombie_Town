using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class VisibilityTargetGroup : MonoBehaviour
{
    [SerializeField] private Tilemap[] tilemaps;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private float revealedAlpha = 0.35f;
    [SerializeField] private float transitionDuration = 0.5f;

    private Color[] initialTilemapColors;
    private Color[] initialSpriteColors;
    private Coroutine revealFadeCoroutine;
    private Coroutine floorFadeCoroutine;
    private Coroutine backOcclusionFadeCoroutine;
    private int revealRequestCount;
    private float revealAlphaMultiplier = 1f;
    private float floorAlphaMultiplier = 1f;
    private float backOcclusionAlphaMultiplier = 1f;

    public float RevealedAlpha => revealedAlpha;
    public float TransitionDuration => transitionDuration;

    private void Awake()
    {
        CacheTargets();
    }

    private void OnEnable()
    {
        ApplyAlpha();
    }

    private void OnDisable()
    {
        revealFadeCoroutine = null;
        floorFadeCoroutine = null;
        backOcclusionFadeCoroutine = null;
    }

    private void OnValidate()
    {
        revealedAlpha = Mathf.Clamp01(revealedAlpha);
        transitionDuration = Mathf.Max(0f, transitionDuration);
    }

    public void RequestReveal()
    {
        revealRequestCount++;

        if (revealRequestCount == 1)
        {
            StartRevealFade(revealedAlpha, transitionDuration);
        }
    }

    public void ReleaseReveal()
    {
        if (revealRequestCount == 0)
        {
            return;
        }

        revealRequestCount--;

        if (revealRequestCount == 0)
        {
            StartRevealFade(1f, transitionDuration);
        }
    }

    public void SetFloorVisibilityAlpha(float alpha, float duration)
    {
        StartFloorFade(Mathf.Clamp01(alpha), Mathf.Max(0f, duration));
    }

    public void SetBackOcclusionAlpha(float alpha, float duration)
    {
        StartBackOcclusionFade(Mathf.Clamp01(alpha), Mathf.Max(0f, duration));
    }

    private void CacheTargets()
    {
        initialTilemapColors = new Color[tilemaps != null ? tilemaps.Length : 0];
        for (int i = 0; i < initialTilemapColors.Length; i++)
        {
            initialTilemapColors[i] = tilemaps[i] != null ? tilemaps[i].color : Color.white;
        }

        initialSpriteColors = new Color[spriteRenderers != null ? spriteRenderers.Length : 0];
        for (int i = 0; i < initialSpriteColors.Length; i++)
        {
            initialSpriteColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
        }
    }

    private void StartRevealFade(float targetRevealAlphaMultiplier, float duration)
    {
        if (revealFadeCoroutine != null)
        {
            StopCoroutine(revealFadeCoroutine);
            revealFadeCoroutine = null;
        }

        if (!isActiveAndEnabled || duration <= 0f)
        {
            revealAlphaMultiplier = targetRevealAlphaMultiplier;
            ApplyAlpha();
            return;
        }

        revealFadeCoroutine = StartCoroutine(FadeRevealRoutine(targetRevealAlphaMultiplier, duration));
    }

    private void StartFloorFade(float targetFloorAlphaMultiplier, float duration)
    {
        if (floorFadeCoroutine != null)
        {
            StopCoroutine(floorFadeCoroutine);
            floorFadeCoroutine = null;
        }

        if (!isActiveAndEnabled || duration <= 0f)
        {
            floorAlphaMultiplier = targetFloorAlphaMultiplier;
            ApplyAlpha();
            return;
        }

        floorFadeCoroutine = StartCoroutine(FadeFloorRoutine(targetFloorAlphaMultiplier, duration));
    }

    private void StartBackOcclusionFade(float targetBackOcclusionAlphaMultiplier, float duration)
    {
        if (backOcclusionFadeCoroutine != null)
        {
            StopCoroutine(backOcclusionFadeCoroutine);
            backOcclusionFadeCoroutine = null;
        }

        if (!isActiveAndEnabled || duration <= 0f)
        {
            backOcclusionAlphaMultiplier = targetBackOcclusionAlphaMultiplier;
            ApplyAlpha();
            return;
        }

        backOcclusionFadeCoroutine = StartCoroutine(FadeBackOcclusionRoutine(targetBackOcclusionAlphaMultiplier, duration));
    }

    private IEnumerator FadeRevealRoutine(float targetRevealAlphaMultiplier, float duration)
    {
        if (initialTilemapColors == null || initialSpriteColors == null)
        {
            CacheTargets();
        }

        if (duration <= 0f)
        {
            revealAlphaMultiplier = targetRevealAlphaMultiplier;
            ApplyAlpha();
            revealFadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        float startRevealAlphaMultiplier = revealAlphaMultiplier;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            revealAlphaMultiplier = Mathf.Lerp(startRevealAlphaMultiplier, targetRevealAlphaMultiplier, t);
            ApplyAlpha();
            yield return null;
        }

        revealAlphaMultiplier = targetRevealAlphaMultiplier;
        ApplyAlpha();
        revealFadeCoroutine = null;
    }

    private IEnumerator FadeFloorRoutine(float targetFloorAlphaMultiplier, float duration)
    {
        if (initialTilemapColors == null || initialSpriteColors == null)
        {
            CacheTargets();
        }

        if (duration <= 0f)
        {
            floorAlphaMultiplier = targetFloorAlphaMultiplier;
            ApplyAlpha();
            floorFadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        float startFloorAlphaMultiplier = floorAlphaMultiplier;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            floorAlphaMultiplier = Mathf.Lerp(startFloorAlphaMultiplier, targetFloorAlphaMultiplier, t);
            ApplyAlpha();
            yield return null;
        }

        floorAlphaMultiplier = targetFloorAlphaMultiplier;
        ApplyAlpha();
        floorFadeCoroutine = null;
    }

    private IEnumerator FadeBackOcclusionRoutine(float targetBackOcclusionAlphaMultiplier, float duration)
    {
        if (initialTilemapColors == null || initialSpriteColors == null)
        {
            CacheTargets();
        }

        if (duration <= 0f)
        {
            backOcclusionAlphaMultiplier = targetBackOcclusionAlphaMultiplier;
            ApplyAlpha();
            backOcclusionFadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        float startBackOcclusionAlphaMultiplier = backOcclusionAlphaMultiplier;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            backOcclusionAlphaMultiplier = Mathf.Lerp(startBackOcclusionAlphaMultiplier, targetBackOcclusionAlphaMultiplier, t);
            ApplyAlpha();
            yield return null;
        }

        backOcclusionAlphaMultiplier = targetBackOcclusionAlphaMultiplier;
        ApplyAlpha();
        backOcclusionFadeCoroutine = null;
    }

    private void ApplyAlpha()
    {
        if (initialTilemapColors == null || initialSpriteColors == null)
        {
            CacheTargets();
        }

        float targetAlphaMultiplier = revealAlphaMultiplier * floorAlphaMultiplier * backOcclusionAlphaMultiplier;

        for (int i = 0; i < initialTilemapColors.Length; i++)
        {
            Tilemap tilemap = tilemaps[i];
            if (tilemap == null)
            {
                continue;
            }

            Color baseColor = initialTilemapColors[i];
            float targetAlpha = baseColor.a * targetAlphaMultiplier;

            baseColor.a = targetAlpha;
            tilemap.color = baseColor;
        }

        for (int i = 0; i < initialSpriteColors.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            if (spriteRenderer == null)
            {
                continue;
            }

            Color baseColor = initialSpriteColors[i];
            float targetAlpha = baseColor.a * targetAlphaMultiplier;

            baseColor.a = targetAlpha;
            spriteRenderer.color = baseColor;
        }
    }
}
