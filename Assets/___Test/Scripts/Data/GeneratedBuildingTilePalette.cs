using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(
    fileName = "NewGeneratedBuildingTilePalette",
    menuName = "Zombie Town/Test Building Generation/Tile Palette"
)]
public class GeneratedBuildingTilePalette : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string paletteId = "house_01_palette";
    [SerializeField] private string displayName = "House 01 Tile Palette";

    [Header("Floor")]
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase ceilingTile;
    [SerializeField] private Vector2Int ceilingCellOffset = new Vector2Int(3, 3);
    [SerializeField] private TileBase collisionTile;

    [Header("Outer Building Walls")]
    [SerializeField] private DirectionalTileSet outerWallPrimary = new DirectionalTileSet();
    [SerializeField] private DirectionalTileSet outerWallSecondary = new DirectionalTileSet();

    [Header("Room Walls")]
    [SerializeField] private DirectionalTileSet roomWallPrimary = new DirectionalTileSet();
    [SerializeField] private DirectionalTileSet roomWallSecondary = new DirectionalTileSet();

    [Header("Doors")]
    [SerializeField] private DirectionalTileSet doorTiles = new DirectionalTileSet();

    [Header("Stair")]
    [SerializeField] private TileBase stairVisualTile;

    [Header("Default Props")]
    [SerializeField] private TileBase defaultPropTile;

    public string PaletteId => paletteId;
    public string DisplayName => displayName;

    public TileBase GroundTile => groundTile;
    public TileBase CeilingTile => ceilingTile;
    public Vector2Int CeilingCellOffset => ceilingCellOffset;
    public TileBase CollisionTile => collisionTile;
    public DirectionalTileSet OuterWallPrimary => outerWallPrimary;
    public DirectionalTileSet OuterWallSecondary => outerWallSecondary;
    public DirectionalTileSet RoomWallPrimary => roomWallPrimary;
    public DirectionalTileSet RoomWallSecondary => roomWallSecondary;
    public DirectionalTileSet DoorTiles => doorTiles;
    public TileBase StairVisualTile => stairVisualTile;
    public TileBase DefaultPropTile => defaultPropTile;

    public bool IsValid(out string errorMessage)
    {
        StringBuilder builder = new StringBuilder();

        AppendMissing(builder, groundTile == null, "Ground tile");
        AppendMissing(builder, ceilingTile == null, "Ceiling tile");
        AppendMissing(builder, collisionTile == null, "Collision tile");
        AppendMissing(builder, outerWallPrimary == null || !outerWallPrimary.IsComplete(), "Outer wall primary directional set");
        AppendMissing(builder, outerWallSecondary == null || !outerWallSecondary.IsComplete(), "Outer wall secondary directional set");
        AppendMissing(builder, roomWallPrimary == null || !roomWallPrimary.IsComplete(), "Room wall primary directional set");
        AppendMissing(builder, roomWallSecondary == null || !roomWallSecondary.IsComplete(), "Room wall secondary directional set");
        AppendMissing(builder, doorTiles == null || !doorTiles.IsComplete(), "Door directional set");
        AppendMissing(builder, stairVisualTile == null, "Stair visual tile");

        errorMessage = builder.ToString();
        return errorMessage.Length == 0;
    }

    private static void AppendMissing(StringBuilder builder, bool missing, string label)
    {
        if (!missing)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(label);
        builder.Append(" is missing.");
    }
}
