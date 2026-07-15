using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class GeneratedBuildingData
{
    [SerializeField] private string buildingId = "generated_house_01";
    [SerializeField] private string displayName = "Generated House 01";
    [SerializeField] private int seed;
    [SerializeField] private Vector2Int footprintOrigin;
    [SerializeField] private Vector2Int footprintSize = new Vector2Int(60, 42);
    [SerializeField] private List<GeneratedFloorData> floors = new List<GeneratedFloorData>();
    [SerializeField] private List<GeneratedEntranceData> entrances = new List<GeneratedEntranceData>();

    public string BuildingId => buildingId;
    public string DisplayName => displayName;
    public int Seed => seed;
    public Vector2Int FootprintOrigin => footprintOrigin;
    public Vector2Int FootprintSize => footprintSize;
    public RectInt Footprint => new RectInt(footprintOrigin, footprintSize);
    public IReadOnlyList<GeneratedFloorData> Floors => floors;
    public IReadOnlyList<GeneratedEntranceData> Entrances => entrances;

    public void Initialize(
        string newBuildingId,
        string newDisplayName,
        int newSeed,
        Vector2Int newFootprintOrigin,
        Vector2Int newFootprintSize
    )
    {
        buildingId = newBuildingId;
        displayName = newDisplayName;
        seed = newSeed;
        footprintOrigin = newFootprintOrigin;
        footprintSize = newFootprintSize;
        floors.Clear();
        entrances.Clear();
    }

    public void AddFloor(GeneratedFloorData floor)
    {
        if (floor != null)
        {
            floors.Add(floor);
        }
    }

    public GeneratedFloorData GetFloor(int floorIndex)
    {
        for (int i = 0; i < floors.Count; i++)
        {
            if (floors[i] != null && floors[i].FloorIndex == floorIndex)
            {
                return floors[i];
            }
        }

        return null;
    }

    public void AddEntrance(GeneratedEntranceData entrance)
    {
        if (entrance != null)
        {
            entrances.Add(entrance);
        }
    }

    public string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Generated Building Data");
        builder.AppendLine($"- Id: {buildingId}");
        builder.AppendLine($"- Name: {displayName}");
        builder.AppendLine($"- Seed: {seed}");
        builder.AppendLine($"- Footprint: {Footprint}");
        builder.AppendLine($"- Entrances: {entrances.Count}");

        for (int i = 0; i < entrances.Count; i++)
        {
            GeneratedEntranceData entrance = entrances[i];

            if (entrance == null)
            {
                builder.AppendLine($"  entrance[{i}] null");
                continue;
            }

            builder.AppendLine(
                $"  entrance[{i}] id={entrance.EntranceId} kind={entrance.EntranceKind} " +
                $"floor={entrance.FloorIndex} cell={entrance.Cell} cells={entrance.Cells.Count} " +
                $"road={entrance.RoadConnectionCell} side={entrance.Side}"
            );
        }

        builder.AppendLine($"- Floors: {floors.Count}");

        for (int i = 0; i < floors.Count; i++)
        {
            if (floors[i] == null)
            {
                builder.AppendLine($"  [{i}] null");
                continue;
            }

            builder.AppendLine($"  [{i}] {floors[i].BuildSummary()}");
        }

        return builder.ToString();
    }

    public string BuildDeterministicFingerprint()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(buildingId);
        builder.Append('|');
        builder.Append(displayName);
        builder.Append('|');
        builder.Append(seed);
        builder.Append('|');
        builder.Append(footprintOrigin.x);
        builder.Append(',');
        builder.Append(footprintOrigin.y);
        builder.Append('|');
        builder.Append(footprintSize.x);
        builder.Append(',');
        builder.Append(footprintSize.y);
        builder.Append("|entrances=");

        for (int i = 0; i < entrances.Count; i++)
        {
            AppendEntranceFingerprint(builder, entrances[i]);
        }

        for (int i = 0; i < floors.Count; i++)
        {
            AppendFloorFingerprint(builder, floors[i]);
        }

        return builder.ToString();
    }

    public int BuildDeterministicHash()
    {
        unchecked
        {
            string fingerprint = BuildDeterministicFingerprint();
            int hash = 17;

            for (int i = 0; i < fingerprint.Length; i++)
            {
                hash = hash * 31 + fingerprint[i];
            }

            return hash;
        }
    }

    private static void AppendFloorFingerprint(StringBuilder builder, GeneratedFloorData floor)
    {
        builder.Append("|floor:");

        if (floor == null)
        {
            builder.Append("null");
            return;
        }

        builder.Append(floor.FloorId);
        builder.Append(',');
        builder.Append(floor.FloorIndex);
        builder.Append(',');
        builder.Append(floor.Bounds);
        builder.Append(",ground=");
        AppendMask(builder, floor.GroundMask);
        builder.Append(",holes=");
        AppendMask(builder, floor.FloorHoleMask);
        builder.Append(",rooms=");

        for (int i = 0; i < floor.Rooms.Count; i++)
        {
            AppendRoomFingerprint(builder, floor.Rooms[i]);
        }

        builder.Append(",doors=");

        for (int i = 0; i < floor.Doors.Count; i++)
        {
            AppendDoorFingerprint(builder, floor.Doors[i]);
        }

        builder.Append(",stairs=");

        for (int i = 0; i < floor.Stairs.Count; i++)
        {
            AppendStairFingerprint(builder, floor.Stairs[i]);
        }
    }

    private static void AppendRoomFingerprint(StringBuilder builder, GeneratedRoomData room)
    {
        builder.Append('[');

        if (room == null)
        {
            builder.Append("null]");
            return;
        }

        builder.Append(room.RoomId);
        builder.Append(',');
        builder.Append(room.Bounds);
        builder.Append(",floor=");
        AppendMask(builder, room.FloorMask);
        builder.Append(",walls=");

        for (int i = 0; i < room.Walls.Count; i++)
        {
            AppendWallFingerprint(builder, room.Walls[i]);
        }

        builder.Append(",doors=");

        for (int i = 0; i < room.Doors.Count; i++)
        {
            AppendDoorFingerprint(builder, room.Doors[i]);
        }

        builder.Append(']');
    }

    private static void AppendWallFingerprint(StringBuilder builder, GeneratedWallData wall)
    {
        builder.Append('(');

        if (wall == null)
        {
            builder.Append("null)");
            return;
        }

        builder.Append(wall.WallId);
        builder.Append(',');
        builder.Append(wall.Layer);
        builder.Append(',');
        builder.Append(wall.Side);
        builder.Append(',');

        for (int i = 0; i < wall.Cells.Count; i++)
        {
            AppendCell(builder, wall.Cells[i].Position);
        }

        builder.Append(')');
    }

    private static void AppendDoorFingerprint(StringBuilder builder, GeneratedDoorData door)
    {
        builder.Append('(');

        if (door == null)
        {
            builder.Append("null)");
            return;
        }

        builder.Append(door.DoorId);
        builder.Append(',');
        builder.Append(door.OwnerId);
        builder.Append(',');
        builder.Append(door.FloorIndex);
        builder.Append(',');
        builder.Append("cells=");

        for (int i = 0; i < door.Cells.Count; i++)
        {
            AppendCell(builder, door.Cells[i].Position);
        }

        builder.Append(',');
        builder.Append(door.WallSide);
        builder.Append(')');
    }

    private static void AppendEntranceFingerprint(StringBuilder builder, GeneratedEntranceData entrance)
    {
        builder.Append('(');

        if (entrance == null)
        {
            builder.Append("null)");
            return;
        }

        builder.Append(entrance.EntranceId);
        builder.Append(',');
        builder.Append(entrance.EntranceKind);
        builder.Append(',');
        builder.Append(entrance.FloorIndex);
        builder.Append(',');
        builder.Append("cells=");

        for (int i = 0; i < entrance.Cells.Count; i++)
        {
            AppendCell(builder, entrance.Cells[i].Position);
        }

        builder.Append(',');
        builder.Append("road=");
        AppendCell(builder, entrance.RoadConnectionCell);
        builder.Append(',');
        builder.Append(entrance.Side);
        builder.Append(')');
    }

    private static void AppendStairFingerprint(StringBuilder builder, GeneratedStairData stair)
    {
        builder.Append('(');

        if (stair == null)
        {
            builder.Append("null)");
            return;
        }

        builder.Append(stair.StairId);
        builder.Append(',');
        builder.Append(stair.LowerFloorIndex);
        builder.Append(',');
        builder.Append(stair.UpperFloorIndex);
        builder.Append(",visual=");
        AppendMask(builder, stair.VisualMask);
        builder.Append(",walkable=");
        AppendMask(builder, stair.WalkableMask);
        builder.Append(",entry=");
        AppendMask(builder, stair.EntryTriggerMask);
        builder.Append(",exit=");
        AppendMask(builder, stair.ExitTriggerMask);
        builder.Append(",leftRail=");
        AppendMask(builder, stair.LeftRailMask);
        builder.Append(",rightRail=");
        AppendMask(builder, stair.RightRailMask);
        builder.Append(')');
    }

    private static void AppendMask(StringBuilder builder, GeneratedCellMask mask)
    {
        if (mask == null)
        {
            builder.Append("null");
            return;
        }

        builder.Append(mask.CellCount);
        builder.Append(':');

        for (int i = 0; i < mask.Cells.Count; i++)
        {
            AppendCell(builder, mask.Cells[i].Position);
        }
    }

    private static void AppendCell(StringBuilder builder, Vector2Int cell)
    {
        builder.Append(cell.x);
        builder.Append('/');
        builder.Append(cell.y);
        builder.Append(';');
    }
}
