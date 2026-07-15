using System.Collections.Generic;
using UnityEngine;

public static class GeneratedBuildingValidator
{
    public static GeneratedBuildingValidationResult Validate(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile
    )
    {
        GeneratedBuildingValidationResult result = new GeneratedBuildingValidationResult();

        ValidateProfile(profile, result);
        ValidateBuildingData(buildingData, result);

        if (buildingData == null)
        {
            return result;
        }

        ValidateFloors(buildingData, profile, result);
        ValidateEntrances(buildingData, result);
        ValidateStairs(buildingData, profile, result);
        ValidateConnectivity(buildingData, result);
        GeneratedBuildingGameplayValidator.Validate(buildingData, profile, result);
        return result;
    }

    private static void ValidateProfile(
        GeneratedBuildingProfile profile,
        GeneratedBuildingValidationResult result
    )
    {
        if (profile == null)
        {
            result.AddError("GeneratedBuildingProfile is missing.");
            return;
        }

        if (!profile.IsValid(out string profileError))
        {
            result.AddError($"GeneratedBuildingProfile is invalid. {profileError}");
        }
    }

    private static void ValidateBuildingData(
        GeneratedBuildingData buildingData,
        GeneratedBuildingValidationResult result
    )
    {
        if (buildingData == null)
        {
            result.AddError("GeneratedBuildingData is missing.");
            return;
        }

        if (buildingData.FootprintSize.x <= 0 || buildingData.FootprintSize.y <= 0)
        {
            result.AddError($"Building footprint size must be positive. size={buildingData.FootprintSize}");
        }

        if (buildingData.Floors.Count <= 0)
        {
            result.AddError("Building has no floors.");
        }

        if (buildingData.Entrances.Count <= 0)
        {
            result.AddWarning("Building has no entrance data.");
        }
    }

