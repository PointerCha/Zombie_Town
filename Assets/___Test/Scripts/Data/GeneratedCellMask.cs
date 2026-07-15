using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class GeneratedCellMask
{
    [SerializeField] private string maskName = "Mask";
    [SerializeField] private Vector2Int origin;
    [SerializeField] private Vector2Int size;
    [SerializeField] private List<GeneratedCell> cells = new List<GeneratedCell>();

    [NonSerialized] private HashSet<Vector2Int> cellLookup;
    [NonSerialized] private bool lookupDirty = true;

    public string MaskName => maskName;
    public Vector2Int Origin => origin;
    public Vector2Int Size => size;
    public RectInt Bounds => new RectInt(origin, size);
    public IReadOnlyList<GeneratedCell> Cells => cells;
    public int CellCount => cells.Count;

    public void Initialize(string newMaskName, Vector2Int newOrigin, Vector2Int newSize)
    {
        maskName = newMaskName;
        origin = newOrigin;
        size = newSize;
        cells.Clear();
        MarkLookupDirty();
    }

    public void FillRect(RectInt rect)
    {
        for (int y = rect.yMin; y < rect.yMax; y++)
        {
            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                AddCell(new Vector2Int(x, y));
            }
        }
    }

    public void AddCell(Vector2Int cell)
    {
        EnsureLookup();

        if (cellLookup.Add(cell))
        {
            cells.Add(new GeneratedCell(cell));
        }
    }

    public void RemoveCell(Vector2Int cell)
    {
        EnsureLookup();

        if (!cellLookup.Remove(cell))
        {
            return;
        }

        for (int i = cells.Count - 1; i >= 0; i--)
        {
            if (cells[i].Position == cell)
            {
                cells.RemoveAt(i);
                break;
            }
        }
    }

    public bool Contains(Vector2Int cell)
    {
        EnsureLookup();
        return cellLookup.Contains(cell);
    }

    public void Clear()
    {
        cells.Clear();
        MarkLookupDirty();
    }

    public string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(maskName);
        builder.Append(" origin=");
        builder.Append(origin);
        builder.Append(" size=");
        builder.Append(size);
        builder.Append(" cells=");
        builder.Append(cells.Count);
        return builder.ToString();
    }

    private void EnsureLookup()
    {
        if (!lookupDirty && cellLookup != null)
        {
            return;
        }

        cellLookup = new HashSet<Vector2Int>();

        for (int i = 0; i < cells.Count; i++)
        {
            cellLookup.Add(cells[i].Position);
        }

        lookupDirty = false;
    }

    private void MarkLookupDirty()
    {
        lookupDirty = true;
    }
}
