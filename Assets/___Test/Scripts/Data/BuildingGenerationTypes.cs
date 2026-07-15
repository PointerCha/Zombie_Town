using System;
using UnityEngine;

public enum GeneratedWallSide
{
    North,
    South,
    East,
    West
}

public enum GeneratedWallLayer
{
    Upper,
    Under
}

public enum GeneratedWallOwner
{
    OuterBuilding,
    Room
}

public enum GeneratedRoomType
{
    None,
    Bedroom,
    LivingRoom,
    Kitchen,
    Storage,
    Shop,
    Workshop,
    Hallway,
    Bathroom,
    Custom
}

public enum GeneratedRoomLayoutStrategy
{
    SectorSplit,
    RandomPacked
}

public enum GeneratedDoorSideSelectionStrategy
{
    FixedDefaultSide,
    PreferDefaultThenAvailable,
    RandomAvailable
}

public enum GeneratedEntrancePositionStrategy
{
    Centered,
    SeededRandomAlongSide
}

public enum GeneratedStairDirection
{
    FloorUp,
    FloorDown
}

public enum GeneratedStairZoneKind
{
    Walkable,
    Entry,
    Exit,
    LeftRail,
    RightRail
}

public enum GeneratedVisibilityGroupKind
{
    Floor,
    Room,
    Ceiling,
    Stair
}

public enum GeneratedEntranceKind
{
    Main,
    Service,
    Emergency
}

public enum GeneratedRuntimeObjectKind
{
    None,
    Building,
    Floor,
    Room,
    Ceiling,
    Stair,
    FloorGroundTrigger,
    FloorUpperWallCollider,
    FloorUnderWallCollider,
    RoomGroundTrigger,
    RoomUpperWallCollider,
    RoomUnderWallCollider
}

[Serializable]
public struct GeneratedCell
{
    [SerializeField] private int x;
    [SerializeField] private int y;

    public int X => x;
    public int Y => y;
    public Vector2Int Position => new Vector2Int(x, y);

    public GeneratedCell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public GeneratedCell(Vector2Int position)
    {
        x = position.x;
        y = position.y;
    }
}
