using UnityEngine;
using UnityEngine.Tilemaps;

public static class GeneratedVisibilityBinder
{
    public static void Bind(GeneratedBuildingSceneRoot sceneRoot, GeneratedBuildingData buildingData)
    {
        if (sceneRoot == null || buildingData == null)
        {
            return;
        }

        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floorData = buildingData.Floors[i];

            if (floorData != null)
            {
                BindFloor(sceneRoot.GetFloorRoot(floorData.FloorIndex), floorData);
            }
        }

        BindCeiling(sceneRoot.CeilingRoot);
        BindStair(sceneRoot.StairRoot);
        BindController(sceneRoot.GeneratedHouseRoot);
    }

    private static void BindFloor(Transform floorRoot, GeneratedFloorData floorData)
    {
        if (floorRoot == null || floorData == null)
        {
            return;
        }

        GeneratedVisibilityGroup floorGroup = EnsureGroup(
            floorRoot,
            $"{floorData.FloorId}_outer_visibility",
            GeneratedVisibilityGroupKind.Floor,
            floorData.FloorIndex,
            0
        );

        AddRenderer(floorGroup, floorRoot.Find($"Floor {floorData.FloorIndex:00} Visual Tilemaps/Upper Wall"));
        AddRenderer(floorGroup, floorRoot.Find($"Floor {floorData.FloorIndex:00} Visual Tilemaps/Under Wall"));

        Transform roomRoot = floorRoot.Find($"Floor {floorData.FloorIndex:00} Room Tilemaps");

        if (roomRoot == null)
        {
            return;
        }

        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData roomData = floorData.Rooms[i];

            if (roomData == null)
            {
                continue;
            }

            Transform roomGroupTransform = roomRoot.Find($"Room {i + 1:00} Visual Tilemap");

            if (roomGroupTransform == null)
            {
                continue;
            }

            GeneratedVisibilityGroup roomGroup = EnsureGroup(
                roomGroupTransform,
                $"{roomData.RoomId}_visibility",
                GeneratedVisibilityGroupKind.Room,
                floorData.FloorIndex,
                i + 1
            );

            AddRenderer(roomGroup, roomGroupTransform.Find("Upper Wall"));
            AddRenderer(roomGroup, roomGroupTransform.Find("Under Wall"));
        }
    }

    private static void BindCeiling(Transform ceilingRoot)
    {
        if (ceilingRoot == null)
        {
            return;
        }

        GeneratedVisibilityGroup ceilingGroup = EnsureGroup(
            ceilingRoot,
            "ceiling_visibility",
            GeneratedVisibilityGroupKind.Ceiling,
            0,
            0
        );

        AddRenderer(ceilingGroup, ceilingRoot.Find("Ceiling Grid/Visual Tilemap"));
    }

    private static void BindStair(Transform stairRoot)
    {
        if (stairRoot == null)
        {
            return;
        }

        GeneratedVisibilityGroup stairGroup = EnsureGroup(
            stairRoot,
            "stair_visibility",
            GeneratedVisibilityGroupKind.Stair,
            0,
            0
        );

        AddRenderer(stairGroup, stairRoot.Find("Stair Visual Tilemaps/Visual Tilemap"));
    }

    private static void BindController(Transform generatedHouseRoot)
    {
        if (generatedHouseRoot == null)
        {
            return;
        }

        GeneratedVisibilityController controller = generatedHouseRoot.GetComponent<GeneratedVisibilityController>();

        if (controller == null)
        {
            controller = generatedHouseRoot.gameObject.AddComponent<GeneratedVisibilityController>();
        }

        controller.RebuildCache();
    }

    private static GeneratedVisibilityGroup EnsureGroup(
        Transform target,
        string groupId,
        GeneratedVisibilityGroupKind groupKind,
        int floorIndex,
        int roomIndex
    )
    {
        GeneratedVisibilityGroup group = target.GetComponent<GeneratedVisibilityGroup>();

        if (group == null)
        {
            group = target.gameObject.AddComponent<GeneratedVisibilityGroup>();
        }

        group.Configure(groupId, groupKind, floorIndex, roomIndex);
        return group;
    }

    private static void AddRenderer(GeneratedVisibilityGroup group, Transform target)
    {
        if (group == null || target == null)
        {
            return;
        }

        group.AddTarget(target.GetComponent<TilemapRenderer>());
    }
}
