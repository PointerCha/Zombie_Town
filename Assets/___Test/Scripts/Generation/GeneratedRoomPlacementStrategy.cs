using System.Collections.Generic;
using UnityEngine;

internal interface IGeneratedRoomPlacementStrategy
{
    bool TryPlace(
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        RectInt availableBounds,
        int roomCount,
        int roomIndex,
        List<RectInt> placedRoomBounds,
        GeneratedBuildingProfile profile,
        out RectInt roomBounds
    );
}

internal sealed class GeneratedRandomPackedRoomPlacementStrategy : IGeneratedRoomPlacementStrategy
{
    public bool TryPlace(
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        RectInt availableBounds,
        int roomCount,
        int roomIndex,
        List<RectInt> placedRoomBounds,
        GeneratedBuildingProfile profile,
        out RectInt roomBounds
    )
    {
        return GeneratedRoomPlanner.TryBuildRandomPackedRoom(
            floorData,
            buildingData,
            availableBounds,
            placedRoomBounds,
            profile,
            out roomBounds
        );
    }
}

internal sealed class GeneratedSectorRoomPlacementStrategy : IGeneratedRoomPlacementStrategy
{
    public bool TryPlace(
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        RectInt availableBounds,
        int roomCount,
        int roomIndex,
        List<RectInt> placedRoomBounds,
        GeneratedBuildingProfile profile,
        out RectInt roomBounds
    )
    {
        return GeneratedRoomPlanner.TryBuildSectorRoom(
            floorData,
            buildingData,
            availableBounds,
            roomCount,
            roomIndex,
            placedRoomBounds,
            profile,
            out roomBounds
        );
    }
}
