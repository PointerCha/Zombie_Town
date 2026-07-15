using UnityEngine;
using UnityEngine.Tilemaps;

public static class GeneratedCeilingRenderer
{
    public static Vector3 CalculateLocalPosition(int floorCount, Vector3 floorVisualOffsetStep)
    {
        return floorVisualOffsetStep * Mathf.Max(0, floorCount - 1);
    }

    public static Vector3 CalculateLocalPosition(GeneratedBuildingProfile profile)
    {
        return profile != null
            ? CalculateLocalPosition(profile.FloorCount, profile.FloorVisualOffsetStep)
            : Vector3.zero;
    }

    public static RectInt CalculateFootprint(
        RectInt buildingFootprint,
        GeneratedBuildingTilePalette tilePalette
    )
    {
        Vector2Int offset = tilePalette != null
            ? tilePalette.CeilingCellOffset
            : Vector2Int.zero;

        return new RectInt(buildingFootprint.position + offset, buildingFootprint.size);
    }

    public static void PaintCeiling(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        if (sceneRoot == null || buildingData == null || profile == null || sceneRoot.CeilingRoot == null)
        {
            return;
        }

        sceneRoot.CeilingRoot.localPosition = CalculateLocalPosition(profile);

        Transform ceilingGrid = GeneratedTilemapUtility.EnsureChild(sceneRoot.CeilingRoot, "Ceiling Grid");
        Transform visualTarget = GeneratedTilemapUtility.EnsureChild(ceilingGrid, "Visual Tilemap");

        Tilemap tilemap = GeneratedTilemapUtility.EnsureTilemap(
            visualTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.CeilingOrder
        );

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(tilemap);
        }

        RectInt ceilingFootprint = CalculateFootprint(
            buildingData.Footprint,
            profile.TilePalette
        );

        GeneratedCellMask ceilingMask = new GeneratedCellMask();
        ceilingMask.Initialize("ceiling", ceilingFootprint.position, ceilingFootprint.size);
        ceilingMask.FillRect(ceilingFootprint);

        GeneratedTilemapUtility.PaintCells(tilemap, ceilingMask, profile.TilePalette.CeilingTile);
    }
}
