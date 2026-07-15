using UnityEngine;

public static class GeneratedRuntimeBinder
{
    public static void Bind(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile
    )
    {
        if (sceneRoot == null || buildingData == null || profile == null)
        {
            return;
        }

        Transform ownershipRoot = sceneRoot.GeneratedHouseRoot;

        if (ownershipRoot == null)
        {
            return;
        }

        GeneratedBuildingRuntimeRegistry registry =
            ownershipRoot.GetComponent<GeneratedBuildingRuntimeRegistry>();

        if (registry == null)
        {
            registry = ownershipRoot.gameObject.AddComponent<GeneratedBuildingRuntimeRegistry>();
        }

        string buildingInstanceId = sceneRoot.EnsureBuildingInstanceId();
        int bindingRevision = registry.BeginRebuild(
            buildingInstanceId,
            buildingData.BuildingId,
            profile.BuildingId
        );

        BindIdentity(
            ownershipRoot,
            buildingData,
            profile,
            buildingData.BuildingId,
            buildingData.BuildingId,
            GeneratedRuntimeObjectKind.Building,
            0,
            0,
            buildingData.Footprint,
            false,
            false,
            buildingInstanceId,
            bindingRevision,
            registry
        );

        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floorData = buildingData.Floors[i];

            if (floorData == null)
            {
                continue;
            }

            Transform floorRoot = ResolveFloorRoot(sceneRoot, floorData.FloorIndex);
            BindFloor(
                floorRoot,
                floorData,
                buildingData,
                profile,
                buildingInstanceId,
                bindingRevision,
                registry
            );
        }

        BindIdentity(
            sceneRoot.CeilingRoot,
            buildingData,
            profile,
            "ceiling",
            buildingData.BuildingId,
            GeneratedRuntimeObjectKind.Ceiling,
            0,
            0,
            buildingData.Footprint,
            false,
            false,
            buildingInstanceId,
            bindingRevision,
            registry
        );

        BindIdentity(
            sceneRoot.StairRoot,
            buildingData,
            profile,
            "stair_root",
            buildingData.BuildingId,
            GeneratedRuntimeObjectKind.Stair,
            0,
            0,
            buildingData.Footprint,
            false,
            false,
            buildingInstanceId,
            bindingRevision,
            registry
        );

        BindStairColliderIdentities(
            sceneRoot.StairRoot,
            buildingData,
            profile,
            buildingInstanceId,
            bindingRevision,
            registry
        );