    private static void ValidateEntrances(
        GeneratedBuildingData buildingData,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < buildingData.Entrances.Count; i++)
        {
            GeneratedEntranceData entrance = buildingData.Entrances[i];

            if (entrance == null)
            {
                result.AddError($"Entrance at index {i} is null.");
                continue;
            }

            GeneratedFloorData floorData = buildingData.GetFloor(entrance.FloorIndex);

            if (floorData == null)
            {
                result.AddError($"{entrance.EntranceId}: entrance floor does not exist. floor={entrance.FloorIndex}");
                continue;
            }

            if (entrance.Cells.Count <= 0)
            {
                result.AddError($"{entrance.EntranceId}: entrance has no cells.");
                continue;
            }

            for (int cellIndex = 0; cellIndex < entrance.Cells.Count; cellIndex++)
            {
                Vector2Int entranceCell = entrance.Cells[cellIndex].Position;

                if (!floorData.Bounds.Contains(entranceCell))
                {
                    result.AddError($"{entrance.EntranceId}: entrance cell is outside floor bounds. cell={entranceCell}");
                }

                if (IsBlockedByWalls(floorData.OuterWalls, entranceCell))
                {
                    result.AddError($"{entrance.EntranceId}: entrance cell is still blocked by outer wall data. cell={entranceCell}");
                }
            }
        }
    }

    private static void ValidateFloors(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        GeneratedBuildingValidationResult result
    )
    {
        HashSet<int> floorIndexes = new HashSet<int>();

        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floorData = buildingData.Floors[i];

            if (floorData == null)
            {
                result.AddError($"Floor at list index {i} is null.");
                continue;
            }

            if (!floorIndexes.Add(floorData.FloorIndex))
            {
                result.AddError($"Duplicate floor index detected. floor={floorData.FloorIndex}");
            }

            ValidateFloor(buildingData, profile, floorData, result);
        }
    }

    private static void ValidateFloor(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        GeneratedFloorData floorData,
        GeneratedBuildingValidationResult result
    )
    {
        if (!ContainsRect(buildingData.Footprint, floorData.Bounds))
        {
            result.AddError($"{floorData.FloorId}: floor bounds are outside building footprint. bounds={floorData.Bounds}");
        }

        if (floorData.GroundMask.CellCount <= 0)
        {
            result.AddError($"{floorData.FloorId}: ground mask is empty.");
        }

        ValidateRoomCount(profile, floorData, result);
        ValidateRooms(floorData, result);
    }

    private static void ValidateRoomCount(
        GeneratedBuildingProfile profile,
        GeneratedFloorData floorData,
        GeneratedBuildingValidationResult result
    )
    {
        if (profile == null)
        {
            return;
        }

        if (floorData.Rooms.Count < profile.MinRoomsPerFloor)
        {
            result.AddError(
                $"{floorData.FloorId}: generated room count is below profile minimum. " +
                $"rooms={floorData.Rooms.Count}, min={profile.MinRoomsPerFloor}"
            );
        }

        if (floorData.Rooms.Count > profile.MaxRoomsPerFloor)
        {
            result.AddError(
                $"{floorData.FloorId}: generated room count exceeds profile maximum. " +
                $"rooms={floorData.Rooms.Count}, max={profile.MaxRoomsPerFloor}"
            );
        }
    }

    private static void ValidateRooms(
        GeneratedFloorData floorData,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData room = floorData.Rooms[i];

            if (room == null)
            {
                result.AddError($"{floorData.FloorId}: room at index {i} is null.");
                continue;
            }

            ValidateRoom(floorData, room, result);

            for (int otherIndex = i + 1; otherIndex < floorData.Rooms.Count; otherIndex++)
            {
                GeneratedRoomData otherRoom = floorData.Rooms[otherIndex];

                if (otherRoom != null && room.Bounds.Overlaps(otherRoom.Bounds))
                {
                    result.AddError(
                        $"{floorData.FloorId}: rooms overlap. roomA={room.RoomId} boundsA={room.Bounds}, " +
                        $"roomB={otherRoom.RoomId} boundsB={otherRoom.Bounds}"
                    );
                }
            }
        }
    }

    private static void ValidateRoom(
        GeneratedFloorData floorData,
        GeneratedRoomData room,
        GeneratedBuildingValidationResult result
    )
    {
        if (!ContainsRect(floorData.Bounds, room.Bounds))
        {
            result.AddError($"{room.RoomId}: room bounds are outside floor bounds. bounds={room.Bounds}");
        }

        if (room.FloorIndex != floorData.FloorIndex)
        {
            result.AddError(
                $"{room.RoomId}: room floor index does not match owner floor. room={room.FloorIndex}, floor={floorData.FloorIndex}"
            );
        }

        if (room.FloorMask.CellCount <= 0)
        {
            result.AddError($"{room.RoomId}: room floor mask is empty.");
        }

        if (room.Doors.Count <= 0)
        {
            result.AddWarning($"{room.RoomId}: room has no doors.");
        }

        ValidateRoomDoors(room, result);
    }

    private static void ValidateRoomDoors(
        GeneratedRoomData room,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < room.Doors.Count; i++)
        {
            GeneratedDoorData door = room.Doors[i];

            if (door == null)
            {
                result.AddError($"{room.RoomId}: door at index {i} is null.");
                continue;
            }

            if (door.FloorIndex != room.FloorIndex)
            {
                result.AddError(
                    $"{door.DoorId}: door floor index does not match room. door={door.FloorIndex}, room={room.FloorIndex}"
                );
            }

            if (door.OwnerId != room.RoomId)
            {
                result.AddError($"{door.DoorId}: door owner id does not match room id. owner={door.OwnerId}, room={room.RoomId}");
            }

            if (door.Cells.Count <= 0)
            {
                result.AddError($"{door.DoorId}: door has no cells.");
                continue;
            }

            for (int cellIndex = 0; cellIndex < door.Cells.Count; cellIndex++)
            {
                Vector2Int doorCell = door.Cells[cellIndex].Position;

                if (IsDoorBlockedByRoomWall(room, doorCell))
                {
                    result.AddError($"{door.DoorId}: door cell is still blocked by room wall data. cell={doorCell}");
                }
            }
        }
    }

    private static bool IsDoorBlockedByRoomWall(GeneratedRoomData room, Vector2Int doorCell)
    {
        for (int wallIndex = 0; wallIndex < room.Walls.Count; wallIndex++)
        {
            GeneratedWallData wall = room.Walls[wallIndex];

            if (wall == null)
            {
                continue;
            }

            for (int cellIndex = 0; cellIndex < wall.Cells.Count; cellIndex++)
            {
                if (wall.Cells[cellIndex].Position == doorCell)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsBlockedByWalls(IReadOnlyList<GeneratedWallData> walls, Vector2Int cell)
    {
        for (int wallIndex = 0; wallIndex < walls.Count; wallIndex++)
        {
            GeneratedWallData wall = walls[wallIndex];

            if (wall == null)
            {
                continue;
            }

            for (int cellIndex = 0; cellIndex < wall.Cells.Count; cellIndex++)
            {
                if (wall.Cells[cellIndex].Position == cell)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void ValidateStairs(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        GeneratedBuildingValidationResult result
    )
    {
        if (profile == null || !profile.GenerateStairs)
        {
            return;
        }

        if (profile.FloorCount < 2)
        {
            result.AddWarning("GenerateStairs is enabled, but profile has fewer than 2 floors.");
            return;
        }

        for (int floorIndex = 1; floorIndex < profile.FloorCount; floorIndex++)
        {
            GeneratedFloorData lowerFloor = buildingData.GetFloor(floorIndex);
            GeneratedFloorData upperFloor = buildingData.GetFloor(floorIndex + 1);

            if (lowerFloor == null || upperFloor == null)
            {
                result.AddError($"Stair validation requires Floor {floorIndex} and Floor {floorIndex + 1}.");
                continue;
            }

            bool lowerHasConnection = HasStairConnection(lowerFloor, floorIndex, floorIndex + 1);
            bool upperHasConnection = HasStairConnection(upperFloor, floorIndex, floorIndex + 1);

            if (!lowerHasConnection)
            {
                result.AddError($"Floor {floorIndex} has no stair data to Floor {floorIndex + 1}.");
            }

            if (!upperHasConnection)
            {
                result.AddError($"Floor {floorIndex + 1} has no stair data from Floor {floorIndex}.");
            }

            for (int i = 0; i < lowerFloor.Stairs.Count; i++)
            {
                GeneratedStairData stair = lowerFloor.Stairs[i];

                if (stair != null &&
                    stair.LowerFloorIndex == floorIndex &&
                    stair.UpperFloorIndex == floorIndex + 1)
                {
                    ValidateStair(stair, buildingData, result);
                }
            }
        }
    }

    private static bool HasStairConnection(GeneratedFloorData floorData, int lowerFloorIndex, int upperFloorIndex)
    {
        for (int i = 0; i < floorData.Stairs.Count; i++)
        {
            GeneratedStairData stair = floorData.Stairs[i];

            if (stair != null &&
                stair.LowerFloorIndex == lowerFloorIndex &&
                stair.UpperFloorIndex == upperFloorIndex)
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateStair(
        GeneratedStairData stair,
        GeneratedBuildingData buildingData,
        GeneratedBuildingValidationResult result
    )
    {
        if (stair == null)
        {
            result.AddError("Stair data is null.");
            return;
        }

        if (buildingData.GetFloor(stair.LowerFloorIndex) == null)
        {
            result.AddError($"{stair.StairId}: lower floor does not exist. floor={stair.LowerFloorIndex}");
        }

        if (buildingData.GetFloor(stair.UpperFloorIndex) == null)
        {
            result.AddError($"{stair.StairId}: upper floor does not exist. floor={stair.UpperFloorIndex}");
        }

        ValidateMaskNotEmpty(stair.StairId, "visual", stair.VisualMask, result);
        ValidateMaskNotEmpty(stair.StairId, "walkable", stair.WalkableMask, result);
        ValidateMaskNotEmpty(stair.StairId, "entry", stair.EntryTriggerMask, result);
        ValidateMaskNotEmpty(stair.StairId, "exit", stair.ExitTriggerMask, result);
        ValidateMaskNotEmpty(stair.StairId, "left rail", stair.LeftRailMask, result);
        ValidateMaskNotEmpty(stair.StairId, "right rail", stair.RightRailMask, result);
    }

    private static void ValidateConnectivity(
        GeneratedBuildingData buildingData,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floorData = buildingData.Floors[i];

            if (floorData == null)
            {
                continue;
            }

            ValidateFloorConnectivity(buildingData, floorData, result);
        }

        ValidateStairConnectivity(buildingData, result);
    }

    private static void ValidateFloorConnectivity(
        GeneratedBuildingData buildingData,
        GeneratedFloorData floorData,
        GeneratedBuildingValidationResult result
    )
    {
        HashSet<Vector2Int> walkableCells = BuildWalkableCells(floorData);

        if (walkableCells.Count == 0)
        {
            result.AddError($"{floorData.FloorId}: no walkable cells found for connectivity validation.");
            return;
        }

        Vector2Int startCell;

        if (!TryFindConnectivityStartCell(buildingData, floorData, walkableCells, out startCell))
        {
            result.AddError($"{floorData.FloorId}: no public floor cell found outside rooms.");
            return;
        }

        HashSet<Vector2Int> reachableCells = FloodFill(startCell, walkableCells);
        ValidateEntranceConnectivity(buildingData, floorData, reachableCells, result);

        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData room = floorData.Rooms[i];

            if (room == null)
            {
                continue;
            }

            ValidateRoomConnectivity(floorData, room, reachableCells, result);
        }

        ValidateStairAccessOnFloor(floorData, reachableCells, result);
    }

    private static bool TryFindConnectivityStartCell(
        GeneratedBuildingData buildingData,
        GeneratedFloorData floorData,
        HashSet<Vector2Int> walkableCells,
        out Vector2Int startCell
    )
    {
        for (int entranceIndex = 0; entranceIndex < buildingData.Entrances.Count; entranceIndex++)
        {
            GeneratedEntranceData entrance = buildingData.Entrances[entranceIndex];

            if (entrance == null || entrance.FloorIndex != floorData.FloorIndex)
            {
                continue;
            }

            for (int cellIndex = 0; cellIndex < entrance.Cells.Count; cellIndex++)
            {
                Vector2Int entranceCell = entrance.Cells[cellIndex].Position;

                if (walkableCells.Contains(entranceCell))
                {
                    startCell = entranceCell;
                    return true;
                }
            }
        }

        return TryFindPublicStartCell(floorData, walkableCells, out startCell);
    }

    private static void ValidateEntranceConnectivity(
        GeneratedBuildingData buildingData,
        GeneratedFloorData floorData,
        HashSet<Vector2Int> reachableCells,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < buildingData.Entrances.Count; i++)
        {
            GeneratedEntranceData entrance = buildingData.Entrances[i];

            if (entrance == null || entrance.FloorIndex != floorData.FloorIndex)
            {
                continue;
            }

            if (!IsAnyEntranceCellReachable(entrance, reachableCells))
            {
                result.AddError($"{entrance.EntranceId}: no entrance cell is reachable from public floor area. cell={entrance.Cell}");
            }
        }
    }

    private static bool IsAnyEntranceCellReachable(
        GeneratedEntranceData entrance,
        HashSet<Vector2Int> reachableCells
    )
    {
        for (int i = 0; i < entrance.Cells.Count; i++)
        {
            if (reachableCells.Contains(entrance.Cells[i].Position))
            {
                return true;
            }
        }

        return false;
    }

    private static HashSet<Vector2Int> BuildWalkableCells(GeneratedFloorData floorData)
    {
        HashSet<Vector2Int> walkableCells = new HashSet<Vector2Int>();

        for (int i = 0; i < floorData.GroundMask.Cells.Count; i++)
        {
            walkableCells.Add(floorData.GroundMask.Cells[i].Position);
        }

        RemoveWallCells(walkableCells, floorData.OuterWalls);

        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData room = floorData.Rooms[i];

            if (room != null)
            {
                RemoveWallCells(walkableCells, room.Walls);
            }
        }

        return walkableCells;
    }

    private static void RemoveWallCells(
        HashSet<Vector2Int> walkableCells,
        IReadOnlyList<GeneratedWallData> walls
    )
    {
        for (int wallIndex = 0; wallIndex < walls.Count; wallIndex++)
        {
            GeneratedWallData wall = walls[wallIndex];

            if (wall == null)
            {
                continue;
            }

            for (int cellIndex = 0; cellIndex < wall.Cells.Count; cellIndex++)
            {
                walkableCells.Remove(wall.Cells[cellIndex].Position);
            }
        }
    }

    private static bool TryFindPublicStartCell(
        GeneratedFloorData floorData,
        HashSet<Vector2Int> walkableCells,
        out Vector2Int startCell
    )
    {
        foreach (Vector2Int cell in walkableCells)
        {
            if (!IsInsideAnyRoom(floorData, cell))
            {
                startCell = cell;
                return true;
            }
        }

        startCell = default;
        return false;
    }

    private static bool IsInsideAnyRoom(GeneratedFloorData floorData, Vector2Int cell)
    {
        for (int i = 0; i < floorData.Rooms.Count; i++)
        {
            GeneratedRoomData room = floorData.Rooms[i];

            if (room != null && room.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }

    private static HashSet<Vector2Int> FloodFill(Vector2Int startCell, HashSet<Vector2Int> walkableCells)
    {
        HashSet<Vector2Int> reachableCells = new HashSet<Vector2Int>();
        Queue<Vector2Int> pendingCells = new Queue<Vector2Int>();

        reachableCells.Add(startCell);
        pendingCells.Enqueue(startCell);

        while (pendingCells.Count > 0)
        {
            Vector2Int cell = pendingCells.Dequeue();
            TryVisit(cell + Vector2Int.up, walkableCells, reachableCells, pendingCells);
            TryVisit(cell + Vector2Int.down, walkableCells, reachableCells, pendingCells);
            TryVisit(cell + Vector2Int.left, walkableCells, reachableCells, pendingCells);
            TryVisit(cell + Vector2Int.right, walkableCells, reachableCells, pendingCells);
        }

        return reachableCells;
    }

    private static void TryVisit(
        Vector2Int cell,
        HashSet<Vector2Int> walkableCells,
        HashSet<Vector2Int> reachableCells,
        Queue<Vector2Int> pendingCells
    )
    {
        if (!walkableCells.Contains(cell) || !reachableCells.Add(cell))
        {
            return;
        }

        pendingCells.Enqueue(cell);
    }

    private static void ValidateRoomConnectivity(
        GeneratedFloorData floorData,
        GeneratedRoomData room,
        HashSet<Vector2Int> reachableCells,
        GeneratedBuildingValidationResult result
    )
    {
        if (room.Doors.Count == 0)
        {
            return;
        }

        for (int i = 0; i < room.Doors.Count; i++)
        {
            GeneratedDoorData door = room.Doors[i];

            if (door == null)
            {
                continue;
            }

            if (IsDoorReachable(room, door, reachableCells))
            {
                return;
            }
        }

        result.AddError($"{room.RoomId}: no reachable door connects this room to public floor space.");
    }

    private static bool IsDoorReachable(
        GeneratedRoomData room,
        GeneratedDoorData door,
        HashSet<Vector2Int> reachableCells
    )
    {
        for (int i = 0; i < door.Cells.Count; i++)
        {
            Vector2Int doorCell = door.Cells[i].Position;

            if (reachableCells.Contains(doorCell) && HasReachableRoomInterior(room, doorCell, reachableCells))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasReachableRoomInterior(
        GeneratedRoomData room,
        Vector2Int doorCell,
        HashSet<Vector2Int> reachableCells
    )
    {
        return IsReachableRoomInteriorCell(room, doorCell, reachableCells) ||
               IsReachableRoomInteriorCell(room, doorCell + Vector2Int.up, reachableCells) ||
               IsReachableRoomInteriorCell(room, doorCell + Vector2Int.down, reachableCells) ||
               IsReachableRoomInteriorCell(room, doorCell + Vector2Int.left, reachableCells) ||
               IsReachableRoomInteriorCell(room, doorCell + Vector2Int.right, reachableCells);
    }

    private static bool IsReachableRoomInteriorCell(
        GeneratedRoomData room,
        Vector2Int cell,
        HashSet<Vector2Int> reachableCells
    )
    {
        return room.Contains(cell) && reachableCells.Contains(cell);
    }

    private static void ValidateStairConnectivity(
        GeneratedBuildingData buildingData,
        GeneratedBuildingValidationResult result
    )
    {
        for (int floorIndex = 0; floorIndex < buildingData.Floors.Count; floorIndex++)
        {
            GeneratedFloorData floorData = buildingData.Floors[floorIndex];

            if (floorData == null)
            {
                continue;
            }

            for (int stairIndex = 0; stairIndex < floorData.Stairs.Count; stairIndex++)
            {
                GeneratedStairData stair = floorData.Stairs[stairIndex];

                if (stair == null)
                {
                    continue;
                }

                GeneratedFloorData lowerFloor = buildingData.GetFloor(stair.LowerFloorIndex);
                GeneratedFloorData upperFloor = buildingData.GetFloor(stair.UpperFloorIndex);

                if (lowerFloor == null || upperFloor == null)
                {
                    continue;
                }

                if (!ContainsStairReference(lowerFloor, stair.StairId))
                {
                    result.AddError($"{stair.StairId}: lower floor does not reference this stair.");
                }

                if (!ContainsStairReference(upperFloor, stair.StairId))
                {
                    result.AddError($"{stair.StairId}: upper floor does not reference this stair.");
                }

                if (upperFloor.FloorHoleMask.CellCount <= 0)
                {
                    result.AddError($"{stair.StairId}: upper floor has no stair hole cells.");
                }
            }
        }
    }

    private static void ValidateStairAccessOnFloor(
        GeneratedFloorData floorData,
        HashSet<Vector2Int> reachableCells,
        GeneratedBuildingValidationResult result
    )
    {
        for (int stairIndex = 0; stairIndex < floorData.Stairs.Count; stairIndex++)
        {
            GeneratedStairData stair = floorData.Stairs[stairIndex];

            if (stair == null)
            {
                continue;
            }

            if (floorData.FloorIndex == stair.LowerFloorIndex &&
                !HasReachableNeighbor(stair.VisualMask, reachableCells))
            {
                result.AddError($"{stair.StairId}: lower-floor stair entrance is not reachable from public floor area.");
            }

            if (floorData.FloorIndex == stair.UpperFloorIndex &&
                !HasReachableNeighbor(floorData.FloorHoleMask, reachableCells))
            {
                result.AddError($"{stair.StairId}: upper-floor stair hole is not reachable from public floor area.");
            }
        }
    }

    private static bool HasReachableNeighbor(
        GeneratedCellMask mask,
        HashSet<Vector2Int> reachableCells
    )
    {
        if (mask == null)
        {
            return false;
        }

        for (int i = 0; i < mask.Cells.Count; i++)
        {
            Vector2Int cell = mask.Cells[i].Position;

            if (reachableCells.Contains(cell) ||
                reachableCells.Contains(cell + Vector2Int.up) ||
                reachableCells.Contains(cell + Vector2Int.down) ||
                reachableCells.Contains(cell + Vector2Int.left) ||
                reachableCells.Contains(cell + Vector2Int.right))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsStairReference(GeneratedFloorData floorData, string stairId)
    {
        for (int i = 0; i < floorData.Stairs.Count; i++)
        {
            GeneratedStairData stair = floorData.Stairs[i];

            if (stair != null && stair.StairId == stairId)
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateMaskNotEmpty(
        string ownerId,
        string maskLabel,
        GeneratedCellMask mask,
        GeneratedBuildingValidationResult result
    )
    {
        if (mask == null || mask.CellCount <= 0)
        {
            result.AddError($"{ownerId}: {maskLabel} mask is empty.");
        }
    }

    private static bool ContainsRect(RectInt outer, RectInt inner)
    {
        return inner.xMin >= outer.xMin &&
               inner.yMin >= outer.yMin &&
               inner.xMax <= outer.xMax &&
               inner.yMax <= outer.yMax;
    }
}
