using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneratedDoorData
{
    [SerializeField] private string doorId;
    [SerializeField] private string ownerId;
    [SerializeField] private int floorIndex;
    [SerializeField] private Vector2Int cell;
    [SerializeField] private GeneratedWallSide wallSide;
    [SerializeField] private List<GeneratedCell> cells = new List<GeneratedCell>();

    public string DoorId => doorId;
    public string OwnerId => ownerId;
    public int FloorIndex => floorIndex;
    public Vector2Int Cell => cell;
    public GeneratedWallSide WallSide => wallSide;
    public IReadOnlyList<GeneratedCell> Cells => cells;

    public void Initialize(
        string newDoorId,
        string newOwnerId,
        int newFloorIndex,
        Vector2Int newCell,
        GeneratedWallSide newWallSide
    )
    {
        doorId = newDoorId;
        ownerId = newOwnerId;
        floorIndex = newFloorIndex;
        cell = newCell;
        wallSide = newWallSide;
        cells.Clear();
        AddCell(newCell);
    }

    public void AddCell(Vector2Int newCell)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].Position == newCell)
            {
                return;
            }
        }

        cells.Add(new GeneratedCell(newCell));
    }
}
