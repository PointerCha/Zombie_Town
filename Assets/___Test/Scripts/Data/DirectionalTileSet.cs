using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class DirectionalTileSet
{
    [SerializeField] private TileBase north;
    [SerializeField] private TileBase south;
    [SerializeField] private TileBase east;
    [SerializeField] private TileBase west;

    public TileBase North => north;
    public TileBase South => south;
    public TileBase East => east;
    public TileBase West => west;

    public TileBase GetTile(GeneratedWallSide side)
    {
        switch (side)
        {
            case GeneratedWallSide.North:
                return north;

            case GeneratedWallSide.South:
                return south;

            case GeneratedWallSide.East:
                return east;

            case GeneratedWallSide.West:
                return west;

            default:
                return null;
        }
    }

    public bool IsComplete()
    {
        return north != null &&
               south != null &&
               east != null &&
               west != null;
    }
}
