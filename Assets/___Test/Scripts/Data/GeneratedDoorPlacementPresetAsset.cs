using System.Text;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewGeneratedDoorPlacementPreset",
    menuName = "Zombie Town/Test Building Generation/Door Placement Preset"
)]
public class GeneratedDoorPlacementPresetAsset : ScriptableObject
{
    [SerializeField, Min(1)] private int doorWidth = 3;
    [SerializeField] private GeneratedWallSide defaultDoorSide = GeneratedWallSide.East;
    [SerializeField] private GeneratedDoorSideSelectionStrategy sideSelectionStrategy =
        GeneratedDoorSideSelectionStrategy.PreferDefaultThenAvailable;

    public int DoorWidth => doorWidth;
    public GeneratedWallSide DefaultDoorSide => defaultDoorSide;
    public GeneratedDoorSideSelectionStrategy SideSelectionStrategy => sideSelectionStrategy;

    public bool IsValid(out string errorMessage)
    {
        StringBuilder builder = new StringBuilder();
        AppendInvalid(builder, doorWidth <= 0, "Door width must be positive.");

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
