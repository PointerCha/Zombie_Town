using System.Text;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewGeneratedRoomLayoutPreset",
    menuName = "Zombie Town/Test Building Generation/Room Layout Preset"
)]
public class GeneratedRoomLayoutPresetAsset : ScriptableObject
{
    [Header("Strategy")]
    [SerializeField] private GeneratedRoomLayoutStrategy layoutStrategy = GeneratedRoomLayoutStrategy.RandomPacked;
    [SerializeField, Min(1)] private int placementAttemptsPerRoom = 32;
    [SerializeField, Min(0)] private int stairClearanceFromRooms = 1;
    [SerializeField, Min(0)] private int entranceClearanceFromRooms = 6;

    [Header("Room Counts")]
    [SerializeField, Min(0)] private int minRoomsPerFloor = 2;
    [SerializeField, Min(0)] private int maxRoomsPerFloor = 2;

    [Header("Room Size")]
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(10, 8);
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(24, 18);

    [Header("Spacing")]
    [SerializeField, Min(0)] private int roomPaddingFromOuterWall = 2;
    [SerializeField, Min(0)] private int minimumRoomGap = 1;

    public GeneratedRoomLayoutStrategy LayoutStrategy => layoutStrategy;
    public int PlacementAttemptsPerRoom => Mathf.Max(1, placementAttemptsPerRoom);
    public int StairClearanceFromRooms => Mathf.Max(0, stairClearanceFromRooms);
    public int EntranceClearanceFromRooms => Mathf.Max(0, entranceClearanceFromRooms);
    public int MinRoomsPerFloor => minRoomsPerFloor;
    public int MaxRoomsPerFloor => maxRoomsPerFloor;
    public Vector2Int MinRoomSize => minRoomSize;
    public Vector2Int MaxRoomSize => maxRoomSize;
    public int RoomPaddingFromOuterWall => roomPaddingFromOuterWall;
    public int MinimumRoomGap => minimumRoomGap;

    public bool IsValid(out string errorMessage)
    {
        StringBuilder builder = new StringBuilder();
        AppendInvalid(builder, minRoomsPerFloor > maxRoomsPerFloor, "Min rooms per floor cannot exceed max rooms per floor.");
        AppendInvalid(builder, placementAttemptsPerRoom <= 0, "Placement attempts per room must be positive.");
        AppendInvalid(builder, minRoomSize.x <= 0 || minRoomSize.y <= 0, "Min room size must be positive.");
        AppendInvalid(builder, maxRoomSize.x < minRoomSize.x || maxRoomSize.y < minRoomSize.y, "Max room size must be greater than or equal to min room size.");

        errorMessage = builder.ToString();
        return errorMessage.Length == 0;
    }

    private static void AppendInvalid(StringBuilder builder, bool invalid, string message)
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
