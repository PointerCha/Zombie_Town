using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneratedEntranceData
{
    [SerializeField] private string entranceId;
    [SerializeField] private GeneratedEntranceKind entranceKind;
    [SerializeField] private int floorIndex;
    [SerializeField] private Vector2Int cell;
    [SerializeField] private Vector2Int roadConnectionCell;
    [SerializeField] private List<GeneratedCell> cells = new List<GeneratedCell>();
    [SerializeField] private GeneratedWallSide side;

    public string EntranceId => entranceId;
    public GeneratedEntranceKind EntranceKind => entranceKind;
    public int FloorIndex => floorIndex;
    public Vector2Int Cell => cell;
    public Vector2Int RoadConnectionCell => roadConnectionCell;
    public IReadOnlyList<GeneratedCell> Cells => cells;
    public GeneratedWallSide Side => side;

    public void Initialize(
        string newEntranceId,
        GeneratedEntranceKind newEntranceKind,
        int newFloorIndex,
        Vector2Int newCell,
        GeneratedWallSide newSide
    )
    {
        entranceId = newEntranceId;
        entranceKind = newEntranceKind;
        floorIndex = newFloorIndex;
        cell = newCell;
        side = newSide;
        roadConnectionCell = newCell + GetOutwardDirection(newSide);
        cells.Clear();
        cells.Add(new GeneratedCell(newCell));
    }

    public void Initialize(
        string newEntranceId,
        GeneratedEntranceKind newEntranceKind,
        int newFloorIndex,
        IReadOnlyList<Vector2Int> newCells,
        GeneratedWallSide newSide
    )
    {
        Initialize(
            newEntranceId,
            newEntranceKind,
            newFloorIndex,
            newCells,
            newSide,
            GetRoadConnectionCell(newCells, newSide, 1)
        );
    }

    public void Initialize(
        string newEntranceId,
        GeneratedEntranceKind newEntranceKind,
        int newFloorIndex,
        IReadOnlyList<Vector2Int> newCells,
        GeneratedWallSide newSide,
        Vector2Int newRoadConnectionCell
    )
    {
        entranceId = newEntranceId;
        entranceKind = newEntranceKind;
        floorIndex = newFloorIndex;
        side = newSide;
        roadConnectionCell = newRoadConnectionCell;
        cells.Clear();

        if (newCells == null || newCells.Count == 0)
        {
            cell = Vector2Int.zero;
            return;
        }

        cell = newCells[0];

        for (int i = 0; i < newCells.Count; i++)
        {
            cells.Add(new GeneratedCell(newCells[i]));
        }
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
}
