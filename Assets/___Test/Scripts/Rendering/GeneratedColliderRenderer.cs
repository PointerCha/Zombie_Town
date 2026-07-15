using UnityEngine;
using UnityEngine.Tilemaps;

public static class GeneratedColliderRenderer
{
    public static void PaintBuildingColliders(
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
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

            PaintFloorColliders(
                sceneRoot.EnsureFloorRoot(floorData.FloorIndex),
                floorData,
                profile,
                clearBeforeGenerate
            );
        }
    }

    private static void PaintFloorColliders(
        Transform floorRoot,
        GeneratedFloorData floorData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        if (floorRoot == null || floorData == null)
        {
            return;
        }

        Transform floorColliderRoot = FindOrCreateFloorColliderRoot(floorRoot, floorData.FloorIndex);
        PaintFloorGroundTrigger(floorColliderRoot, floorData, profile, clearBeforeGenerate);
        PaintFloorWallColliders(floorColliderRoot, floorData, profile, clearBeforeGenerate);
        PaintRoomColliders(floorRoot, floorData, profile, clearBeforeGenerate);
    }

    private static Transform FindOrCreateFloorColliderRoot(Transform floorRoot, int floorIndex)
    {
        Transform colliderRoot = floorRoot.Find($"Floor {floorIndex:00} Collider Tilemaps");

        if (colliderRoot == null)
        {
            colliderRoot = GeneratedTilemapUtility.EnsureChild(floorRoot, $"Floor {floorIndex:00} Collider Tilemaps");
        }

        Transform floorColliderGroup = colliderRoot.Find("Floor Collider Tilemap");

        if (floorColliderGroup != null)
        {
            return floorColliderGroup;
        }

        return GeneratedTilemapUtility.EnsureChild(colliderRoot, "Floor Collider Tilemap");
    }

    private static void PaintFloorGroundTrigger(
        Transform floorColliderRoot,
        GeneratedFloorData floorData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        Transform groundTarget = GeneratedTilemapUtility.EnsureChild(floorColliderRoot, "Ground Trigger");
        Tilemap groundTilemap = GeneratedTilemapUtility.EnsureColliderTilemap(
            groundTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            true,
            true
        );

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(groundTilemap);
        }

        GeneratedTilemapUtility.PaintCells(groundTilemap, floorData.GroundMask, profile.TilePalette.CollisionTile);
    }

    private static void PaintFloorWallColliders(
        Transform floorColliderRoot,
        GeneratedFloorData floorData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        Transform wallColliderRoot = GeneratedTilemapUtility.EnsureChild(floorColliderRoot, "Wall Colliders");
        Transform upperTarget = GeneratedTilemapUtility.EnsureChild(wallColliderRoot, "Upper Wall");
        Transform underTarget = GeneratedTilemapUtility.EnsureChild(wallColliderRoot, "Under Wall");

        Tilemap upperTilemap = GeneratedTilemapUtility.EnsureColliderTilemap(
            upperTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            false,
            true
        );

        Tilemap underTilemap = GeneratedTilemapUtility.EnsureColliderTilemap(
            underTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            false,
            true
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

            GeneratedTilemapUtility.PaintWallCollision(targetTilemap, wallData, profile.TilePalette.CollisionTile);
        }
    }

    private static void PaintRoomColliders(
        Transform floorRoot,
        GeneratedFloorData floorData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        Transform colliderRoot = floorRoot.Find(GeneratedBuildingSceneRoot.GetFloorColliderRootName(floorData.FloorIndex));

        if (colliderRoot == null)
        {
            colliderRoot = GeneratedTilemapUtility.EnsureChild(
                floorRoot,
                GeneratedBuildingSceneRoot.GetFloorColliderRootName(floorData.FloorIndex)
            );
        }

        if (clearBeforeGenerate)
        {
            ClearGeneratedRoomColliderRoots(colliderRoot);
        }

        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData roomData = floorData.Rooms[i];

            if (roomData == null)
            {
                continue;
            }

            Transform roomColliderRoot = GeneratedTilemapUtility.EnsureChild(
                colliderRoot,
                $"Room {i + 1:00} Collider Tilemap"
            );

            PaintRoomCollider(roomColliderRoot, roomData, profile, clearBeforeGenerate);
        }
    }

    private static void ClearGeneratedRoomColliderRoots(Transform colliderRoot)
    {
        for (int i = colliderRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = colliderRoot.GetChild(i);

            if (child == null || !child.name.StartsWith("Room ") || !child.name.Contains(" Collider Tilemap"))
            {
                continue;
            }

            GeneratedTilemapUtility.DestroyGeneratedChild(child);
        }
    }

    private static void PaintRoomCollider(
        Transform roomColliderRoot,
        GeneratedRoomData roomData,
        GeneratedBuildingProfile profile,
        bool clearBeforeGenerate
    )
    {
        Transform groundTarget = GeneratedTilemapUtility.EnsureChild(roomColliderRoot, "Ground Trigger");
        Transform upperTarget = GeneratedTilemapUtility.EnsureChild(roomColliderRoot, "Upper Wall");
        Transform underTarget = GeneratedTilemapUtility.EnsureChild(roomColliderRoot, "Under Wall");

        Tilemap groundTilemap = GeneratedTilemapUtility.EnsureColliderTilemap(
            groundTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            true,
            true
        );

        Tilemap upperTilemap = GeneratedTilemapUtility.EnsureColliderTilemap(
            upperTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            false,
            true
        );

        Tilemap underTilemap = GeneratedTilemapUtility.EnsureColliderTilemap(
            underTarget,
            profile.GridProfile.VisualCellSize,
            profile.GridProfile.CellLayout,
            false,
            true
        );

        if (clearBeforeGenerate)
        {
            GeneratedTilemapUtility.Clear(groundTilemap);
            GeneratedTilemapUtility.Clear(upperTilemap);
            GeneratedTilemapUtility.Clear(underTilemap);
        }

        GeneratedTilemapUtility.PaintCells(groundTilemap, roomData.FloorMask, profile.TilePalette.CollisionTile);

        for (int i = 0; i < roomData.Walls.Count; i++)
        {
            GeneratedWallData wallData = roomData.Walls[i];

            if (wallData == null)
            {
                continue;
            }

            Tilemap targetTilemap = wallData.Layer == GeneratedWallLayer.Upper
                ? upperTilemap
                : underTilemap;

            GeneratedTilemapUtility.PaintWallCollision(targetTilemap, wallData, profile.TilePalette.CollisionTile);
        }
    }
}
