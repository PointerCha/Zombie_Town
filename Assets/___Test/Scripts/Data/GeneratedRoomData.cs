using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class GeneratedRoomData
{
    [SerializeField] private string roomId;
    [SerializeField] private string displayName;
    [SerializeField] private GeneratedRoomType roomType;
    [SerializeField] private int floorIndex;
    [SerializeField] private RectInt bounds;
    [SerializeField] private GeneratedCellMask floorMask = new GeneratedCellMask();
    [SerializeField] private List<GeneratedWallData> walls = new List<GeneratedWallData>();
    [SerializeField] private List<GeneratedDoorData> doors = new List<GeneratedDoorData>();

    public string RoomId => roomId;
    public string DisplayName => displayName;
    public GeneratedRoomType RoomType => roomType;
    public int FloorIndex => floorIndex;
    public RectInt Bounds => bounds;
    public GeneratedCellMask FloorMask => floorMask;
    public IReadOnlyList<GeneratedWallData> Walls => walls;
    public IReadOnlyList<GeneratedDoorData> Doors => doors;

    public void Initialize(
        string newRoomId,
        string newDisplayName,
        GeneratedRoomType newRoomType,
        int newFloorIndex,
        RectInt newBounds
    )
    {
        roomId = newRoomId;
        displayName = newDisplayName;
        roomType = newRoomType;
        floorIndex = newFloorIndex;
        bounds = newBounds;

        floorMask.Initialize($"{roomId}_floor", newBounds.position, newBounds.size);
        floorMask.FillRect(newBounds);
        walls.Clear();
        doors.Clear();
    }

    public void AddWall(GeneratedWallData wall)
    {
        if (wall != null)
        {
            walls.Add(wall);
        }
    }

    public void AddDoor(GeneratedDoorData door)
    {
        if (door != null)
        {
            doors.Add(door);
        }
    }

    public bool Contains(Vector2Int cell)
    {
        return floorMask.Contains(cell);
    }

    public string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(displayName);
        builder.Append(" id=");
        builder.Append(roomId);
        builder.Append(" type=");
        builder.Append(roomType);
        builder.Append(" floor=");
        builder.Append(floorIndex);
        builder.Append(" bounds=");
        builder.Append(bounds);
        builder.Append(" floorCells=");
        builder.Append(floorMask.CellCount);
        builder.Append(" walls=");
        builder.Append(walls.Count);
        builder.Append(" doors=");
        builder.Append(doors.Count);
        return builder.ToString();
    }
}
