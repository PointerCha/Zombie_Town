using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class GeneratedTilemapUtility
{
    public static bool UseBatchedPainting { get; set; } = true;

    public static Transform EnsureChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);

        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        child = childObject.transform;
        child.SetParent(parent, false);
        return child;
    }

    public static Tilemap EnsureTilemap(
        Transform target,
        Vector3 cellSize,
        GridLayout.CellLayout cellLayout,
        int sortingOrder,
        bool rendererEnabled = true
    )
    {
        Transform gridHost = target.parent != null ? target.parent : target;
        Grid grid = gridHost.GetComponent<Grid>();

        if (grid == null)
        {
            grid = gridHost.gameObject.AddComponent<Grid>();
        }

        grid.cellSize = cellSize;
        grid.cellLayout = cellLayout;

        Tilemap tilemap = target.GetComponent<Tilemap>();

        if (tilemap == null)
        {
            tilemap = target.gameObject.AddComponent<Tilemap>();
        }

        TilemapRenderer renderer = target.GetComponent<TilemapRenderer>();

        if (renderer == null)
        {
            renderer = target.gameObject.AddComponent<TilemapRenderer>();
        }

        renderer.enabled = rendererEnabled;
        renderer.sortingOrder = sortingOrder;
        renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;

        return tilemap;
    }

    public static Tilemap EnsureColliderTilemap(
        Transform target,
        Vector3 cellSize,
        GridLayout.CellLayout cellLayout,
        bool isTrigger,
        bool useComposite
    )
    {
        Tilemap tilemap = EnsureTilemap(target, cellSize, cellLayout, 0, false);
        TilemapCollider2D tilemapCollider = target.GetComponent<TilemapCollider2D>();

        if (tilemapCollider == null)
        {
            tilemapCollider = target.gameObject.AddComponent<TilemapCollider2D>();
        }

        tilemapCollider.isTrigger = isTrigger;
        tilemapCollider.usedByComposite = useComposite;

        if (!useComposite)
        {
            return tilemap;
        }

        Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();

        if (rigidbody == null)
        {
            rigidbody = target.gameObject.AddComponent<Rigidbody2D>();
        }

        rigidbody.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D compositeCollider = target.GetComponent<CompositeCollider2D>();

        if (compositeCollider == null)
        {
            compositeCollider = target.gameObject.AddComponent<CompositeCollider2D>();
        }

        compositeCollider.isTrigger = isTrigger;
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        return tilemap;
    }

    public static void Clear(Tilemap tilemap)
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }
    }

    public static void ClearTilemapsInChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }

        Tilemap[] tilemaps = root.GetComponentsInChildren<Tilemap>(true);

        for (int i = 0; i < tilemaps.Length; i++)
        {
            Clear(tilemaps[i]);
        }
    }

    public static void PaintCells(Tilemap tilemap, GeneratedCellMask mask, TileBase tile)
    {
        if (tilemap == null || mask == null || tile == null)
        {
            return;
        }

        PaintCells(tilemap, mask.Cells, tile);
    }

    public static void PaintWall(Tilemap tilemap, GeneratedWallData wallData, DirectionalTileSet tileSet)
    {
        if (tilemap == null || wallData == null || tileSet == null)
        {
            return;
        }

        TileBase tile = tileSet.GetTile(wallData.Side);

        if (tile == null)
        {
            return;
        }

        PaintCells(tilemap, wallData.Cells, tile);
    }

    public static void PaintDoor(Tilemap tilemap, GeneratedDoorData doorData, DirectionalTileSet tileSet)
    {
        if (tilemap == null || doorData == null || tileSet == null)
        {
            return;
        }

        TileBase tile = tileSet.GetTile(doorData.WallSide);

        if (tile == null)
        {
            return;
        }

        PaintCells(tilemap, doorData.Cells, tile);
    }

    public static void PaintWallCollision(Tilemap tilemap, GeneratedWallData wallData, TileBase collisionTile)
    {
        if (tilemap == null || wallData == null || collisionTile == null)
        {
            return;
        }

        PaintCells(tilemap, wallData.Cells, collisionTile);
    }

    public static void DestroyGeneratedChild(Transform child)
    {
        if (child == null)
        {
            return;
        }

        GameObject childObject = child.gameObject;
        childObject.SetActive(false);
        child.SetParent(null, false);

        if (Application.isPlaying)
        {
            Object.Destroy(childObject);
        }
        else
        {
            Object.DestroyImmediate(childObject);
        }
    }

    private static void PaintCells(
        Tilemap tilemap,
        IReadOnlyList<GeneratedCell> cells,
        TileBase tile
    )
    {
        int count = cells != null ? cells.Count : 0;

        if (count == 0)
        {
            return;
        }

        if (!UseBatchedPainting)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2Int cell = cells[i].Position;
                tilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), tile);
            }

            tilemap.CompressBounds();
            return;
        }

        Vector3Int[] positions = new Vector3Int[count];
        TileBase[] tiles = new TileBase[count];

        for (int i = 0; i < count; i++)
        {
            Vector2Int cell = cells[i].Position;
            positions[i] = new Vector3Int(cell.x, cell.y, 0);
            tiles[i] = tile;
        }

        tilemap.SetTiles(positions, tiles);
        tilemap.CompressBounds();
    }
}
