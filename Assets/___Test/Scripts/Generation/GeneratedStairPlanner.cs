using System.Collections.Generic;
using UnityEngine;

public static class GeneratedStairPlanner
{
    public static void AddStairs(GeneratedBuildingData buildingData, GeneratedBuildingProfile profile)
    {
        if (buildingData == null || profile == null || !profile.GenerateStairs || profile.FloorCount < 2)
        {
            return;
        }

        GeneratedStairModulePreset stairPreset = profile.StairModulePreset;

        if (stairPreset == null)
        {
            return;
        }

        RectInt stairVisualBounds = ClampStairBounds(
            profile.DefaultStairVisualBounds,
            stairPreset.FallbackVisualBounds,
            stairPreset.MaxReasonableStairArea
        );
        RectInt upperFloorHoleBounds = ClampStairBounds(
            profile.DefaultUpperFloorHoleBounds,
            stairPreset.FallbackUpperFloorHoleBounds,
            stairPreset.MaxReasonableStairArea
        );

        for (int floorIndex = 1; floorIndex < profile.FloorCount; floorIndex++)
        {
            AddStairBetweenFloors(
                buildingData,
                stairPreset,
                stairVisualBounds,
                upperFloorHoleBounds,
                floorIndex,
                floorIndex + 1
            );
        }
    }

    private static void AddStairBetweenFloors(
        GeneratedBuildingData buildingData,
        GeneratedStairModulePreset stairPreset,
        RectInt stairVisualBounds,
        RectInt upperFloorHoleBounds,
        int lowerFloorIndex,
        int upperFloorIndex
    )
    {
        GeneratedFloorData lowerFloor = buildingData.GetFloor(lowerFloorIndex);
        GeneratedFloorData upperFloor = buildingData.GetFloor(upperFloorIndex);

        if (lowerFloor == null || upperFloor == null)
        {
            return;
        }

        GeneratedStairData stairData = new GeneratedStairData();
        stairData.Initialize(
            $"stair_{lowerFloorIndex:00}_to_{upperFloorIndex:00}",
            lowerFloor.FloorIndex,
            upperFloor.FloorIndex,
            GeneratedStairDirection.FloorUp,
            stairVisualBounds
        );

        FillVisualMask(stairData, stairPreset);
        FillUpperFloorHole(upperFloor, upperFloorHoleBounds);
        FillPreciseStairUtilityMasks(stairData, stairPreset);

        lowerFloor.AddStair(stairData);
        upperFloor.AddStair(stairData);
    }

    private static RectInt ClampStairBounds(
        RectInt sourceBounds,
        RectInt fallbackBounds,
        int maxReasonableStairArea
    )
    {
        if (sourceBounds.width <= 0 || sourceBounds.height <= 0)
        {
            return fallbackBounds;
        }

        if (sourceBounds.width * sourceBounds.height > maxReasonableStairArea)
        {
            return fallbackBounds;
        }

        return sourceBounds;
    }

    private static void FillVisualMask(GeneratedStairData stairData, GeneratedStairModulePreset stairPreset)
    {
        RectInt bounds = stairData.VisualBounds;
        List<int> rowOffsets = new List<int>();

        for (int row = 0; row < stairPreset.VisualRowCount; row++)
        {
            if (!stairPreset.TryGetVisualRowOffsets(row, rowOffsets))
            {
                continue;
            }

            int y = bounds.yMin + row;

            for (int i = 0; i < rowOffsets.Count; i++)
            {
                int x = bounds.xMin + rowOffsets[i];
                stairData.VisualMask.AddCell(new Vector2Int(x, y));
            }
        }
    }

    private static void FillUpperFloorHole(GeneratedFloorData upperFloor, RectInt holeBounds)
    {
        upperFloor.AddFloorHole(holeBounds);
    }

    private static void FillPreciseStairUtilityMasks(
        GeneratedStairData stairData,
        GeneratedStairModulePreset stairPreset
    )
    {
        RectInt visualBounds = stairData.VisualBounds;
        Vector2Int fineOrigin = new Vector2Int(
            visualBounds.xMin * stairPreset.FineGridScale,
            visualBounds.yMin * stairPreset.FineGridScale
        );

        IReadOnlyList<int> minOffsets = stairPreset.WalkableMinOffsets;
        IReadOnlyList<int> maxOffsets = stairPreset.WalkableMaxOffsets;

        for (int row = 0; row < minOffsets.Count; row++)
        {
            int y = fineOrigin.y + row;

            for (int x = fineOrigin.x + minOffsets[row]; x <= fineOrigin.x + maxOffsets[row]; x++)
            {
                stairData.WalkableMask.AddCell(new Vector2Int(x, y));
            }
        }

        for (int row = stairPreset.EntryTriggerRowRange.x; row <= stairPreset.EntryTriggerRowRange.y; row++)
        {
            stairData.EntryTriggerMask.AddCell(new Vector2Int(fineOrigin.x, fineOrigin.y + row));
        }

        for (int row = stairPreset.ExitTriggerRowRange.x; row <= stairPreset.ExitTriggerRowRange.y; row++)
        {
            stairData.ExitTriggerMask.AddCell(
                new Vector2Int(fineOrigin.x + stairPreset.ExitTriggerXOffset, fineOrigin.y + row)
            );
        }

        Vector2Int rightRailStart = ConvertReferenceRailCellToCurrentStair(
            stairPreset.ReferenceRightRailStart,
            visualBounds,
            stairPreset
        );
        Vector2Int leftRailStart = ConvertReferenceRailCellToCurrentStair(
            stairPreset.ReferenceLeftRailStart,
            visualBounds,
            stairPreset
        );

        for (int offset = 0; offset < stairPreset.RailLength; offset++)
        {
            stairData.RightRailMask.AddCell(rightRailStart + new Vector2Int(offset, 0));
            stairData.LeftRailMask.AddCell(leftRailStart + new Vector2Int(offset, 0));
        }
    }

    private static Vector2Int ConvertReferenceRailCellToCurrentStair(
        Vector2Int referenceRailCell,
        RectInt currentVisualBounds,
        GeneratedStairModulePreset stairPreset
    )
    {
        Vector2Int visualDelta = currentVisualBounds.min - stairPreset.ReferenceStairVisualMin;
        Vector2 fineGridDelta = new Vector2(
            visualDelta.x * stairPreset.FineGridScale,
            visualDelta.y * stairPreset.FineGridScale
        );

        Vector2 desiredLocalOffset = IsometricCellToLocal(fineGridDelta);
        Vector2 railLocalOffset = Rotate(desiredLocalOffset, -stairPreset.RailRotationDegrees);
        Vector2 railCellOffset = LocalToIsometricCell(railLocalOffset);

        return referenceRailCell + new Vector2Int(
            Mathf.RoundToInt(railCellOffset.x),
            Mathf.RoundToInt(railCellOffset.y)
        );
    }

    private static Vector2 IsometricCellToLocal(Vector2 cell)
    {
        return new Vector2(
            (cell.x - cell.y) * 0.1f,
            (cell.x + cell.y) * 0.05f
        );
    }

    private static Vector2 LocalToIsometricCell(Vector2 local)
    {
        float sum = local.y / 0.05f;
        float difference = local.x / 0.1f;
        return new Vector2(
            (sum + difference) * 0.5f,
            (sum - difference) * 0.5f
        );
    }

    private static Vector2 Rotate(Vector2 value, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            value.x * cos - value.y * sin,
            value.x * sin + value.y * cos
        );
    }
}
