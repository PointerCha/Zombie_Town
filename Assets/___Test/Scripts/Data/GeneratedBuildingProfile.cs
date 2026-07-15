using System.Text;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewGeneratedBuildingProfile",
    menuName = "Zombie Town/Test Building Generation/Building Profile"
)]
public class GeneratedBuildingProfile : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string buildingId = "house_01_like";
    [SerializeField] private string displayName = "House 01 Like";

    [Header("Tile Palette")]
    [SerializeField] private GeneratedBuildingTilePalette tilePalette;

    [Header("Footprint")]
    [SerializeField] private Vector2Int footprintOrigin = new Vector2Int(-26, -9);
    [SerializeField] private Vector2Int footprintSize = new Vector2Int(60, 42);
    [SerializeField, Min(1)] private int floorCount = 2;
    [SerializeField] private Vector3 floorVisualOffsetStep = new Vector3(0f, 1.5f, 0f);

    [Header("Rooms")]
    [SerializeField] private GeneratedRoomLayoutPresetAsset roomLayoutPreset;
    [SerializeField, Min(0)] private int minRoomsPerFloor = 2;
    [SerializeField, Min(0)] private int maxRoomsPerFloor = 2;
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(10, 8);
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(24, 18);
    [SerializeField, Min(0)] private int roomPaddingFromOuterWall = 2;
    [SerializeField, Min(0)] private int minimumRoomGap = 1;

    [Header("Doors")]
    [SerializeField] private GeneratedDoorPlacementPresetAsset doorPlacementPreset;
    [SerializeField, Min(1)] private int defaultRoomDoorWidth = 3;
    [SerializeField] private GeneratedWallSide defaultRoomDoorSide = GeneratedWallSide.East;
    [SerializeField] private GeneratedDoorSideSelectionStrategy defaultRoomDoorSideSelectionStrategy =
        GeneratedDoorSideSelectionStrategy.PreferDefaultThenAvailable;

    [Header("Entrance")]
    [SerializeField] private GeneratedEntrancePlacementPresetAsset entrancePlacementPreset;
    [SerializeField] private GeneratedWallSide defaultEntranceSide = GeneratedWallSide.East;
    [SerializeField] private int defaultEntranceFloorIndex = 1;
    [SerializeField, Min(1)] private int defaultEntranceWidth = 3;
    [SerializeField] private GeneratedEntrancePositionStrategy defaultEntrancePositionStrategy =
        GeneratedEntrancePositionStrategy.Centered;
    [SerializeField, Min(0)] private int defaultEntranceSidePaddingFromCorners = 1;
    [SerializeField, Min(1)] private int defaultEntranceRoadConnectionOffset = 1;

    [Header("Stair")]
    [SerializeField] private bool generateStairs = true;
    [SerializeField] private RectInt defaultStairVisualBounds = new RectInt(-13, 0, 5, 6);
    [SerializeField] private RectInt defaultUpperFloorHoleBounds = new RectInt(-15, 0, 6, 5);
    [SerializeField] private GeneratedStairModulePresetAsset stairModulePresetAsset;
    [SerializeField] private GeneratedStairModulePreset stairModulePreset = new GeneratedStairModulePreset();

    [Header("Grid / Sorting")]
    [SerializeField] private GeneratedBuildingGridProfile gridProfile = new GeneratedBuildingGridProfile();
    [SerializeField] private GeneratedBuildingSortingProfile sortingProfile = new GeneratedBuildingSortingProfile();

    public string BuildingId => buildingId;
    public string DisplayName => displayName;
    public GeneratedBuildingTilePalette TilePalette => tilePalette;
    public Vector2Int FootprintOrigin => footprintOrigin;
    public Vector2Int FootprintSize => footprintSize;
    public int FloorCount => floorCount;
    public Vector3 FloorVisualOffsetStep => floorVisualOffsetStep;
    public GeneratedRoomLayoutStrategy RoomLayoutStrategy => roomLayoutPreset != null ? roomLayoutPreset.LayoutStrategy : GeneratedRoomLayoutStrategy.SectorSplit;
    public int RoomPlacementAttemptsPerRoom => roomLayoutPreset != null ? roomLayoutPreset.PlacementAttemptsPerRoom : 32;
    public int StairClearanceFromRooms => roomLayoutPreset != null ? roomLayoutPreset.StairClearanceFromRooms : 0;
    public int EntranceClearanceFromRooms => roomLayoutPreset != null ? roomLayoutPreset.EntranceClearanceFromRooms : 0;
    public int MinRoomsPerFloor => roomLayoutPreset != null ? roomLayoutPreset.MinRoomsPerFloor : minRoomsPerFloor;
    public int MaxRoomsPerFloor => roomLayoutPreset != null ? roomLayoutPreset.MaxRoomsPerFloor : maxRoomsPerFloor;
    public Vector2Int MinRoomSize => roomLayoutPreset != null ? roomLayoutPreset.MinRoomSize : minRoomSize;
    public Vector2Int MaxRoomSize => roomLayoutPreset != null ? roomLayoutPreset.MaxRoomSize : maxRoomSize;
    public int RoomPaddingFromOuterWall => roomLayoutPreset != null ? roomLayoutPreset.RoomPaddingFromOuterWall : roomPaddingFromOuterWall;
    public int MinimumRoomGap => roomLayoutPreset != null ? roomLayoutPreset.MinimumRoomGap : minimumRoomGap;
    public int DefaultRoomDoorWidth => doorPlacementPreset != null ? doorPlacementPreset.DoorWidth : defaultRoomDoorWidth;
    public GeneratedWallSide DefaultRoomDoorSide => doorPlacementPreset != null ? doorPlacementPreset.DefaultDoorSide : defaultRoomDoorSide;
    public GeneratedDoorSideSelectionStrategy RoomDoorSideSelectionStrategy => doorPlacementPreset != null
        ? doorPlacementPreset.SideSelectionStrategy
        : defaultRoomDoorSideSelectionStrategy;
    public GeneratedWallSide DefaultEntranceSide => entrancePlacementPreset != null ? entrancePlacementPreset.EntranceSide : defaultEntranceSide;
    public int DefaultEntranceFloorIndex => entrancePlacementPreset != null ? entrancePlacementPreset.EntranceFloorIndex : defaultEntranceFloorIndex;
    public int DefaultEntranceWidth => entrancePlacementPreset != null ? entrancePlacementPreset.EntranceWidth : defaultEntranceWidth;
    public GeneratedEntrancePositionStrategy DefaultEntrancePositionStrategy => entrancePlacementPreset != null
        ? entrancePlacementPreset.PositionStrategy
        : defaultEntrancePositionStrategy;
    public int DefaultEntranceSidePaddingFromCorners => entrancePlacementPreset != null
        ? entrancePlacementPreset.SidePaddingFromCorners
        : defaultEntranceSidePaddingFromCorners;
    public int DefaultEntranceRoadConnectionOffset => entrancePlacementPreset != null
        ? entrancePlacementPreset.RoadConnectionOffset
        : defaultEntranceRoadConnectionOffset;
    public bool GenerateStairs => generateStairs;
    public RectInt DefaultStairVisualBounds => defaultStairVisualBounds;
    public RectInt DefaultUpperFloorHoleBounds => defaultUpperFloorHoleBounds;
    public GeneratedStairModulePreset StairModulePreset => stairModulePresetAsset != null ? stairModulePresetAsset.Preset : stairModulePreset;
    public GeneratedBuildingGridProfile GridProfile => gridProfile;
    public GeneratedBuildingSortingProfile SortingProfile => sortingProfile;

    public Vector3 GetFloorLocalPosition(int zeroBasedFloorIndex)
    {
        return floorVisualOffsetStep * Mathf.Max(0, zeroBasedFloorIndex);
    }

    public bool IsValid(out string errorMessage)
    {
        StringBuilder builder = new StringBuilder();

        if (tilePalette == null)
        {
            builder.Append("Tile palette is missing.");
        }
        else if (!tilePalette.IsValid(out string paletteError))
        {
            builder.Append(paletteError);
        }

        AppendMissing(builder, footprintSize.x <= 0 || footprintSize.y <= 0, "Footprint size must be positive.");
        AppendMissing(builder, floorCount <= 0, "Floor count must be positive.");
        AppendMissing(builder, MinRoomsPerFloor > MaxRoomsPerFloor, "Min rooms per floor cannot exceed max rooms per floor.");
        AppendMissing(builder, MinRoomSize.x <= 0 || MinRoomSize.y <= 0, "Min room size must be positive.");
        AppendMissing(builder, MaxRoomSize.x < MinRoomSize.x || MaxRoomSize.y < MinRoomSize.y, "Max room size must be greater than or equal to min room size.");
        AppendMissing(builder, RoomPaddingFromOuterWall * 2 >= footprintSize.x, "Room padding is too large for footprint width.");
        AppendMissing(builder, RoomPaddingFromOuterWall * 2 >= footprintSize.y, "Room padding is too large for footprint height.");
        AppendMissing(builder, DefaultRoomDoorWidth <= 0, "Default room door width must be positive.");
        AppendMissing(builder, DefaultEntranceFloorIndex <= 0 || DefaultEntranceFloorIndex > floorCount, "Default entrance floor index is outside generated floor range.");
        AppendMissing(builder, DefaultEntranceWidth <= 0, "Default entrance width must be positive.");
        AppendMissing(builder, DefaultEntranceRoadConnectionOffset <= 0, "Default entrance road connection offset must be positive.");

        if (roomLayoutPreset != null && !roomLayoutPreset.IsValid(out string roomPresetError))
        {
            AppendMissing(builder, true, roomPresetError);
        }

        if (doorPlacementPreset != null && !doorPlacementPreset.IsValid(out string doorPresetError))
        {
            AppendMissing(builder, true, doorPresetError);
        }

        if (entrancePlacementPreset != null && !entrancePlacementPreset.IsValid(out string entrancePresetError))
        {
            AppendMissing(builder, true, entrancePresetError);
        }

        if (generateStairs)
        {
            GeneratedStairModulePreset resolvedStairPreset = StairModulePreset;

            if (resolvedStairPreset == null)
            {
                AppendMissing(builder, true, "Stair module preset is missing.");
            }
            else if (!resolvedStairPreset.IsValid(out string stairPresetError))
            {
                AppendMissing(builder, true, stairPresetError);
            }

            if (stairModulePresetAsset != null && !stairModulePresetAsset.IsValid(out string stairPresetAssetError))
            {
                AppendMissing(builder, true, stairPresetAssetError);
            }
        }

        errorMessage = builder.ToString();
        return errorMessage.Length == 0;
    }

    private static void AppendMissing(StringBuilder builder, bool invalid, string message)
    {
        if (!invalid)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(message);
    }
}
