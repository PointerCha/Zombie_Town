using System.Collections.Generic;
using UnityEngine;

public static class GeneratedRoomPlanner
{
    private static readonly IGeneratedRoomPlacementStrategy SectorPlacementStrategy =
        new GeneratedSectorRoomPlacementStrategy();
    private static readonly IGeneratedRoomPlacementStrategy RandomPackedPlacementStrategy =
        new GeneratedRandomPackedRoomPlacementStrategy();

    private static readonly GeneratedWallSide[] DoorSideSearchOrder =
    {
        GeneratedWallSide.East,
        GeneratedWallSide.South,
        GeneratedWallSide.West,
        GeneratedWallSide.North
    };

    public static void AddRooms(GeneratedBuildingData buildingData, GeneratedBuildingProfile profile)
    {
        if (buildingData == null || profile == null)
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

            AddRoomsToFloor(buildingData, floorData, profile, buildingData.Seed + floorData.FloorIndex * 997);
        }
    }

    private static void AddRoomsToFloor(
        GeneratedBuildingData buildingData,
        GeneratedFloorData floorData,
        GeneratedBuildingProfile profile,
        int floorSeed
    )
    {
        RectInt availableBounds = Shrink(floorData.Bounds, profile.RoomPaddingFromOuterWall);

        if (availableBounds.width <= 0 || availableBounds.height <= 0)
        {
            return;
        }

        Random.State previousState = Random.state;
        Random.InitState(floorSeed);

        int roomCount = Random.Range(profile.MinRoomsPerFloor, profile.MaxRoomsPerFloor + 1);
        roomCount = Mathf.Max(0, roomCount);
        List<RectInt> placedRoomBounds = new List<RectInt>(roomCount);

        for (int i = 0; i < roomCount; i++)
        {
            RectInt roomBounds = BuildRoomBounds(
                floorData,
                buildingData,
                availableBounds,
                roomCount,
                i,
                placedRoomBounds,
                profile
            );

            if (roomBounds.width <= 0 || roomBounds.height <= 0)
            {
                continue;
            }

            GeneratedRoomData roomData = new GeneratedRoomData();
            roomData.Initialize(
                $"floor_{floorData.FloorIndex:00}_room_{i + 1:00}",
                $"Floor {floorData.FloorIndex:00} Room {i + 1:00}",
                GeneratedRoomType.Custom,
                floorData.FloorIndex,
                roomBounds
            );

            AddRoomWalls(roomData);
            AddDefaultRoomDoor(floorData, roomData, profile);
            CutDoorOpenings(roomData);
            RegisterRoomDoorsOnFloor(floorData, roomData);
            floorData.AddRoom(roomData);
            placedRoomBounds.Add(roomBounds);
        }

        Random.state = previousState;
    }

    private static RectInt Shrink(RectInt rect, int padding)
    {
        return new RectInt(
            rect.xMin + padding,
            rect.yMin + padding,
            rect.width - padding * 2,
            rect.height - padding * 2
        );
    }

    private static RectInt BuildSector(RectInt availableBounds, int roomCount, int roomIndex, int gap)
    {
        if (roomCount <= 1)
        {
            return Shrink(availableBounds, gap);
        }

        int sectorWidth = Mathf.Max(1, availableBounds.width / roomCount);
        int xMin = availableBounds.xMin + sectorWidth * roomIndex;
        int xMax = roomIndex == roomCount - 1
            ? availableBounds.xMax
            : Mathf.Min(availableBounds.xMax, xMin + sectorWidth);

        RectInt sector = new RectInt(
            xMin,
            availableBounds.yMin,
            Mathf.Max(1, xMax - xMin),
            availableBounds.height
        );

        return Shrink(sector, gap);
    }

    private static RectInt BuildRoomBounds(
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        RectInt availableBounds,
        int roomCount,
        int roomIndex,
        List<RectInt> placedRoomBounds,
        GeneratedBuildingProfile profile
    )
    {
        IGeneratedRoomPlacementStrategy strategy = profile.RoomLayoutStrategy == GeneratedRoomLayoutStrategy.RandomPacked
            ? RandomPackedPlacementStrategy
            : SectorPlacementStrategy;

        if (strategy.TryPlace(
                floorData,
                buildingData,
                availableBounds,
                roomCount,
                roomIndex,
                placedRoomBounds,
                profile,
                out RectInt roomBounds
            ))
        {
            return roomBounds;
        }

        if (strategy != SectorPlacementStrategy &&
            SectorPlacementStrategy.TryPlace(
                floorData,
                buildingData,
                availableBounds,
                roomCount,
                roomIndex,
                placedRoomBounds,
                profile,
                out roomBounds
            ))
        {
            return roomBounds;
        }

        return new RectInt();
    }

    internal static bool TryBuildSectorRoom(
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
        RectInt sector = BuildSector(availableBounds, roomCount, roomIndex, profile.MinimumRoomGap);
        roomBounds = BuildRoomBounds(sector, profile);
        return IsRoomPlacementValid(floorData, buildingData, roomBounds, placedRoomBounds, profile);
    }

    private static RectInt BuildRoomBounds(RectInt sector, GeneratedBuildingProfile profile)
    {
        int maxWidth = Mathf.Min(profile.MaxRoomSize.x, sector.width);
        int maxHeight = Mathf.Min(profile.MaxRoomSize.y, sector.height);

        if (maxWidth < profile.MinRoomSize.x || maxHeight < profile.MinRoomSize.y)
        {
            return new RectInt();
        }

        int width = Random.Range(profile.MinRoomSize.x, maxWidth + 1);
        int height = Random.Range(profile.MinRoomSize.y, maxHeight + 1);
        int x = Random.Range(sector.xMin, sector.xMax - width + 1);
        int y = Random.Range(sector.yMin, sector.yMax - height + 1);

        return new RectInt(x, y, width, height);
    }

    internal static bool TryBuildRandomPackedRoom(
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        RectInt availableBounds,
        List<RectInt> placedRoomBounds,
        GeneratedBuildingProfile profile,
        out RectInt roomBounds
    )
    {
        roomBounds = new RectInt();
        float bestScore = float.MinValue;
        int attempts = Mathf.Max(1, profile.RoomPlacementAttemptsPerRoom);

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            RectInt candidate = BuildRoomBounds(availableBounds, profile);

            if (!IsRoomPlacementValid(floorData, buildingData, candidate, placedRoomBounds, profile))
            {
                continue;
            }

            float score = ScoreRoomCandidate(candidate, availableBounds, placedRoomBounds);

            if (score <= bestScore)
            {
                continue;
            }

            roomBounds = candidate;
            bestScore = score;
        }

        return roomBounds.width > 0 && roomBounds.height > 0;
    }

    private static bool IsRoomPlacementValid(
        GeneratedFloorData floorData,
        GeneratedBuildingData buildingData,
        RectInt roomBounds,
        List<RectInt> placedRoomBounds,
        GeneratedBuildingProfile profile
    )
    {
        if (floorData == null || roomBounds.width <= 0 || roomBounds.height <= 0)
        {
            return false;
        }

        if (!ContainsRect(floorData.Bounds, roomBounds))
        {
            return false;
        }

        if (IntersectsMask(roomBounds, floorData.FloorHoleMask, profile.StairClearanceFromRooms))
        {
            return false;
        }

        if (IntersectsAnyStairMask(roomBounds, floorData, profile.StairClearanceFromRooms))
        {
            return false;
        }

        if (IntersectsEntranceClearance(roomBounds, buildingData, floorData.FloorIndex, profile.EntranceClearanceFromRooms))
        {
            return false;
        }

        for (int i = 0; i < placedRoomBounds.Count; i++)
        {
            RectInt occupiedBounds = Inflate(placedRoomBounds[i], profile.MinimumRoomGap);

            if (occupiedBounds.Overlaps(roomBounds))
            {
                return false;
            }
        }

        return HasDoorClearance(floorData, roomBounds, profile);
    }

    private static bool IntersectsEntranceClearance(
        RectInt roomBounds,
        GeneratedBuildingData buildingData,
        int floorIndex,
        int clearance
    )
    {
        if (buildingData == null || clearance <= 0)
        {
            return false;
        }

        for (int i = 0; i < buildingData.Entrances.Count; i++)
        {
            GeneratedEntranceData entrance = buildingData.Entrances[i];

            if (entrance == null || entrance.FloorIndex != floorIndex)
            {
                continue;
            }

            if (IntersectsEntranceClearance(roomBounds, entrance, clearance))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IntersectsEntranceClearance(
        RectInt roomBounds,
        GeneratedEntranceData entrance,
        int clearance
    )
    {
        Vector2Int inwardDirection = -GetDoorExteriorDirection(entrance.Side);
        Vector2Int sideDirection = GetEntranceSideDirection(entrance.Side);
        int sidePadding = Mathf.Max(1, clearance / 2);

        for (int cellIndex = 0; cellIndex < entrance.Cells.Count; cellIndex++)
        {
            Vector2Int entranceCell = entrance.Cells[cellIndex].Position;

            for (int depth = 0; depth <= clearance; depth++)
            {
                Vector2Int corridorCell = entranceCell + inwardDirection * depth;

                for (int side = -sidePadding; side <= sidePadding; side++)
                {
                    if (roomBounds.Contains(corridorCell + sideDirection * side))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static Vector2Int GetEntranceSideDirection(GeneratedWallSide side)
    {
        switch (side)
        {
            case GeneratedWallSide.North:
            case GeneratedWallSide.South:
                return Vector2Int.up;
            case GeneratedWallSide.West:
            case GeneratedWallSide.East:
            default:
                return Vector2Int.right;
        }
    }

    private static float ScoreRoomCandidate(
        RectInt candidate,
        RectInt availableBounds,
        List<RectInt> placedRoomBounds
    )
    {
        Vector2 candidateCenter = candidate.center;
        Vector2 availableCenter = availableBounds.center;
        float centerBias = -Vector2.Distance(candidateCenter, availableCenter) * 0.05f;

        if (placedRoomBounds.Count == 0)
        {
            return Random.value + centerBias;
        }

        float nearestDistance = float.MaxValue;

        for (int i = 0; i < placedRoomBounds.Count; i++)
        {
            nearestDistance = Mathf.Min(
                nearestDistance,
                Vector2.Distance(candidateCenter, placedRoomBounds[i].center)
            );
        }

        return nearestDistance + centerBias + Random.value * 0.1f;
    }

    private static bool HasDoorClearance(
        GeneratedFloorData floorData,
        RectInt roomBounds,
        GeneratedBuildingProfile profile
    )
    {
        return TrySelectDoorSide(floorData, roomBounds, profile, false, out _);
    }

    private static Vector2Int GetDoorExteriorDirection(GeneratedWallSide side)
    {
        switch (side)
        {
            case GeneratedWallSide.North:
                return Vector2Int.right;
            case GeneratedWallSide.South:
                return Vector2Int.left;
            case GeneratedWallSide.West:
                return Vector2Int.up;
            case GeneratedWallSide.East:
            default:
                return Vector2Int.down;
        }
    }

    private static bool IntersectsAnyStairMask(
        RectInt roomBounds,
        GeneratedFloorData floorData,
        int clearance
    )
    {
        for (int i = 0; i < floorData.Stairs.Count; i++)
        {
            GeneratedStairData stairData = floorData.Stairs[i];

            if (stairData == null)
            {
                continue;
            }

            if (IntersectsMask(roomBounds, stairData.VisualMask, clearance) ||
                IntersectsMask(roomBounds, stairData.EntryTriggerMask, clearance) ||
                IntersectsMask(roomBounds, stairData.ExitTriggerMask, clearance))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IntersectsMask(RectInt rect, GeneratedCellMask mask, int clearance)
    {
        if (mask == null || mask.CellCount <= 0)
        {
            return false;
        }

        RectInt checkedRect = Inflate(rect, clearance);

        for (int i = 0; i < mask.Cells.Count; i++)
        {
            if (checkedRect.Contains(mask.Cells[i].Position))
            {
                return true;
            }
        }

        return false;
    }

    private static RectInt Inflate(RectInt rect, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        return new RectInt(
            rect.xMin - safeAmount,
            rect.yMin - safeAmount,
            rect.width + safeAmount * 2,
            rect.height + safeAmount * 2
        );
    }

    private static bool ContainsRect(RectInt outer, RectInt inner)
    {
        return inner.xMin >= outer.xMin &&
               inner.yMin >= outer.yMin &&
               inner.xMax <= outer.xMax &&
               inner.yMax <= outer.yMax;
    }

    private static void AddRoomWalls(GeneratedRoomData roomData)
    {
        RectInt bounds = roomData.Bounds;

        AddRoomWall(
            roomData,
            GeneratedWallLayer.Upper,
            GeneratedWallSide.West,
            bounds.xMin,
            bounds.xMax,
            bounds.yMax - 1,
            true
        );

        AddRoomWall(
            roomData,
            GeneratedWallLayer.Upper,
            GeneratedWallSide.North,
            bounds.yMin,
            bounds.yMax - 1,
            bounds.xMax - 1,
            false
        );

        AddRoomWall(
            roomData,
            GeneratedWallLayer.Under,
            GeneratedWallSide.East,
            bounds.xMin,
            bounds.xMax - 1,
            bounds.yMin,
            true
        );

        AddRoomWall(
            roomData,
            GeneratedWallLayer.Under,
            GeneratedWallSide.South,
            bounds.yMin + 1,
            bounds.yMax - 1,
            bounds.xMin,
            false
        );

        AddRoomCorner(
            roomData,
            GeneratedWallLayer.Upper,
            GeneratedWallSide.South,
            new Vector2Int(bounds.xMin, bounds.yMax - 1)
        );

        AddRoomCorner(
            roomData,
            GeneratedWallLayer.Upper,
            GeneratedWallSide.West,
            new Vector2Int(bounds.xMax - 1, bounds.yMax - 1)
        );

        AddRoomCorner(
            roomData,
            GeneratedWallLayer.Upper,
            GeneratedWallSide.North,
            new Vector2Int(bounds.xMax - 1, bounds.yMin)
        );

        AddRoomCorner(
            roomData,
            GeneratedWallLayer.Under,
            GeneratedWallSide.East,
            new Vector2Int(bounds.xMin, bounds.yMin)
        );
    }

    private static void AddRoomWall(
        GeneratedRoomData roomData,
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
            $"{roomData.RoomId}_{layer.ToString().ToLowerInvariant()}_{side.ToString().ToLowerInvariant()}",
            GeneratedWallOwner.Room,
            roomData.RoomId,
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

        roomData.AddWall(wallData);
    }

    private static void AddRoomCorner(
        GeneratedRoomData roomData,
        GeneratedWallLayer layer,
        GeneratedWallSide side,
        Vector2Int cell
    )
    {
        GeneratedWallData wallData = new GeneratedWallData();
        wallData.Initialize(
            $"{roomData.RoomId}_{layer.ToString().ToLowerInvariant()}_corner_{side.ToString().ToLowerInvariant()}",
            GeneratedWallOwner.Room,
            roomData.RoomId,
            layer,
            side
        );
        wallData.AddCell(cell);
        roomData.AddWall(wallData);
    }

    private static void RegisterRoomDoorsOnFloor(GeneratedFloorData floorData, GeneratedRoomData roomData)
    {
        for (int i = 0; i < roomData.Doors.Count; i++)
        {
            floorData.AddDoor(roomData.Doors[i]);
        }
    }

    private static void AddDefaultRoomDoor(
        GeneratedFloorData floorData,
        GeneratedRoomData roomData,
        GeneratedBuildingProfile profile
    )
    {
        RectInt bounds = roomData.Bounds;
        int doorWidth = Mathf.Max(1, profile.DefaultRoomDoorWidth);

        if (!TrySelectDoorSide(floorData, bounds, profile, true, out GeneratedWallSide doorSide))
        {
            return;
        }

        List<Vector2Int> doorCells = GetDoorCells(bounds, doorSide, doorWidth);

        if (doorCells.Count <= 0)
        {
            return;
        }

        GeneratedDoorData doorData = new GeneratedDoorData();
        doorData.Initialize(
            $"{roomData.RoomId}_door_01",
            roomData.RoomId,
            roomData.FloorIndex,
            doorCells[0],
            doorSide
        );

        for (int i = 1; i < doorCells.Count; i++)
        {
            doorData.AddCell(doorCells[i]);
        }

        roomData.AddDoor(doorData);
    }

    private static bool TrySelectDoorSide(
        GeneratedFloorData floorData,
        RectInt roomBounds,
        GeneratedBuildingProfile profile,
        bool allowRandomSelection,
        out GeneratedWallSide selectedSide
    )
    {
        selectedSide = profile.DefaultRoomDoorSide;

        switch (profile.RoomDoorSideSelectionStrategy)
        {
            case GeneratedDoorSideSelectionStrategy.FixedDefaultSide:
                return IsDoorSideClear(floorData, roomBounds, selectedSide, profile.DefaultRoomDoorWidth);

            case GeneratedDoorSideSelectionStrategy.RandomAvailable:
                return TrySelectRandomDoorSide(
                    floorData,
                    roomBounds,
                    profile.DefaultRoomDoorWidth,
                    allowRandomSelection,
                    out selectedSide
                );

            case GeneratedDoorSideSelectionStrategy.PreferDefaultThenAvailable:
            default:
                if (IsDoorSideClear(floorData, roomBounds, selectedSide, profile.DefaultRoomDoorWidth))
                {
                    return true;
                }

                for (int i = 0; i < DoorSideSearchOrder.Length; i++)
                {
                    GeneratedWallSide candidateSide = DoorSideSearchOrder[i];

                    if (candidateSide == profile.DefaultRoomDoorSide)
                    {
                        continue;
                    }

                    if (IsDoorSideClear(floorData, roomBounds, candidateSide, profile.DefaultRoomDoorWidth))
                    {
                        selectedSide = candidateSide;
                        return true;
                    }
                }

                return false;
        }
    }

    private static bool TrySelectRandomDoorSide(
        GeneratedFloorData floorData,
        RectInt roomBounds,
        int doorWidth,
        bool allowRandomSelection,
        out GeneratedWallSide selectedSide
    )
    {
        selectedSide = GeneratedWallSide.East;

        if (!allowRandomSelection)
        {
            for (int i = 0; i < DoorSideSearchOrder.Length; i++)
            {
                GeneratedWallSide candidateSide = DoorSideSearchOrder[i];

                if (IsDoorSideClear(floorData, roomBounds, candidateSide, doorWidth))
                {
                    selectedSide = candidateSide;
                    return true;
                }
            }

            return false;
        }

        List<GeneratedWallSide> availableSides = new List<GeneratedWallSide>(DoorSideSearchOrder.Length);

        for (int i = 0; i < DoorSideSearchOrder.Length; i++)
        {
            GeneratedWallSide candidateSide = DoorSideSearchOrder[i];

            if (IsDoorSideClear(floorData, roomBounds, candidateSide, doorWidth))
            {
                availableSides.Add(candidateSide);
            }
        }

        if (availableSides.Count <= 0)
        {
            return false;
        }

        selectedSide = availableSides[Random.Range(0, availableSides.Count)];
        return true;
    }

    private static bool IsDoorSideClear(
        GeneratedFloorData floorData,
        RectInt roomBounds,
        GeneratedWallSide side,
        int doorWidth
    )
    {
        List<Vector2Int> doorCells = GetDoorCells(roomBounds, side, Mathf.Max(1, doorWidth));

        if (doorCells.Count <= 0)
        {
            return false;
        }

        Vector2Int exteriorDirection = GetDoorExteriorDirection(side);

        for (int i = 0; i < doorCells.Count; i++)
        {
            Vector2Int exteriorCell = doorCells[i] + exteriorDirection;

            if (floorData.GroundMask.Contains(exteriorCell) &&
                !floorData.FloorHoleMask.Contains(exteriorCell) &&
                !roomBounds.Contains(exteriorCell))
            {
                return true;
            }
        }

        return false;
    }

    private static List<Vector2Int> GetDoorCells(RectInt bounds, GeneratedWallSide side, int width)
    {
        List<Vector2Int> cells = new List<Vector2Int>(Mathf.Max(1, width));
        Vector2Int centerCell = GetDoorCenterCell(bounds, side);
        int halfWidth = Mathf.Max(1, width) / 2;

        for (int i = 0; i < Mathf.Max(1, width); i++)
        {
            int offset = i - halfWidth;
            Vector2Int cell;

            switch (side)
            {
                case GeneratedWallSide.North:
                case GeneratedWallSide.South:
                    cell = new Vector2Int(
                        centerCell.x,
                        Mathf.Clamp(centerCell.y + offset, bounds.yMin + 1, bounds.yMax - 2)
                    );
                    break;
                case GeneratedWallSide.West:
                case GeneratedWallSide.East:
                default:
                    cell = new Vector2Int(
                        Mathf.Clamp(centerCell.x + offset, bounds.xMin + 1, bounds.xMax - 2),
                        centerCell.y
                    );
                    break;
            }

            if (!cells.Contains(cell))
            {
                cells.Add(cell);
            }
        }

        return cells;
    }

    private static Vector2Int GetDoorCenterCell(RectInt bounds, GeneratedWallSide side)
    {
        switch (side)
        {
            case GeneratedWallSide.North:
                return new Vector2Int(bounds.xMax - 1, bounds.yMin + bounds.height / 2);
            case GeneratedWallSide.South:
                return new Vector2Int(bounds.xMin, bounds.yMin + bounds.height / 2);
            case GeneratedWallSide.West:
                return new Vector2Int(bounds.xMin + bounds.width / 2, bounds.yMax - 1);
            case GeneratedWallSide.East:
            default:
                return new Vector2Int(bounds.xMin + bounds.width / 2, bounds.yMin);
        }
    }

    private static void CutDoorOpenings(GeneratedRoomData roomData)
    {
        for (int i = 0; i < roomData.Doors.Count; i++)
        {
            GeneratedDoorData doorData = roomData.Doors[i];

            if (doorData == null)
            {
                continue;
            }

            for (int j = 0; j < roomData.Walls.Count; j++)
            {
                GeneratedWallData wallData = roomData.Walls[j];

                if (wallData == null || wallData.Side != doorData.WallSide)
                {
                    continue;
                }

                bool removedAnyDoorCell = false;

                for (int cellIndex = 0; cellIndex < doorData.Cells.Count; cellIndex++)
                {
                    removedAnyDoorCell |= wallData.RemoveCell(doorData.Cells[cellIndex].Position);
                }

                if (removedAnyDoorCell)
                {
                    break;
                }
            }
        }
    }
}
