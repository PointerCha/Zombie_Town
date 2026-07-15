using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class GeneratedStairModulePreset
{
    [Header("Identity")]
    [SerializeField] private string presetId = "house_01_stair_module";

    [Header("Fallback Bounds")]
    [SerializeField] private RectInt fallbackVisualBounds = new RectInt(-13, 0, 5, 6);
    [SerializeField] private RectInt fallbackUpperFloorHoleBounds = new RectInt(-15, 0, 6, 5);
    [SerializeField, Min(1)] private int maxReasonableStairArea = 64;

    [Header("Fine Grid")]
    [SerializeField, Min(1)] private int fineGridScale = 5;
    [SerializeField] private float railRotationDegrees = 30f;

    [Header("Reference Rail")]
    [SerializeField] private Vector2Int referenceStairVisualMin = new Vector2Int(-13, 25);
    [SerializeField] private Vector2Int referenceRightRailStart = new Vector2Int(46, 195);
    [SerializeField] private Vector2Int referenceLeftRailStart = new Vector2Int(59, 222);
    [SerializeField, Min(1)] private int railLength = 27;

    [Header("Visual Mask")]
    [SerializeField] private string[] visualRowOffsets =
    {
        "0",
        "0,2",
        "0,2,4",
        "0,2,4",
        "2,4",
        "4"
    };

    [Header("Walkable Mask")]
    [SerializeField] private int[] walkableMinOffsets =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 1, 3, 5, 7, 9, 11, 13, 15,
        17, 20, 21, 23, 25, 27, 29, 31
    };

    [SerializeField] private int[] walkableMaxOffsets =
    {
        6, 7, 8, 9, 10, 16, 17, 18, 19, 20,
        26, 27, 28, 29, 30, 31, 32, 32, 32, 32,
        32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
        32, 32, 32, 32, 32, 32, 32, 32
    };

    [Header("Transition Trigger Rows")]
    [SerializeField] private Vector2Int entryTriggerRowRange = new Vector2Int(0, 21);
    [SerializeField] private Vector2Int exitTriggerRowRange = new Vector2Int(17, 37);
    [SerializeField] private int exitTriggerXOffset = 32;

    public string PresetId => presetId;
    public RectInt FallbackVisualBounds => fallbackVisualBounds;
    public RectInt FallbackUpperFloorHoleBounds => fallbackUpperFloorHoleBounds;
    public int MaxReasonableStairArea => maxReasonableStairArea;
    public int FineGridScale => fineGridScale;
    public float RailRotationDegrees => railRotationDegrees;
    public Vector2Int ReferenceStairVisualMin => referenceStairVisualMin;
    public Vector2Int ReferenceRightRailStart => referenceRightRailStart;
    public Vector2Int ReferenceLeftRailStart => referenceLeftRailStart;
    public int RailLength => railLength;
    public IReadOnlyList<int> WalkableMinOffsets => walkableMinOffsets;
    public IReadOnlyList<int> WalkableMaxOffsets => walkableMaxOffsets;
    public Vector2Int EntryTriggerRowRange => entryTriggerRowRange;
    public Vector2Int ExitTriggerRowRange => exitTriggerRowRange;
    public int ExitTriggerXOffset => exitTriggerXOffset;

    public bool TryGetVisualRowOffsets(int row, List<int> offsets)
    {
        offsets.Clear();

        if (visualRowOffsets == null || row < 0 || row >= visualRowOffsets.Length)
        {
            return false;
        }

        string rowData = visualRowOffsets[row];

        if (string.IsNullOrWhiteSpace(rowData))
        {
            return false;
        }

        string[] parts = rowData.Split(',');

        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i].Trim(), out int offset))
            {
                offsets.Add(offset);
            }
        }

        return offsets.Count > 0;
    }

    public int VisualRowCount => visualRowOffsets != null ? visualRowOffsets.Length : 0;

    public bool IsValid(out string errorMessage)
    {
        StringBuilder builder = new StringBuilder();
        AppendInvalid(builder, string.IsNullOrWhiteSpace(presetId), "Stair preset id is missing.");
        AppendInvalid(builder, fallbackVisualBounds.width <= 0 || fallbackVisualBounds.height <= 0, "Stair fallback visual bounds must be positive.");
        AppendInvalid(builder, fallbackUpperFloorHoleBounds.width <= 0 || fallbackUpperFloorHoleBounds.height <= 0, "Stair fallback upper floor hole bounds must be positive.");
        AppendInvalid(builder, maxReasonableStairArea <= 0, "Stair max reasonable area must be positive.");
        AppendInvalid(builder, fineGridScale <= 0, "Stair fine grid scale must be positive.");
        AppendInvalid(builder, railLength <= 0, "Stair rail length must be positive.");
        AppendInvalid(builder, visualRowOffsets == null || visualRowOffsets.Length == 0, "Stair visual row offsets are missing.");
        AppendInvalid(builder, walkableMinOffsets == null || walkableMaxOffsets == null, "Stair walkable offsets are missing.");
        AppendInvalid(builder, walkableMinOffsets != null && walkableMaxOffsets != null && walkableMinOffsets.Length != walkableMaxOffsets.Length, "Stair walkable min/max offset lengths must match.");
        AppendInvalid(builder, entryTriggerRowRange.y < entryTriggerRowRange.x, "Stair entry row range is invalid.");
        AppendInvalid(builder, exitTriggerRowRange.y < exitTriggerRowRange.x, "Stair exit row range is invalid.");

        errorMessage = builder.ToString();
        return errorMessage.Length == 0;
    }

    private static void AppendInvalid(StringBuilder builder, bool invalid, string message)
    {
        if (!invalid)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(message);
    }
}