        registry.CompleteRebuild(ownershipRoot);
    }

    private static void BindFloor(
        Transform floorRoot,
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        string buildingInstanceId,
        int bindingRevision,
        GeneratedBuildingRuntimeRegistry registry
    )
    {
        if (floorRoot == null || floorData == null)
        {
            return;
        }

        BindIdentity(
            floorRoot,
            buildingData,
            profile,
            floorData.FloorId,
            buildingData.BuildingId,
            GeneratedRuntimeObjectKind.Floor,
            floorData.FloorIndex,
            0,
            floorData.Bounds,
            false,
            false,
            buildingInstanceId,
            bindingRevision,
            registry
        );

        BindFloorColliderIdentities(
            floorRoot, floorData, buildingData, profile,
            buildingInstanceId, bindingRevision, registry
        );
        BindRoomIdentities(
            floorRoot, floorData, buildingData, profile,
            buildingInstanceId, bindingRevision, registry
        );
    }

    private static void BindFloorColliderIdentities(
        Transform floorRoot,
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        string buildingInstanceId,
        int bindingRevision,
        GeneratedBuildingRuntimeRegistry registry
    )
    {
        Transform floorColliderRoot = floorRoot.Find($"Floor {floorData.FloorIndex:00} Collider Tilemaps/Floor Collider Tilemap");

        BindIdentity(
            floorColliderRoot != null ? floorColliderRoot.Find("Ground Trigger") : null,
            buildingData,
            profile,
            $"{floorData.FloorId}_ground_trigger",
            floorData.FloorId,
            GeneratedRuntimeObjectKind.FloorGroundTrigger,
            floorData.FloorIndex,
            0,
            floorData.Bounds,
            true,
            true,
            buildingInstanceId,
            bindingRevision,
            registry
        );

        Transform wallRoot = floorColliderRoot != null ? floorColliderRoot.Find("Wall Colliders") : null;
        BindIdentity(
            wallRoot != null ? wallRoot.Find("Upper Wall") : null,
            buildingData,
            profile,
            $"{floorData.FloorId}_outer_upper_wall_collider",
            floorData.FloorId,
            GeneratedRuntimeObjectKind.FloorUpperWallCollider,
            floorData.FloorIndex,
            0,
            floorData.Bounds,
            true,
            false,
            buildingInstanceId,
            bindingRevision,
            registry
        );
        BindIdentity(
            wallRoot != null ? wallRoot.Find("Under Wall") : null,
            buildingData,
            profile,
            $"{floorData.FloorId}_outer_under_wall_collider",
            floorData.FloorId,
            GeneratedRuntimeObjectKind.FloorUnderWallCollider,
            floorData.FloorIndex,
            0,
            floorData.Bounds,
            true,
            false,
            buildingInstanceId,
            bindingRevision,
            registry
        );
    }

    private static void BindRoomIdentities(
        Transform floorRoot,
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        string buildingInstanceId,
        int bindingRevision,
        GeneratedBuildingRuntimeRegistry registry
    )
    {
        Transform visualRoot = floorRoot.Find($"Floor {floorData.FloorIndex:00} Room Tilemaps");
        Transform colliderRoot = floorRoot.Find($"Floor {floorData.FloorIndex:00} Collider Tilemaps");

        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData roomData = floorData.Rooms[i];

            if (roomData == null)
            {
                continue;
            }

            int roomIndex = i + 1;
            Transform roomVisualRoot = visualRoot != null ? visualRoot.Find($"Room {roomIndex:00} Visual Tilemap") : null;
            Transform roomColliderRoot = colliderRoot != null ? colliderRoot.Find($"Room {roomIndex:00} Collider Tilemap") : null;

            BindIdentity(
                roomVisualRoot,
                buildingData,
                profile,
                roomData.RoomId,
                floorData.FloorId,
                GeneratedRuntimeObjectKind.Room,
                floorData.FloorIndex,
                roomIndex,
                roomData.Bounds,
                false,
                false,
                buildingInstanceId,
                bindingRevision,
                registry
            );

            BindIdentity(
                roomColliderRoot != null ? roomColliderRoot.Find("Ground Trigger") : null,
                buildingData,
                profile,
                $"{roomData.RoomId}_ground_trigger",
                roomData.RoomId,
                GeneratedRuntimeObjectKind.RoomGroundTrigger,
                floorData.FloorIndex,
                roomIndex,
                roomData.Bounds,
                true,
                true,
                buildingInstanceId,
                bindingRevision,
                registry
            );

            BindIdentity(
                roomColliderRoot != null ? roomColliderRoot.Find("Upper Wall") : null,
                buildingData,
                profile,
                $"{roomData.RoomId}_upper_wall_collider",
                roomData.RoomId,
                GeneratedRuntimeObjectKind.RoomUpperWallCollider,
                floorData.FloorIndex,
                roomIndex,
                roomData.Bounds,
                true,
                false,
                buildingInstanceId,
                bindingRevision,
                registry
            );

            BindIdentity(
                roomColliderRoot != null ? roomColliderRoot.Find("Under Wall") : null,
                buildingData,
                profile,
                $"{roomData.RoomId}_under_wall_collider",
                roomData.RoomId,
                GeneratedRuntimeObjectKind.RoomUnderWallCollider,
                floorData.FloorIndex,
                roomIndex,
                roomData.Bounds,
                true,
                false,
                buildingInstanceId,
                bindingRevision,
                registry
            );
        }
    }

    private static void BindStairColliderIdentities(
        Transform stairRoot,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        string buildingInstanceId,
        int bindingRevision,
        GeneratedBuildingRuntimeRegistry registry
    )
    {
        if (stairRoot == null)
        {
            return;
        }

        GeneratedStairZone[] zones = stairRoot.GetComponentsInChildren<GeneratedStairZone>(true);

        for (int i = 0; i < zones.Length; i++)
        {
            GeneratedStairZone zone = zones[i];

            if (zone == null)
            {
                continue;
            }

            Collider2D collider = zone.GetComponent<Collider2D>();
            string stairId = string.IsNullOrWhiteSpace(zone.StairId) ? "stair" : zone.StairId;
            int owningFloorIndex = zone.SourceFloorIndex > 0
                ? zone.SourceFloorIndex
                : zone.LowerFloorIndex;

            BindIdentity(
                zone.transform,
                buildingData,
                profile,
                $"{stairId}_{zone.ZoneKind}",
                stairId,
                GeneratedRuntimeObjectKind.Stair,
                owningFloorIndex,
                0,
                buildingData.Footprint,
                collider != null,
                collider != null && collider.isTrigger,
                buildingInstanceId,
                bindingRevision,
                registry
            );
        }
    }

    private static Transform ResolveFloorRoot(GeneratedBuildingSceneRoot sceneRoot, int floorIndex)
    {
        return sceneRoot != null ? sceneRoot.GetFloorRoot(floorIndex) : null;
    }

    private static void BindIdentity(
        Transform target,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        string objectId,
        string ownerId,
        GeneratedRuntimeObjectKind objectKind,
        int floorIndex,
        int roomIndex,
        RectInt bounds,
        bool colliderObject,
        bool triggerObject,
        string buildingInstanceId,
        int bindingRevision,
        GeneratedBuildingRuntimeRegistry registry
    )
    {
        if (target == null)
        {
            return;
        }

        GeneratedRuntimeIdentity identity = target.GetComponent<GeneratedRuntimeIdentity>();

        if (identity == null)
        {
            identity = target.gameObject.AddComponent<GeneratedRuntimeIdentity>();
        }

        identity.Configure(
            buildingInstanceId,
            buildingData.BuildingId,
            profile.BuildingId,
            objectId,
            ownerId,
            objectKind,
            floorIndex,
            roomIndex,
            bounds,
            colliderObject,
            triggerObject,
            bindingRevision,
            registry
        );
    }
}
