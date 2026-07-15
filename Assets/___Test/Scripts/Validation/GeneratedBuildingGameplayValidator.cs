using System.Collections.Generic;
using UnityEngine;

public static class GeneratedBuildingGameplayValidator
{
    private const int MinimumGameplayEntranceCells = 3;
    private const int MinimumGameplayDoorCells = 3;

    public static void Validate(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        GeneratedBuildingValidationResult result
    )
    {
        if (buildingData == null || result == null)
        {
            return;
        }

        ValidateEntranceWidth(buildingData, result);
        ValidateEntranceRoadConnection(buildingData, result);
        ValidateDoorWidthAndShape(buildingData, result);
        ValidateStairGameplayAccess(buildingData, profile, result);
    }

    private static void ValidateEntranceWidth(
        GeneratedBuildingData buildingData,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < buildingData.Entrances.Count; i++)
        {
            GeneratedEntranceData entrance = buildingData.Entrances[i];

            if (entrance == null)
            {
                continue;
            }

            if (entrance.Cells.Count < MinimumGameplayEntranceCells)
            {
                result.AddError(
                    $"{entrance.EntranceId}: entrance opening is too narrow for gameplay. " +
                    $"cells={entrance.Cells.Count}, required={MinimumGameplayEntranceCells}"
                );
            }

            if (!AreCellsContiguousLine(entrance.Cells))
            {
                result.AddError($"{entrance.EntranceId}: entrance cells must form one contiguous line.");
            }
        }
    }

    private static void ValidateDoorWidthAndShape(
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

            for (int roomIndex = 0; roomIndex < floorData.Rooms.Count; roomIndex++)
            {
                GeneratedRoomData room = floorData.Rooms[roomIndex];

                if (room == null)
                {
                    continue;
                }

                for (int doorIndex = 0; doorIndex < room.Doors.Count; doorIndex++)
                {
                    GeneratedDoorData door = room.Doors[doorIndex];

                    if (door == null)
                    {
                        continue;
                    }

                    if (door.Cells.Count < MinimumGameplayDoorCells)
                    {
                        result.AddError(
                            $"{door.DoorId}: door opening is too narrow for gameplay. " +
                            $"cells={door.Cells.Count}, required={MinimumGameplayDoorCells}"
                        );
                    }

                    if (!AreCellsContiguousLine(door.Cells))
                    {
                        result.AddError($"{door.DoorId}: door cells must form one contiguous line.");
                    }
                }
            }
        }
    }

    private static void ValidateEntranceRoadConnection(
        GeneratedBuildingData buildingData,
        GeneratedBuildingValidationResult result
    )
    {
        for (int i = 0; i < buildingData.Entrances.Count; i++)
        {
            GeneratedEntranceData entrance = buildingData.Entrances[i];

            if (entrance == null)
            {
                continue;
            }

            GeneratedFloorData floorData = buildingData.GetFloor(entrance.FloorIndex);

            if (floorData == null)
            {
                continue;
            }

            if (floorData.Bounds.Contains(entrance.RoadConnectionCell))
            {
                result.AddError(
                    $"{entrance.EntranceId}: road connection cell must be outside floor bounds. " +
                    $"cell={entrance.RoadConnectionCell}, bounds={floorData.Bounds}"
                );
            }

            if (!RoadConnectionTouchesEntrance(entrance))
            {
                result.AddError(
                    $"{entrance.EntranceId}: road connection cell must align outward from entrance cells. " +
                    $"roadCell={entrance.RoadConnectionCell}, side={entrance.Side}"
                );
            }
        }
    }

    private static bool RoadConnectionTouchesEntrance(GeneratedEntranceData entrance)
    {
        Vector2Int outward = GetOutwardDirection(entrance.Side);

        for (int i = 0; i < entrance.Cells.Count; i++)
        {
            Vector2Int entranceCell = entrance.Cells[i].Position;
            Vector2Int delta = entrance.RoadConnectionCell - entranceCell;

            if (outward.x != 0)
            {
                if (delta.y == 0 && delta.x != 0 && Mathf.Sign(delta.x) == Mathf.Sign(outward.x))
                {
                    return true;
                }
            }
            else if (outward.y != 0)
            {
                if (delta.x == 0 && delta.y != 0 && Mathf.Sign(delta.y) == Mathf.Sign(outward.y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool AreCellsContiguousLine(IReadOnlyList<GeneratedCell> cells)
    {
        if (cells == null || cells.Count <= 1)
        {
            return true;
        }

        bool sameX = true;
        bool sameY = true;
        int x = cells[0].X;
        int y = cells[0].Y;

        for (int i = 1; i < cells.Count; i++)
        {
            sameX &= cells[i].X == x;
            sameY &= cells[i].Y == y;
        }

        if (!sameX && !sameY)
        {
            return false;
        }

        List<int> values = new List<int>(cells.Count);

        for (int i = 0; i < cells.Count; i++)
        {
            values.Add(sameX ? cells[i].Y : cells[i].X);
        }

        values.Sort();

        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] != values[i - 1] + 1)
            {
                return false;
            }
        }

        return true;
    }

    private static void ValidateStairGameplayAccess(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        GeneratedBuildingValidationResult result
    )
    {
        if (profile == null || !profile.GenerateStairs)
        {
            return;
        }

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

                if (!MasksTouch(stair.EntryTriggerMask, stair.WalkableMask))
                {
                    result.AddError($"{stair.StairId}: entry trigger does not touch stair walkable mask.");
                }

                if (!MasksTouch(stair.ExitTriggerMask, stair.WalkableMask))
                {
                    result.AddError($"{stair.StairId}: exit trigger does not touch stair walkable mask.");
                }
            }
        }
    }

    private static bool MasksTouch(GeneratedCellMask first, GeneratedCellMask second)
    {
        if (first == null || second == null)
        {
            return false;
        }

        for (int i = 0; i < first.Cells.Count; i++)
        {
            Vector2Int cell = first.Cells[i].Position;

            if (second.Contains(cell) ||
                second.Contains(cell + Vector2Int.up) ||
                second.Contains(cell + Vector2Int.down) ||
                second.Contains(cell + Vector2Int.left) ||
                second.Contains(cell + Vector2Int.right))
            {
                return true;
            }
        }

        return false;
    }

    private static Vector2Int GetOutwardDirection(GeneratedWallSide side)
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
}
