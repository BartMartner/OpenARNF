using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreativeSpore.SuperTilemapEditor;

public class Row4Brush : TilesetBrush
{
    public uint[] TileIds = new uint[] // //NOTE: tileIds now contains tileData, not just tileIds
    {
        Tileset.k_TileData_Empty, // 0
        Tileset.k_TileData_Empty, // 1
        Tileset.k_TileData_Empty, // 2
        Tileset.k_TileData_Empty, // 3
    };

    public override uint PreviewTileData()
    {
        return TileIds[0];
    }

    public override uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData)
    {
        var idx = GetIndexFromGrid(gridX, gridY);
        var brushTileData = RefreshLinkedBrush(tilemap, gridX, gridY, TileIds[idx]);
        brushTileData &= ~Tileset.k_TileDataMask_BrushId;
        brushTileData |= tileData & Tileset.k_TileDataMask_BrushId;
        return brushTileData;
    }

    public override uint[] GetSubtiles(STETilemap tilemap, int gridX, int gridY, uint tileData)
    {
        // Add animated tiles
        {
            var idx = GetIndexFromGrid(gridX, gridY);

            TilesetBrush brush = Tileset.FindBrush(Tileset.GetBrushIdFromTileData(TileIds[idx]));
            if (brush) { brush.GetSubtiles(tilemap, gridX, gridY, tileData); }
            if (brush && brush.IsAnimated())
            {
                TilemapChunk.RegisterAnimatedBrush(brush);
            }
        }
        return null;
    }

    private int GetIndexFromGrid(int gridX, int gridY)
    {
        int idx = 0;

        var g = gridX;
        if (Mathf.Abs(gridY) % 2 == 0) { g -= 2; }
        if (g >= 0)
        {
            idx = g % 4;
        }
        else
        {
            idx = 3 - (Mathf.Abs(g+1) % 4);
        }
        return idx;
    }
}
