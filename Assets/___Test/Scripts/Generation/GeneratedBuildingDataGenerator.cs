using UnityEngine;

public static class GeneratedBuildingDataGenerator
{
    public static GeneratedBuildingData GenerateGround(
        GeneratedBuildingProfile profile,
        int seed
    )
    {
        if (profile == null)
        {
            return null;
        }

        GeneratedBuildingData buildingData = new GeneratedBuildingData();
        buildingData.Initialize(
            profile.BuildingId,
            profile.DisplayName,
            seed,
            profile.FootprintOrigin,
            profile.FootprintSize
        );

        for (int i = 0; i < profile.FloorCount; i++)
        {
            int floorIndex = i + 1;
            GeneratedFloorData floorData = new GeneratedFloorData();
            floorData.Initialize(
                $"floor_{floorIndex:00}",
                floorIndex,
                profile.FootprintOrigin,
                profile.FootprintSize
            );

            buildingData.AddFloor(floorData);
        }

        return buildingData;
    }

    public static GeneratedBuildingData GenerateFull(
        GeneratedBuildingProfile profile,
        int seed
    )
    {
        GeneratedBuildingData buildingData = GenerateGround(profile, seed);

        if (buildingData == null)
        {
            return null;
        }

        AddOuterWalls(buildingData);
        GeneratedStairPlanner.AddStairs(buildingData, profile);
        GeneratedEntrancePlanner.AddMainEntrance(buildingData, profile);
        GeneratedRoomPlanner.AddRooms(buildingData, profile);
        return buildingData;
    }

    public static void AddOuterWalls(GeneratedBuildingData buildingData)
    {
        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floorData = buildingData.Floors[i];

            if (floorData == null)
            {
                continue;
            }

            AddOuterWall(
                floorData,
                GeneratedWallLayer.Upper,
                GeneratedWallSide.West,
                floorData.Bounds.xMin + 1,
                floorData.Bounds.xMax - 1,
                floorData.Bounds.yMax - 1,
                true
            );
            AddOuterWall(
                floorData,
                GeneratedWallLayer.Upper,
                GeneratedWallSide.North,
                floorData.Bounds.yMin + 1,
                floorData.Bounds.yMax - 1,
                floorData.Bounds.xMax - 1,
                false
            );
            AddOuterWall(
                floorData,
                GeneratedWallLayer.Under,
                GeneratedWallSide.East,
                floorData.Bounds.xMin + 1,
                floorData.Bounds.xMax,
                floorData.Bounds.yMin,
                true
            );
            AddOuterWall(
                floorData,
                GeneratedWallLayer.Under,
                GeneratedWallSide.South,
                floorData.Bounds.yMin + 1,
                floorData.Bounds.yMax - 1,
                floorData.Bounds.xMin,
                false
            );

            AddOuterCorner(
                floorData,
                GeneratedWallLayer.Upper,
                GeneratedWallSide.North,
                new Vector2Int(floorData.Bounds.xMax - 1, floorData.Bounds.yMax - 1)
            );
            AddOuterCorner(
                floorData,
                GeneratedWallLayer.Upper,
                GeneratedWallSide.West,
                new Vector2Int(floorData.Bounds.xMin, floorData.Bounds.yMax - 1)
            );

            AddOuterCorner(
                floorData,
                GeneratedWallLayer.Upper,
                GeneratedWallSide.East,
                new Vector2Int(floorData.Bounds.xMax - 1, floorData.Bounds.yMin)
            );

            AddOuterCorner(
                floorData,
                GeneratedWallLayer.Under,
                GeneratedWallSide.South,
                new Vector2Int(floorData.Bounds.xMin, floorData.Bounds.yMin)
            );
        }
    }

    private static void AddOuterWall(
        GeneratedFloorData floorData,
        GeneratedWallLayer layer,
        GeneratedWallSide side,
        int lineStart,
        int lineEnd,
        int fixedAxis,
        bool horizontal
    )
    {
        GeneratedWallData wallData = new GeneratedWallData();
        wallData.Initialize(
            $"{floorData.FloorId}_outer_{layer.ToString().ToLowerInvariant()}_{side.ToString().ToLowerInvariant()}",
            GeneratedWallOwner.OuterBuilding,
            floorData.FloorId,
            layer,
            side
        );

        for (int value = lineStart; value < lineEnd; value++)
        {
            Vector2Int cell = horizontal
                ? new Vector2Int(value, fixedAxis)
                : new Vector2Int(fixedAxis, value);

            wallData.AddCell(cell);
        }

        floorData.AddOuterWall(wallData);
    }

    private static void AddOuterCorner(
        GeneratedFloorData floorData,
        GeneratedWallLayer layer,
        GeneratedWallSide side,
        Vector2Int cell
    )
    {
        GeneratedWallData wallData = new GeneratedWallData();
        wallData.Initialize(
            $"{floorData.FloorId}_outer_{layer.ToString().ToLowerInvariant()}_corner_{side.ToString().ToLowerInvariant()}",
            GeneratedWallOwner.OuterBuilding,
            floorData.FloorId,
            layer,
            side
        );
        wallData.AddCell(cell);
        floorData.AddOuterWall(wallData);
    }
}
