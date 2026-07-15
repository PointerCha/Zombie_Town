using UnityEngine;
using UnityEngine.Tilemaps;

public static class GeneratedBuildingSceneRenderer
{
    public static void RenderFloors(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate,
        bool paintOuterWalls,
        bool paintRooms
    )
    {
        if (sceneRoot == null || buildingData == null || profile == null)
        {
            return;
        }

        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floorData = buildingData.Floors[i];

            if (floorData == null)
            {
                continue;
            }

            int zeroBasedFloorIndex = Mathf.Max(0, floorData.FloorIndex - 1);
            Transform floorRoot = sceneRoot.EnsureFloorRoot(floorData.FloorIndex);

            PaintFloorGround(
                floorData,
                sceneRoot.EnsureFloorVisualTilemap(floorData.FloorIndex, "Ground"),
                profile,
                zeroBasedFloorIndex,
                clearBeforeGenerate
            );

            if (paintOuterWalls)
            {
                PaintFloorOuterWalls(
                    floorData,
                    sceneRoot.EnsureFloorVisualTilemap(floorData.FloorIndex, "Upper Wall"),
                    sceneRoot.EnsureFloorVisualTilemap(floorData.FloorIndex, "Under Wall"),
                    profile,
                    zeroBasedFloorIndex,
                    clearBeforeGenerate
                );
            }

            if (paintRooms)
            {
                PaintFloorRooms(
                    floorData,
                    floorRoot,
                    profile,
                    zeroBasedFloorIndex,
                    clearBeforeGenerate
                );
            }
        }
    }

    public static void RenderCompleteBuilding(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        RenderFloors(sceneRoot, buildingData, profile, clearBeforeGenerate, true, true);
        GeneratedStairRenderer.PaintStairs(sceneRoot, buildingData, profile, clearBeforeGenerate);
        GeneratedColliderRenderer.PaintBuildingColliders(sceneRoot, buildingData, profile, clearBeforeGenerate);
        GeneratedCeilingRenderer.PaintCeiling(sceneRoot, buildingData, profile, clearBeforeGenerate);
        GeneratedVisibilityBinder.Bind(sceneRoot, buildingData);
        GeneratedRuntimeBinder.Bind(sceneRoot, buildingData, profile);
    }

    private static void PaintFloorGround(
        GeneratedFloorData floorData,
        Transform target,
        GeneratedBuildingProfile profile,
        int zeroBasedFloorIndex,
        bool clearBeforeGenerate
    )
    {
        if (floorData == null || target == null)
        {
            return;
        }

        Tilemap tilemap = GeneratedTilemapUtility.EnsureTilemap(
            target,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.GetGroundOrder(zeroBasedFloorIndex)
        );

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(tilemap);
        }

        GeneratedTilemapUtility.PaintCells(tilemap, floorData.GroundMask, profile.TilePalette.GroundTile);
    }

    private static void PaintFloorOuterWalls(
        GeneratedFloorData floorData,
        Transform upperTarget,
        Transform underTarget,
        GeneratedBuildingProfile profile,
        int zeroBasedFloorIndex,
        bool clearBeforeGenerate
    )
    {
        if (floorData == null)
        {
            return;
        }

        Tilemap upperTilemap = GeneratedTilemapUtility.EnsureTilemap(
            upperTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.GetOuterUpperWallOrder(zeroBasedFloorIndex)
        );
        Tilemap underTilemap = GeneratedTilemapUtility.EnsureTilemap(
            underTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.OuterUnderWallOrder
        );

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(upperTilemap);
            GeneratedTilemapUtility.Clear(underTilemap);
        }

        for (int i = 0; i < floorData.OuterWalls.Count; i++)
        {
            GeneratedWallData wallData = floorData.OuterWalls[i];

            if (wallData == null)
            {
                continue;
            }

            Tilemap targetTilemap = wallData.Layer == GeneratedWallLayer.Upper
                ? upperTilemap
                : underTilemap;
            DirectionalTileSet tileSet = IsCornerWall(wallData)
                ? profile.TilePalette.OuterWallSecondary
                : profile.TilePalette.OuterWallPrimary;

            GeneratedTilemapUtility.PaintWall(targetTilemap, wallData, tileSet);
        }
    }

    private static void PaintFloorRooms(
        GeneratedFloorData floorData,
        Transform floorRoot,
        GeneratedBuildingProfile profile,
        int zeroBasedFloorIndex,
        bool clearBeforeGenerate
    )
    {
        if (floorData == null || floorRoot == null)
        {
            return;
        }

        string roomRootName = $"Floor {floorData.FloorIndex:00} Room Tilemaps";
        Transform roomRoot = floorRoot.Find(roomRootName);

        if (roomRoot == null)
        {
            roomRoot = GeneratedTilemapUtility.EnsureChild(floorRoot, roomRootName);
        }

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.ClearTilemapsInChildren(roomRoot);
            RemoveUnusedRoomGroups(roomRoot, floorData.Rooms.Count);
        }

        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData roomData = floorData.Rooms[i];

            if (roomData == null)
            {
                continue;
            }

            Transform roomGroup = GeneratedTilemapUtility.EnsureChild(
                roomRoot,
                $"Room {i + 1:00} Visual Tilemap"
            );
            PaintRoom(
                roomData,
                roomGroup,
                profile,
                zeroBasedFloorIndex,
                i,
                clearBeforeGenerate
            );
        }
    }

    private static void RemoveUnusedRoomGroups(Transform roomRoot, int activeRoomCount)
    {
        for (int i = roomRoot.childCount - 1; i >= activeRoomCount; i--)
        {
            Transform child = roomRoot.GetChild(i);

            if (child != null && child.name.StartsWith("Room ") && child.name.EndsWith(" Visual Tilemap"))
            {
                GeneratedTilemapUtility.DestroyGeneratedChild(child);
            }
        }
    }

    private static void PaintRoom(
        GeneratedRoomData roomData,
        Transform roomGroup,
        GeneratedBuildingProfile profile,
        int zeroBasedFloorIndex,
        int zeroBasedRoomIndex,
        bool clearBeforeGenerate
    )
    {
        Transform groundTarget = GeneratedTilemapUtility.EnsureChild(roomGroup, "Ground");
        Transform upperWallTarget = GeneratedTilemapUtility.EnsureChild(roomGroup, "Upper Wall");
        Transform underWallTarget = GeneratedTilemapUtility.EnsureChild(roomGroup, "Under Wall");

        Tilemap groundTilemap = GeneratedTilemapUtility.EnsureTilemap(
            groundTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.GetRoomGroundOrder(zeroBasedFloorIndex, zeroBasedRoomIndex)
        );
        Tilemap upperWallTilemap = GeneratedTilemapUtility.EnsureTilemap(
            upperWallTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.GetRoomUpperWallOrder(zeroBasedFloorIndex, zeroBasedRoomIndex)
        );
        Tilemap underWallTilemap = GeneratedTilemapUtility.EnsureTilemap(
            underWallTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            profile.SortingProfile.GetRoomUnderWallOrder(zeroBasedFloorIndex, zeroBasedRoomIndex)
        );

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(groundTilemap);
            GeneratedTilemapUtility.Clear(upperWallTilemap);
            GeneratedTilemapUtility.Clear(underWallTilemap);
        }

        GeneratedTilemapUtility.PaintCells(groundTilemap, roomData.FloorMask, profile.TilePalette.GroundTile);

        for (int i = 0; i < roomData.Walls.Count; i++)
        {
            GeneratedWallData wallData = roomData.Walls[i];

            if (wallData == null)
            {
                continue;
            }

            Tilemap targetTilemap = wallData.Layer == GeneratedWallLayer.Upper
                ? upperWallTilemap
                : underWallTilemap;
            GeneratedTilemapUtility.PaintWall(
                targetTilemap,
                wallData,
                IsCornerWall(wallData)
                    ? profile.TilePalette.RoomWallSecondary
                    : profile.TilePalette.RoomWallPrimary
            );
        }

        for (int i = 0; i < roomData.Doors.Count; i++)
        {
            GeneratedDoorData doorData = roomData.Doors[i];

            if (doorData == null)
            {
                continue;
            }

            Tilemap targetTilemap = IsUnderWallSide(doorData.WallSide)
                ? underWallTilemap
                : upperWallTilemap;
            GeneratedTilemapUtility.PaintDoor(targetTilemap, doorData, profile.TilePalette.DoorTiles);
        }
    }

    private static bool IsUnderWallSide(GeneratedWallSide side)
    {
        return side == GeneratedWallSide.East || side == GeneratedWallSide.South;
    }

    private static bool IsCornerWall(GeneratedWallData wallData)
    {
        return wallData != null &&
               wallData.WallId != null &&
               wallData.WallId.Contains("_corner_");
    }
}
