using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class GeneratedFloorData
{
    [SerializeField] private string floorId;
    [SerializeField] private int floorIndex;
    [SerializeField] private Vector2Int origin;
    [SerializeField] private Vector2Int size = new Vector2Int(60, 42);
    [SerializeField] private GeneratedCellMask groundMask = new GeneratedCellMask();
    [SerializeField] private GeneratedCellMask floorHoleMask = new GeneratedCellMask();
    [SerializeField] private List<GeneratedWallData> outerWalls = new List<GeneratedWallData>();
    [SerializeField] private List<GeneratedRoomData> rooms = new List<GeneratedRoomData>();
    [SerializeField] private List<GeneratedDoorData> doors = new List<GeneratedDoorData>();
    [SerializeField] private List<GeneratedStairData> stairs = new List<GeneratedStairData>();

    public string FloorId => floorId;
    public int FloorIndex => floorIndex;
    public Vector2Int Origin => origin;
    public Vector2Int Size => size;
    public RectInt Bounds => new RectInt(origin, size);
    public GeneratedCellMask GroundMask => groundMask;
    public GeneratedCellMask FloorHoleMask => floorHoleMask;
    public IReadOnlyList<GeneratedWallData> OuterWalls => outerWalls;
    public IReadOnlyList<GeneratedRoomData> Rooms => rooms;
    public IReadOnlyList<GeneratedDoorData> Doors => doors;
    public IReadOnlyList<GeneratedStairData> Stairs => stairs;

    public void Initialize(string newFloorId, int newFloorIndex, Vector2Int newOrigin, Vector2Int newSize)
    {
        floorId = newFloorId;
        floorIndex = newFloorIndex;
        origin = newOrigin;
        size = newSize;

        groundMask.Initialize($"{floorId}_ground", origin, size);
        groundMask.FillRect(Bounds);
        floorHoleMask.Initialize($"{floorId}_holes", origin, size);
        outerWalls.Clear();
        rooms.Clear();
        doors.Clear();
        stairs.Clear();
    }

    public void AddFloorHole(RectInt holeBounds)
    {
        floorHoleMask.FillRect(holeBounds);

        for (int y = holeBounds.yMin; y < holeBounds.yMax; y++)
        {
            for (int x = holeBounds.xMin; x < holeBounds.xMax; x++)
            {
                groundMask.RemoveCell(new Vector2Int(x, y));
            }
        }
    }

    public void AddFloorHoleCell(Vector2Int cell)
    {
        floorHoleMask.AddCell(cell);
        groundMask.RemoveCell(cell);
    }

    public void AddOuterWall(GeneratedWallData wall)
    {
        if (wall != null)
        {
            outerWalls.Add(wall);
        }
    }

    public void AddRoom(GeneratedRoomData room)
    {
        if (room != null)
        {
            rooms.Add(room);
        }
    }

    public void AddDoor(GeneratedDoorData door)
    {
        if (door != null)
        {
            doors.Add(door);
        }
    }

    public void AddStair(GeneratedStairData stair)
    {
        if (stair != null)
        {
            stairs.Add(stair);
        }
    }

    public string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("Floor ");
        builder.Append(floorIndex);
        builder.Append(" id=");
        builder.Append(floorId);
        builder.Append(" bounds=");
        builder.Append(Bounds);
        builder.Append(" groundCells=");
        builder.Append(groundMask.CellCount);
        builder.Append(" holes=");
        builder.Append(floorHoleMask.CellCount);
        builder.Append(" outerWalls=");
        builder.Append(outerWalls.Count);
        builder.Append(" rooms=");
        builder.Append(rooms.Count);
        builder.Append(" doors=");
        builder.Append(doors.Count);
        builder.Append(" stairs=");
        builder.Append(stairs.Count);

        if (stairs.Count > 0)
        {
            builder.Append(" stair[0]=");
            builder.Append(stairs[0].BuildSummary());
        }

        return builder.ToString();
    }
}
