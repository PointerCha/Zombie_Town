using System.Text;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewGeneratedEntrancePlacementPreset",
    menuName = "Zombie Town/Test Building Generation/Entrance Placement Preset"
)]
public class GeneratedEntrancePlacementPresetAsset : ScriptableObject
{
    [SerializeField] private GeneratedWallSide entranceSide = GeneratedWallSide.East;
    [SerializeField, Min(1)] private int entranceFloorIndex = 1;
    [SerializeField, Min(1)] private int entranceWidth = 3;
    [SerializeField] private GeneratedEntrancePositionStrategy positionStrategy =
        GeneratedEntrancePositionStrategy.Centered;
    [SerializeField, Min(0)] private int sidePaddingFromCorners = 1;
    [SerializeField, Min(1)] private int roadConnectionOffset = 1;

    public GeneratedWallSide EntranceSide => entranceSide;
    public int EntranceFloorIndex => entranceFloorIndex;
    public int EntranceWidth => entranceWidth;
    public GeneratedEntrancePositionStrategy PositionStrategy => positionStrategy;
    public int SidePaddingFromCorners => sidePaddingFromCorners;
    public int RoadConnectionOffset => roadConnectionOffset;

    public bool IsValid(out string errorMessage)
    {
        StringBuilder builder = new StringBuilder();
        AppendInvalid(builder, entranceFloorIndex <= 0, "Entrance floor index must be positive.");
        AppendInvalid(builder, entranceWidth <= 0, "Entrance width must be positive.");
        AppendInvalid(builder, roadConnectionOffset <= 0, "Road connection offset must be positive.");

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
