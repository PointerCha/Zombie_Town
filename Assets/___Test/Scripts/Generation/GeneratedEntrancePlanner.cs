using System.Collections.Generic;
using UnityEngine;

public static class GeneratedEntrancePlanner
{
    public static void AddMainEntrance(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile
    )
    {
        if (buildingData == null || profile == null)
        {
            return;
        }

        GeneratedFloorData floorData = buildingData.GetFloor(profile.DefaultEntranceFloorIndex);

        if (floorData == null)
        {
            return;
        }

        List<Vector2Int> entranceCells = GetEntranceCells(
            floorData.Bounds,
            profile.DefaultEntranceSide,
            profile.DefaultEntranceWidth,
            profile.DefaultEntrancePositionStrategy,
            profile.DefaultEntranceSidePaddingFromCorners,
            buildingData.Seed,
            floorData.FloorIndex
        );
        Vector2Int roadConnectionCell = GetRoadConnectionCell(
            entranceCells,
            profile.DefaultEntranceSide,
            profile.DefaultEntranceRoadConnectionOffset
        );
        GeneratedEntranceData entranceData = new GeneratedEntranceData();
        entranceData.Initialize(
            "main_entrance_01",
            GeneratedEntranceKind.Main,
            floorData.FloorIndex,
            entranceCells,
            profile.DefaultEntranceSide,
            roadConnectionCell
        );

        CutEntranceOpening(floorData, entranceCells);
        buildingData.AddEntrance(entranceData);
    }

    private static List<Vector2Int> GetEntranceCells(
        RectInt bounds,
        GeneratedWallSide side,
        int width,
        GeneratedEntrancePositionStrategy positionStrategy,
        int sidePaddingFromCorners,
        int seed,
        int floorIndex
    )
    {
        List<Vector2Int> cells = new List<Vector2Int>(Mathf.Max(1, width));
        Vector2Int centerCell = GetEntranceCell(
            bounds,
            side,
            Mathf.Max(1, width),
            positionStrategy,
            Mathf.Max(0, sidePaddingFromCorners),
            seed,
            floorIndex
        );
        int halfWidth = Mathf.Max(1, width) / 2;

        for (int i = 0; i < Mathf.Max(1, width); i++)
        {
            int offset = i - halfWidth;
            Vector2Int cell;

            switch (side)
            {
                case GeneratedWallSide.North:
                case GeneratedWallSide.South:
                    cell = new Vector2Int(centerCell.x, Mathf.Clamp(centerCell.y + offset, bounds.yMin + 1, bounds.yMax - 2));
                    break;
                case GeneratedWallSide.West:
                case GeneratedWallSide.East:
                default:
                    cell = new Vector2Int(Mathf.Clamp(centerCell.x + offset, bounds.xMin + 1, bounds.xMax - 2), centerCell.y);
                    break;
            }

            if (!cells.Contains(cell))
            {
                cells.Add(cell);
            }
        }

        return cells;
    }

    private static Vector2Int GetEntranceCell(
        RectInt bounds,
        GeneratedWallSide side,
        int width,
        GeneratedEntrancePositionStrategy positionStrategy,
        int sidePaddingFromCorners,
        int seed,
        int floorIndex
    )
    {
        int sideCoordinate = GetEntranceSideCoordinate(
            bounds,
            side,
            width,
            positionStrategy,
            sidePaddingFromCorners,
            seed,
            floorIndex
        );

        switch (side)
        {
            case GeneratedWallSide.North:
                return new Vector2Int(bounds.xMax - 1, sideCoordinate);
            case GeneratedWallSide.South:
                return new Vector2Int(bounds.xMin, sideCoordinate);
            case GeneratedWallSide.West:
                return new Vector2Int(sideCoordinate, bounds.yMax - 1);
            case GeneratedWallSide.East:
            default:
                return new Vector2Int(sideCoordinate, bounds.yMin);
        }
    }

    private static int GetEntranceSideCoordinate(
        RectInt bounds,
        GeneratedWallSide side,
        int width,
        GeneratedEntrancePositionStrategy positionStrategy,
        int sidePaddingFromCorners,
        int seed,
        int floorIndex
    )
    {
        bool verticalSide = side == GeneratedWallSide.North || side == GeneratedWallSide.South;
        int min = verticalSide ? bounds.yMin + 1 : bounds.xMin + 1;
        int max = verticalSide ? bounds.yMax - 2 : bounds.xMax - 2;
        int halfWidth = Mathf.Max(1, width) / 2;
        int safeMin = min + sidePaddingFromCorners + halfWidth;
        int safeMax = max - sidePaddingFromCorners - halfWidth;

        if (safeMax < safeMin)
        {
            return (min + max) / 2;
        }

        if (positionStrategy == GeneratedEntrancePositionStrategy.SeededRandomAlongSide)
        {
            int hash = seed;
            hash = hash * 397 ^ floorIndex;
            hash = hash * 397 ^ (int)side;
            hash = hash * 397 ^ width;
            System.Random random = new System.Random(hash & int.MaxValue);
            return random.Next(safeMin, safeMax + 1);
        }

        return (safeMin + safeMax) / 2;
    }

    private static Vector2Int GetRoadConnectionCell(
        IReadOnlyList<Vector2Int> entranceCells,
        GeneratedWallSide side,
        int offset
    )
    {
        if (entranceCells == null || entranceCells.Count <= 0)
        {
            return Vector2Int.zero;
        }

        int centerIndex = Mathf.Clamp(entranceCells.Count / 2, 0, entranceCells.Count - 1);
        return entranceCells[centerIndex] + GetOutwardDirection(side) * Mathf.Max(1, offset);
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

    private static void CutEntranceOpening(GeneratedFloorData floorData, IReadOnlyList<Vector2Int> entranceCells)
    {
        if (entranceCells == null)
        {
            return;
        }

        for (int i = 0; i < floorData.OuterWalls.Count; i++)
        {
            GeneratedWallData wallData = floorData.OuterWalls[i];

            if (wallData == null)
            {
                continue;
            }

            for (int cellIndex = 0; cellIndex < entranceCells.Count; cellIndex++)
            {
                wallData.RemoveCell(entranceCells[cellIndex]);
            }
        }
    }
}
