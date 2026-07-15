using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneratedWallData
{
    [SerializeField] private string wallId;
    [SerializeField] private GeneratedWallOwner owner;
    [SerializeField] private string ownerId;
    [SerializeField] private GeneratedWallLayer layer;
    [SerializeField] private GeneratedWallSide side;
    [SerializeField] private List<GeneratedCell> cells = new List<GeneratedCell>();

    public string WallId => wallId;
    public GeneratedWallOwner Owner => owner;
    public string OwnerId => ownerId;
    public GeneratedWallLayer Layer => layer;
    public GeneratedWallSide Side => side;
    public IReadOnlyList<GeneratedCell> Cells => cells;
    public int CellCount => cells.Count;

    public void Initialize(
        string newWallId,
        GeneratedWallOwner newOwner,
        string newOwnerId,
        GeneratedWallLayer newLayer,
        GeneratedWallSide newSide
    )
    {
        wallId = newWallId;
        owner = newOwner;
        ownerId = newOwnerId;
        layer = newLayer;
        side = newSide;
        cells.Clear();
    }

    public void AddCell(Vector2Int cell)
    {
        cells.Add(new GeneratedCell(cell));
    }

    public bool RemoveCell(Vector2Int cell)
    {
        for (int i = cells.Count - 1; i >= 0; i--)
        {
            if (cells[i].Position != cell)
            {
                continue;
            }

            cells.RemoveAt(i);
            return true;
        }

        return false;
    }
}
