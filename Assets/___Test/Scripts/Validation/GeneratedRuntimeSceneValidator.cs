using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GeneratedRuntimeSceneValidator
{
    public static GeneratedBuildingValidationResult Validate(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingProfile profile
    )
    {
        GeneratedBuildingValidationResult result = new GeneratedBuildingValidationResult();

        if (sceneRoot == null)
        {
            result.AddError("GeneratedBuildingSceneRoot is missing.");
            return result;
        }

        if (sceneRoot.GeneratedHouseRoot == null)
        {
            result.AddError("Generated House Root is missing.");
            return result;
        }

        ValidateVisibility(sceneRoot, result);
        ValidateRuntimeIdentities(sceneRoot, result);
        ValidateCeilingPlacement(sceneRoot, profile, result);
        ValidateStairZones(sceneRoot, profile, result);
        ValidateTestUnitAgents(result);
        return result;
    }

    private static void ValidateCeilingPlacement(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingProfile profile,
        GeneratedBuildingValidationResult result
    )
    {
        if (profile == null || profile.TilePalette == null || sceneRoot.CeilingRoot == null)
        {
            return;
        }

        Transform ceilingTransform = sceneRoot.CeilingRoot.Find("Ceiling Grid/Visual Tilemap");
        Tilemap ceilingTilemap = ceilingTransform != null
            ? ceilingTransform.GetComponent<Tilemap>()
            : null;

        if (ceilingTilemap == null)
        {
            result.AddError("Ceiling Grid/Visual Tilemap is missing.");
            return;
        }

        ceilingTilemap.CompressBounds();
        BoundsInt actual = ceilingTilemap.cellBounds;
        RectInt buildingFootprint = new RectInt(profile.FootprintOrigin, profile.FootprintSize);
        RectInt expected = GeneratedCeilingRenderer.CalculateFootprint(
            buildingFootprint,
            profile.TilePalette
        );

        if (actual.xMin != expected.xMin || actual.yMin != expected.yMin ||
            actual.size.x != expected.width || actual.size.y != expected.height)
        {
            result.AddError(
                "Ceiling tile bounds do not match the palette alignment rule. " +
                $"actual=({actual.xMin},{actual.yMin},{actual.size.x},{actual.size.y}), " +
                $"expected=({expected.x},{expected.y},{expected.width},{expected.height}), " +
                $"offset={profile.TilePalette.CeilingCellOffset}"
            );
        }
    }

    private static void ValidateVisibility(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingValidationResult result
    )
    {
        GeneratedVisibilityController controller = sceneRoot.GeneratedHouseRoot != null
            ? sceneRoot.GeneratedHouseRoot.GetComponent<GeneratedVisibilityController>()
            : null;

        if (controller == null)
        {
            result.AddError("Generated House Root has no GeneratedVisibilityController.");
        }

        GeneratedVisibilityGroup[] groups = sceneRoot.GeneratedHouseRoot != null
            ? sceneRoot.GeneratedHouseRoot.GetComponentsInChildren<GeneratedVisibilityGroup>(true)
            : new GeneratedVisibilityGroup[0];

        if (groups.Length <= 0)
        {
            result.AddError("No GeneratedVisibilityGroup components found.");
            return;
        }

        for (int i = 0; i < groups.Length; i++)
        {
            GeneratedVisibilityGroup group = groups[i];

            if (group == null)
            {
                continue;
            }

            if (group.TargetCount <= 0)
            {
                result.AddWarning($"{group.name}: visibility group has no renderer targets.");
            }

            ValidateVisibilityGroupTargets(group, result);
        }

        ValidateVisibilityTargetUniqueness(groups, result);
        ValidateVisibilityGroupCoverage(sceneRoot, groups, result);
    }

    private static void ValidateVisibilityGroupTargets(
        GeneratedVisibilityGroup group,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < group.Targets.Count; i++)
        {
            TilemapRenderer renderer = group.Targets[i];

            if (renderer == null)
            {
                result.AddError($"{group.name}: visibility group contains a missing TilemapRenderer target.");
                continue;
            }

            Tilemap tilemap = renderer.GetComponent<Tilemap>();

            if (tilemap == null)
            {
                result.AddError($"{group.name}: visibility target {renderer.name} has no Tilemap component.");
            }

            switch (group.GroupKind)
            {
                case GeneratedVisibilityGroupKind.Floor:
                    if (renderer.name != "Upper Wall" && renderer.name != "Under Wall")
                    {
                        result.AddError($"{group.name}: floor visibility group should only target outer Upper/Under Wall tilemaps. target={renderer.name}");
                    }
                    break;
                case GeneratedVisibilityGroupKind.Room:
                    if (renderer.name != "Upper Wall" && renderer.name != "Under Wall")
                    {
                        result.AddError($"{group.name}: room visibility group should only target room Upper/Under Wall tilemaps. target={renderer.name}");
                    }
                    break;
                case GeneratedVisibilityGroupKind.Ceiling:
                    if (renderer.name != "Visual Tilemap")
                    {
                        result.AddWarning($"{group.name}: ceiling visibility target is expected to be named Visual Tilemap. target={renderer.name}");
                    }
                    break;
            }
        }
    }

    private static void ValidateVisibilityTargetUniqueness(
        GeneratedVisibilityGroup[] groups,
        GeneratedBuildingValidationResult result
    )
    {
        Dictionary<TilemapRenderer, GeneratedVisibilityGroup> ownerByRenderer =
            new Dictionary<TilemapRenderer, GeneratedVisibilityGroup>();

        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            GeneratedVisibilityGroup group = groups[groupIndex];

            if (group == null)
            {
                continue;
            }

            for (int targetIndex = 0; targetIndex < group.Targets.Count; targetIndex++)
            {
                TilemapRenderer renderer = group.Targets[targetIndex];

                if (renderer == null)
                {
                    continue;
                }

                if (ownerByRenderer.TryGetValue(renderer, out GeneratedVisibilityGroup existingOwner))
                {
                    result.AddError(
                        $"{renderer.name}: visibility target is controlled by multiple groups. " +
                        $"first={existingOwner.name}, second={group.name}"
                    );
                    continue;
                }

                ownerByRenderer.Add(renderer, group);
            }
        }
    }

    private static void ValidateVisibilityGroupCoverage(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedVisibilityGroup[] groups,
        GeneratedBuildingValidationResult result
    )
    {
        Dictionary<int, GeneratedVisibilityGroup> floorGroups = new Dictionary<int, GeneratedVisibilityGroup>();
        Dictionary<string, GeneratedVisibilityGroup> roomGroups = new Dictionary<string, GeneratedVisibilityGroup>();

        for (int i = 0; i < groups.Length; i++)
        {
            GeneratedVisibilityGroup group = groups[i];

            if (group == null)
            {
                continue;
            }

            if (group.GroupKind == GeneratedVisibilityGroupKind.Floor)
            {
                if (group.FloorIndex <= 0)
                {
                    result.AddError($"{group.name}: floor visibility group must have a positive floor index.");
                    continue;
                }

                floorGroups[group.FloorIndex] = group;
            }
            else if (group.GroupKind == GeneratedVisibilityGroupKind.Room)
            {
                if (group.FloorIndex <= 0 || group.RoomIndex <= 0)
                {
                    result.AddError($"{group.name}: room visibility group must have positive floor and room indexes.");
                    continue;
                }

                roomGroups[BuildRoomKey(group.FloorIndex, group.RoomIndex)] = group;
            }
        }

        GeneratedRuntimeIdentity[] identities = sceneRoot.GeneratedHouseRoot != null
            ? sceneRoot.GeneratedHouseRoot.GetComponentsInChildren<GeneratedRuntimeIdentity>(true)
            : new GeneratedRuntimeIdentity[0];

        for (int i = 0; i < identities.Length; i++)
        {
            GeneratedRuntimeIdentity identity = identities[i];

            if (identity == null)
            {
                continue;
            }

            if (identity.ObjectKind == GeneratedRuntimeObjectKind.Floor &&
                !floorGroups.ContainsKey(identity.FloorIndex))
            {
                result.AddError($"{identity.name}: floor has no matching floor visibility group. floor={identity.FloorIndex}");
            }

            if (identity.ObjectKind == GeneratedRuntimeObjectKind.Room &&
                !roomGroups.ContainsKey(BuildRoomKey(identity.FloorIndex, identity.RoomIndex)))
            {
                result.AddError($"{identity.name}: room has no matching room visibility group. floor={identity.FloorIndex}, room={identity.RoomIndex}");
            }
        }
    }

    private static void ValidateRuntimeIdentities(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingValidationResult result
    )
    {
        GeneratedRuntimeIdentity[] identities = sceneRoot.GeneratedHouseRoot != null
            ? sceneRoot.GeneratedHouseRoot.GetComponentsInChildren<GeneratedRuntimeIdentity>(true)
            : new GeneratedRuntimeIdentity[0];

        if (identities.Length <= 0)
        {
            result.AddError("No GeneratedRuntimeIdentity components found.");
            return;
        }

        for (int i = 0; i < identities.Length; i++)
        {
            GeneratedRuntimeIdentity identity = identities[i];

            if (identity == null)
            {
                continue;
            }

            ValidateRuntimeIdentity(identity, result);
        }
    }

    private static void ValidateRuntimeIdentity(
        GeneratedRuntimeIdentity identity,
        GeneratedBuildingValidationResult result
    )
    {
        if ((identity.ColliderObject || identity.TriggerObject) && identity.FloorIndex <= 0)
        {
            result.AddError($"{identity.name}: collider/trigger identity must have a positive floor index.");
        }

        if (IsRoomRuntimeObject(identity.ObjectKind) && identity.RoomIndex <= 0)
        {
            result.AddError($"{identity.name}: room runtime identity must have a positive room index.");
        }

        if (!identity.ColliderObject && !identity.TriggerObject)
        {
            return;
        }

        Collider2D collider = identity.GetComponent<Collider2D>();

        if (collider == null)
        {
            result.AddError($"{identity.name}: runtime collider/trigger identity has no Collider2D.");
            return;
        }

        if (collider.isTrigger != identity.TriggerObject)
        {
            result.AddError(
                $"{identity.name}: Collider2D trigger state does not match GeneratedRuntimeIdentity. " +
                $"collider={collider.isTrigger}, identity={identity.TriggerObject}"
            );
        }

        CompositeCollider2D composite = identity.GetComponent<CompositeCollider2D>();

        if (composite == null)
        {
            result.AddError($"{identity.name}: generated collider tilemap has no CompositeCollider2D.");
            return;
        }

        if (composite.geometryType != CompositeCollider2D.GeometryType.Polygons)
        {
            result.AddError($"{identity.name}: CompositeCollider2D Geometry Type must be Polygons.");
        }
    }

    private static void ValidateStairZones(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingProfile profile,
        GeneratedBuildingValidationResult result
    )
    {
        if (profile == null || !profile.GenerateStairs)
        {
            return;
        }

        if (sceneRoot.StairRoot == null)
        {
            result.AddError("Profile generates stairs, but Stair Root is missing.");
            return;
        }

        GeneratedStairZone[] zones = sceneRoot.StairRoot.GetComponentsInChildren<GeneratedStairZone>(true);
        bool hasWalkable = false;
        bool hasEntry = false;
        bool hasExit = false;
        bool hasLeftRail = false;
        bool hasRightRail = false;

        for (int i = 0; i < zones.Length; i++)
        {
            GeneratedStairZone zone = zones[i];

            if (zone == null)
            {
                continue;
            }

            switch (zone.ZoneKind)
            {
                case GeneratedStairZoneKind.Walkable:
                    hasWalkable = true;
                    ValidateStairZoneCollider(zone, true, result);
                    break;
                case GeneratedStairZoneKind.Entry:
                    hasEntry = true;
                    ValidateTransitionZone(zone, 1, 2, result);
                    ValidateStairZoneCollider(zone, true, result);
                    break;
                case GeneratedStairZoneKind.Exit:
                    hasExit = true;
                    ValidateTransitionZone(zone, 2, 1, result);
                    ValidateStairZoneCollider(zone, true, result);
                    break;
                case GeneratedStairZoneKind.LeftRail:
                    hasLeftRail = true;
                    ValidateStairZoneCollider(zone, false, result);
                    break;
                case GeneratedStairZoneKind.RightRail:
                    hasRightRail = true;
                    ValidateStairZoneCollider(zone, false, result);
                    break;
            }
        }

        if (!hasWalkable)
        {
            result.AddError("Generated stair has no Walkable zone.");
        }

        if (!hasEntry)
        {
            result.AddError("Generated stair has no Entry transition zone.");
        }

        if (!hasExit)
        {
            result.AddError("Generated stair has no Exit transition zone.");
        }

        if (!hasLeftRail || !hasRightRail)
        {
            result.AddError("Generated stair must have left and right rail collider zones.");
        }
    }

    private static void ValidateTransitionZone(
        GeneratedStairZone zone,
        int expectedSourceFloor,
        int expectedDestinationFloor,
        GeneratedBuildingValidationResult result
    )
    {
        if (zone.SourceFloorIndex != expectedSourceFloor ||
            zone.DestinationFloorIndex != expectedDestinationFloor)
        {
            result.AddError(
                $"{zone.name}: invalid stair transition metadata. " +
                $"source={zone.SourceFloorIndex}, destination={zone.DestinationFloorIndex}, " +
                $"expected={expectedSourceFloor}->{expectedDestinationFloor}"
            );
        }
    }

    private static void ValidateStairZoneCollider(
        GeneratedStairZone zone,
        bool expectedTrigger,
        GeneratedBuildingValidationResult result
    )
    {
        Collider2D collider = zone.GetComponent<Collider2D>();

        if (collider == null)
        {
            result.AddError($"{zone.name}: stair zone has no Collider2D.");
            return;
        }

        if (collider.isTrigger != expectedTrigger)
        {
            result.AddError(
                $"{zone.name}: stair zone trigger state is invalid. " +
                $"actual={collider.isTrigger}, expected={expectedTrigger}"
            );
        }

        CompositeCollider2D composite = zone.GetComponent<CompositeCollider2D>();

        if (composite != null && composite.geometryType != CompositeCollider2D.GeometryType.Polygons)
        {
            result.AddError($"{zone.name}: stair zone CompositeCollider2D Geometry Type must be Polygons.");
        }
    }

    private static bool IsRoomRuntimeObject(GeneratedRuntimeObjectKind objectKind)
    {
        return objectKind == GeneratedRuntimeObjectKind.Room ||
               objectKind == GeneratedRuntimeObjectKind.RoomGroundTrigger ||
               objectKind == GeneratedRuntimeObjectKind.RoomUpperWallCollider ||
               objectKind == GeneratedRuntimeObjectKind.RoomUnderWallCollider;
    }

    private static void ValidateTestUnitAgents(GeneratedBuildingValidationResult result)
    {
        GeneratedTestUnitController unit = Object.FindObjectOfType<GeneratedTestUnitController>();

        if (unit == null)
        {
            result.AddWarning("No GeneratedTestUnitController found. Runtime Unit flow checks require Create Or Reset Test Unit.");
            return;
        }

        if (unit.GetComponent<GeneratedFloorIsolationAgent>() == null)
        {
            result.AddError($"{unit.name}: missing GeneratedFloorIsolationAgent.");
        }

        if (unit.GetComponent<GeneratedVisibilityAgent>() == null)
        {
            result.AddError($"{unit.name}: missing GeneratedVisibilityAgent.");
        }

        if (unit.GetComponent<GeneratedStairTransitionAgent>() == null)
        {
            result.AddError($"{unit.name}: missing GeneratedStairTransitionAgent.");
        }
    }

    private static string BuildRoomKey(int floorIndex, int roomIndex)
    {
        return $"{floorIndex}:{roomIndex}";
    }
}
