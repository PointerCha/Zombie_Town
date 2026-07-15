using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GeneratedStairRenderer
{
    public static void PaintStairs(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        if (sceneRoot == null || buildingData == null || profile == null || sceneRoot.StairRoot == null)
        {
            return;
        }

        List<GeneratedStairData> lowerFloorStairs = CollectLowerFloorStairs(buildingData);

        if (clearBeforeGenerate && lowerFloorStairs.Count == 0)
        {
            RemoveGeneratedStairRoot(sceneRoot.StairRoot, "Stair Visual Tilemaps");
            RemoveGeneratedStairRoot(sceneRoot.StairRoot, "Stair Collider And Trigger Tilemaps");
            return;
        }

        Transform stairVisualRoot = GeneratedTilemapUtility.EnsureChild(sceneRoot.StairRoot, "Stair Visual Tilemaps");
        Transform visualTarget = GeneratedTilemapUtility.EnsureChild(stairVisualRoot, "Visual Tilemap");

        Tilemap visualTilemap = GeneratedTilemapUtility.EnsureTilemap(
            visualTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.StairVisualOrder
        );

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(visualTilemap);
        }

        PaintStairColliderAndTriggerTilemaps(sceneRoot.StairRoot, lowerFloorStairs, profile, clearBeforeGenerate);

        for (int i = 0; i < lowerFloorStairs.Count; i++)
        {
            GeneratedStairData stairData = lowerFloorStairs[i];

            if (stairData == null)
            {
                continue;
            }

            GeneratedTilemapUtility.PaintCells(
                visualTilemap,
                stairData.VisualMask,
                profile.TilePalette.StairVisualTile
            );
        }
    }

    private static void RemoveGeneratedStairRoot(Transform stairRoot, string childName)
    {
        Transform child = stairRoot.Find(childName);

        if (child != null)
        {
            GeneratedTilemapUtility.DestroyGeneratedChild(child);
        }
    }

    private static void PaintStairColliderAndTriggerTilemaps(
        Transform stairRoot,
        IReadOnlyList<GeneratedStairData> stairs,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        Transform colliderRoot = GeneratedTilemapUtility.EnsureChild(
            stairRoot,
            "Stair Collider And Trigger Tilemaps"
        );

        Transform walkableTarget = GeneratedTilemapUtility.EnsureChild(
            colliderRoot,
            "Walkable Trigger Area Tilemap"
        );
        Transform leftRailTarget = GeneratedTilemapUtility.EnsureChild(
            colliderRoot,
            "Left Rail Collider Tilemap"
        );
        Transform rightRailTarget = GeneratedTilemapUtility.EnsureChild(
            colliderRoot,
            "Right Rail Collider Tilemap"
        );
        Transform entryTarget = GeneratedTilemapUtility.EnsureChild(
            colliderRoot,
            "Floor Entry Trigger Tilemap"
        );
        Transform exitTarget = GeneratedTilemapUtility.EnsureChild(
            colliderRoot,
            "Floor Exit Trigger Tilemap"
        );

        ConfigureStairUtilityTransform(walkableTarget, 0f);
        ConfigureStairUtilityTransform(leftRailTarget, 30f);
        ConfigureStairUtilityTransform(rightRailTarget, 30f);
        ConfigureStairUtilityTransform(entryTarget, 0f);
        ConfigureStairUtilityTransform(exitTarget, 0f);

        Tilemap walkableTilemap = EnsureStairColliderTilemap(walkableTarget, profile, true);
        Tilemap leftRailTilemap = EnsureStairColliderTilemap(leftRailTarget, profile, false);
        Tilemap rightRailTilemap = EnsureStairColliderTilemap(rightRailTarget, profile, false);
        Tilemap entryTilemap = EnsureStairColliderTilemap(entryTarget, profile, true);
        Tilemap exitTilemap = EnsureStairColliderTilemap(exitTarget, profile, true);

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(walkableTilemap);
            GeneratedTilemapUtility.Clear(leftRailTilemap);
            GeneratedTilemapUtility.Clear(rightRailTilemap);
            GeneratedTilemapUtility.Clear(entryTilemap);
            GeneratedTilemapUtility.Clear(exitTilemap);
        }

        for (int i = 0; i < stairs.Count; i++)
        {
            GeneratedStairData stairData = stairs[i];

            if (stairData == null)
            {
                continue;
            }

            ConfigureStairZone(walkableTarget, stairData, GeneratedStairZoneKind.Walkable);
            ConfigureStairZone(leftRailTarget, stairData, GeneratedStairZoneKind.LeftRail);
            ConfigureStairZone(rightRailTarget, stairData, GeneratedStairZoneKind.RightRail);
            ConfigureStairZone(entryTarget, stairData, GeneratedStairZoneKind.Entry);
            ConfigureStairZone(exitTarget, stairData, GeneratedStairZoneKind.Exit);

            GeneratedTilemapUtility.PaintCells(
                walkableTilemap,
                stairData.WalkableMask,
                profile.TilePalette.CollisionTile
            );
            GeneratedTilemapUtility.PaintCells(
                leftRailTilemap,
                stairData.LeftRailMask,
                profile.TilePalette.CollisionTile
            );
            GeneratedTilemapUtility.PaintCells(
                rightRailTilemap,
                stairData.RightRailMask,
                profile.TilePalette.CollisionTile
            );
            GeneratedTilemapUtility.PaintCells(
                entryTilemap,
                stairData.EntryTriggerMask,
                profile.TilePalette.CollisionTile
            );
            GeneratedTilemapUtility.PaintCells(
                exitTilemap,
                stairData.ExitTriggerMask,
                profile.TilePalette.CollisionTile
            );
        }
    }

    private static List<GeneratedStairData> CollectLowerFloorStairs(GeneratedBuildingData buildingData)
    {
        List<GeneratedStairData> stairs = new List<GeneratedStairData>();

        for (int floorIndex = 0; floorIndex < buildingData.Floors.Count; floorIndex++)
        {
            GeneratedFloorData floorData = buildingData.Floors[floorIndex];

            if (floorData == null)
            {
                continue;
            }

            for (int stairIndex = 0; stairIndex < floorData.Stairs.Count; stairIndex++)
            {
                GeneratedStairData stairData = floorData.Stairs[stairIndex];

                if (stairData == null ||
                    stairData.LowerFloorIndex != floorData.FloorIndex ||
                    stairs.Contains(stairData))
                {
                    continue;
                }

                stairs.Add(stairData);
            }
        }

        return stairs;
    }

    private static void ConfigureStairZone(
        Transform target,
        GeneratedStairData stairData,
        GeneratedStairZoneKind zoneKind
    )
    {
        if (target == null)
        {
            return;
        }

        GeneratedStairZone zone = target.GetComponent<GeneratedStairZone>();

        if (zone == null)
        {
            zone = target.gameObject.AddComponent<GeneratedStairZone>();
        }

        zone.Configure(stairData, zoneKind);
    }

    private static void ConfigureStairUtilityTransform(Transform target, float zRotation)
    {
        if (target == null)
        {
            return;
        }

        target.localPosition = Vector3.zero;
        target.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        target.localScale = Vector3.one;
    }

    private static Tilemap EnsureStairColliderTilemap(
        Transform target,
        GeneratedBuildingProfile profile,
        bool isTrigger
    )
    {
        return GeneratedTilemapUtility.EnsureColliderTilemap(
            target,
            profile.GridProfile.StairColliderCellSize,
            profile.GridProfile.CellLayout,
            isTrigger,
            true
        );
    }
}
