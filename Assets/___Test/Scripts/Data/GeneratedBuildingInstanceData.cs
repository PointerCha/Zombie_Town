using System;
using System.Text;
using UnityEngine;

[Serializable]
public class GeneratedBuildingInstanceData
{
    [SerializeField] private string instanceId;
    [SerializeField] private string buildingId;
    [SerializeField] private string profileId;
    [SerializeField] private string displayName;
    [SerializeField] private int seed;
    [SerializeField] private int deterministicHash;
    [SerializeField] private RectInt footprint;
    [SerializeField] private RectInt occupiedBounds;
    [SerializeField] private int floorCount;
    [SerializeField] private int roomCount;
    [SerializeField] private int stairCount;
    [SerializeField] private int entranceCount;
    [SerializeField] private bool hasStairs;
    [SerializeField] private string primaryEntranceId;
    [SerializeField] private GeneratedEntranceKind primaryEntranceKind;
    [SerializeField] private int primaryEntranceFloorIndex;
    [SerializeField] private Vector2Int primaryEntranceCell;
    [SerializeField] private Vector2Int primaryRoadConnectionCell;
    [SerializeField] private GeneratedWallSide primaryEntranceSide;

    public string InstanceId => instanceId;
    public string BuildingId => buildingId;
    public string ProfileId => profileId;
    public string DisplayName => displayName;
    public int Seed => seed;
    public int DeterministicHash => deterministicHash;
    public RectInt Footprint => footprint;
    public RectInt OccupiedBounds => occupiedBounds;
    public int FloorCount => floorCount;
    public int RoomCount => roomCount;
    public int StairCount => stairCount;
    public int EntranceCount => entranceCount;
    public bool HasStairs => hasStairs;
    public string PrimaryEntranceId => primaryEntranceId;
    public GeneratedEntranceKind PrimaryEntranceKind => primaryEntranceKind;
    public int PrimaryEntranceFloorIndex => primaryEntranceFloorIndex;
    public Vector2Int PrimaryEntranceCell => primaryEntranceCell;
    public Vector2Int PrimaryRoadConnectionCell => primaryRoadConnectionCell;
    public GeneratedWallSide PrimaryEntranceSide => primaryEntranceSide;

    public void Initialize(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile
    )
    {
        if (buildingData == null)
        {
            Clear();
            return;
        }

        instanceId = $"{buildingData.BuildingId}_{buildingData.Seed}";
        buildingId = buildingData.BuildingId;
        profileId = profile != null ? profile.BuildingId : buildingData.BuildingId;
        displayName = buildingData.DisplayName;
        seed = buildingData.Seed;
        deterministicHash = buildingData.BuildDeterministicHash();
        footprint = buildingData.Footprint;
        occupiedBounds = buildingData.Footprint;
        floorCount = buildingData.Floors.Count;
        entranceCount = buildingData.Entrances.Count;
        roomCount = CountRooms(buildingData);
        stairCount = CountUniqueStairs(buildingData);
        hasStairs = stairCount > 0;
        ApplyPrimaryEntrance(buildingData);
    }

    public string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Generated Building Instance Data");
        builder.AppendLine($"- Instance Id: {instanceId}");
        builder.AppendLine($"- Building Id: {buildingId}");
        builder.AppendLine($"- Profile Id: {profileId}");
        builder.AppendLine($"- Seed: {seed}");
        builder.AppendLine($"- Hash: {deterministicHash}");
        builder.AppendLine($"- Footprint: {footprint}");
        builder.AppendLine($"- Occupied Bounds: {occupiedBounds}");
        builder.AppendLine($"- Floors: {floorCount}");
        builder.AppendLine($"- Rooms: {roomCount}");
        builder.AppendLine($"- Stairs: {stairCount}");
        builder.AppendLine($"- Entrances: {entranceCount}");
        builder.AppendLine(
            $"- Primary Entrance: id={primaryEntranceId} kind={primaryEntranceKind} " +
            $"floor={primaryEntranceFloorIndex} cell={primaryEntranceCell} side={primaryEntranceSide}"
        );
        builder.AppendLine($"- Road Connection Cell: {primaryRoadConnectionCell}");
        return builder.ToString();
    }

    private void ApplyPrimaryEntrance(GeneratedBuildingData buildingData)
    {
        GeneratedEntranceData entrance = buildingData.Entrances.Count > 0
            ? buildingData.Entrances[0]
            : null;

        if (entrance == null)
        {
            primaryEntranceId = string.Empty;
            primaryEntranceKind = GeneratedEntranceKind.Main;
            primaryEntranceFloorIndex = 0;
            primaryEntranceCell = Vector2Int.zero;
            primaryRoadConnectionCell = Vector2Int.zero;
            primaryEntranceSide = GeneratedWallSide.East;
            return;
        }

        primaryEntranceId = entrance.EntranceId;
        primaryEntranceKind = entrance.EntranceKind;
        primaryEntranceFloorIndex = entrance.FloorIndex;
        primaryEntranceCell = entrance.Cell;
        primaryEntranceSide = entrance.Side;
        primaryRoadConnectionCell = entrance.RoadConnectionCell;
    }

    private void Clear()
    {
        instanceId = string.Empty;
        buildingId = string.Empty;
        profileId = string.Empty;
        displayName = string.Empty;
        seed = 0;
        deterministicHash = 0;
        footprint = default;
        occupiedBounds = default;
        floorCount = 0;
        roomCount = 0;
        stairCount = 0;
        entranceCount = 0;
        hasStairs = false;
        primaryEntranceId = string.Empty;
        primaryEntranceKind = GeneratedEntranceKind.Main;
        primaryEntranceFloorIndex = 0;
        primaryEntranceCell = Vector2Int.zero;
        primaryRoadConnectionCell = Vector2Int.zero;
        primaryEntranceSide = GeneratedWallSide.East;
    }

    private static int CountRooms(GeneratedBuildingData buildingData)
    {
        int count = 0;

        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floor = buildingData.Floors[i];

            if (floor != null)
            {
                count += floor.Rooms.Count;
            }
        }

        return count;
    }

    private static int CountUniqueStairs(GeneratedBuildingData buildingData)
    {
        int count = 0;

        for (int i = 0; i < buildingData.Floors.Count; i++)
        {
            GeneratedFloorData floor = buildingData.Floors[i];

            if (floor == null)
            {
                continue;
            }

            for (int stairIndex = 0; stairIndex < floor.Stairs.Count; stairIndex++)
            {
                GeneratedStairData stair = floor.Stairs[stairIndex];

                if (stair != null && stair.LowerFloorIndex == floor.FloorIndex)
                {
                    count++;
                }
            }
        }

        return count;
    }

}
