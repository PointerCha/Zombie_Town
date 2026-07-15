using System;
using System.Text;
using UnityEngine;

[Serializable]
public class GeneratedStairData
{
    [SerializeField] private string stairId;
    [SerializeField] private int lowerFloorIndex;
    [SerializeField] private int upperFloorIndex;
    [SerializeField] private GeneratedStairDirection direction;
    [SerializeField] private RectInt visualBounds;
    [SerializeField] private GeneratedCellMask visualMask = new GeneratedCellMask();
    [SerializeField] private GeneratedCellMask walkableMask = new GeneratedCellMask();
    [SerializeField] private GeneratedCellMask leftRailMask = new GeneratedCellMask();
    [SerializeField] private GeneratedCellMask rightRailMask = new GeneratedCellMask();
    [SerializeField] private GeneratedCellMask entryTriggerMask = new GeneratedCellMask();
    [SerializeField] private GeneratedCellMask exitTriggerMask = new GeneratedCellMask();

    public string StairId => stairId;
    public int LowerFloorIndex => lowerFloorIndex;
    public int UpperFloorIndex => upperFloorIndex;
    public GeneratedStairDirection Direction => direction;
    public RectInt VisualBounds => visualBounds;
    public GeneratedCellMask VisualMask => visualMask;
    public GeneratedCellMask WalkableMask => walkableMask;
    public GeneratedCellMask LeftRailMask => leftRailMask;
    public GeneratedCellMask RightRailMask => rightRailMask;
    public GeneratedCellMask EntryTriggerMask => entryTriggerMask;
    public GeneratedCellMask ExitTriggerMask => exitTriggerMask;

    public void Initialize(
        string newStairId,
        int newLowerFloorIndex,
        int newUpperFloorIndex,
        GeneratedStairDirection newDirection,
        RectInt newVisualBounds
    )
    {
        stairId = newStairId;
        lowerFloorIndex = newLowerFloorIndex;
        upperFloorIndex = newUpperFloorIndex;
        direction = newDirection;
        visualBounds = newVisualBounds;

        visualMask.Initialize($"{stairId}_visual", newVisualBounds.position, newVisualBounds.size);
        walkableMask.Initialize($"{stairId}_walkable", newVisualBounds.position, newVisualBounds.size);
        leftRailMask.Initialize($"{stairId}_left_rail", newVisualBounds.position, newVisualBounds.size);
        rightRailMask.Initialize($"{stairId}_right_rail", newVisualBounds.position, newVisualBounds.size);
        entryTriggerMask.Initialize($"{stairId}_entry", newVisualBounds.position, newVisualBounds.size);
        exitTriggerMask.Initialize($"{stairId}_exit", newVisualBounds.position, newVisualBounds.size);
    }

    public string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(stairId);
        builder.Append(" visual=");
        builder.Append(visualMask.CellCount);
        builder.Append(" walkable=");
        builder.Append(walkableMask.CellCount);
        builder.Append(" leftRail=");
        builder.Append(leftRailMask.CellCount);
        builder.Append(" rightRail=");
        builder.Append(rightRailMask.CellCount);
        builder.Append(" entry=");
        builder.Append(entryTriggerMask.CellCount);
        builder.Append(" exit=");
        builder.Append(exitTriggerMask.CellCount);
        return builder.ToString();
    }
}
