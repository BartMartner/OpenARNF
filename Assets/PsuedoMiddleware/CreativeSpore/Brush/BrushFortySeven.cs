using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CreativeSpore.SuperTilemapEditor
{
	// Created by Nikola Kasabov and modified by CreativeSpore.
	public class BrushFortySeven : TilesetBrush
    {
        public uint[] TileIds = Enumerable.Repeat(Tileset.k_TileData_Empty, 49).ToArray(); //NOTE: tileIds now contains tileData, not just tileIds

        #region IBrush

        public override uint PreviewTileData()
        {
            return TileIds[0];
        }

        // 128,   1,   2,
        //  64,   0,   4,
        //  32,  16,   8,
        static int[] binaryGrid = new int[]
        {
             28, 124, 112,  20,  84,  80,  16,
             31, 255, 241,  21,  85,  81,  17,
              7, 199, 193,   5,  69,  65,   1,
            247, 215, 223,   4,  68,  64,   0,
            245, 999,  95,  23, 209, 116,  92, // 999 is not used
			253, 125, 127,  29, 113, 197,  71,
            213,  87, 119, 221, 117,  93, 999, // 999 is not used
		};

        int CalculateIndex(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);

            // upper row
            bool autotiling_NW = AutotileWith(tilemap, brushId, gridX - 1, gridY + 1);
            bool autotiling_N = AutotileWith(tilemap, brushId, gridX, gridY + 1);
            bool autotiling_NE = AutotileWith(tilemap, brushId, gridX + 1, gridY + 1);

            // mid row
            bool autotiling_E = AutotileWith(tilemap, brushId, gridX + 1, gridY);
            bool autotiling_W = AutotileWith(tilemap, brushId, gridX - 1, gridY);

            // bottom row
            bool autotiling_SW = AutotileWith(tilemap, brushId, gridX - 1, gridY - 1);
            bool autotiling_S = AutotileWith(tilemap, brushId, gridX, gridY - 1);
            bool autotiling_SE = AutotileWith(tilemap, brushId, gridX + 1, gridY - 1);

            int binIdx = 0;
            // clockwise
            if (autotiling_N) binIdx = 1;
            if (autotiling_N && autotiling_E && autotiling_NE) binIdx |= 2;
            if (autotiling_E) binIdx |= 4;
            if (autotiling_S && autotiling_E && autotiling_SE) binIdx |= 8;
            if (autotiling_S) binIdx |= 16;
            if (autotiling_S && autotiling_W && autotiling_SW) binIdx |= 32;
            if (autotiling_W) binIdx |= 64;
            if (autotiling_N && autotiling_W && autotiling_NW) binIdx |= 128;

            return Array.IndexOf(binaryGrid, binIdx);
        }

        public override uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            int idx = CalculateIndex(tilemap, gridX, gridY, tileData);

            uint brushTileData = RefreshLinkedBrush(tilemap, gridX, gridY, TileIds[idx]);
            // overwrite brush id
            brushTileData &= ~Tileset.k_TileDataMask_BrushId;
            brushTileData |= tileData & Tileset.k_TileDataMask_BrushId;
            return brushTileData;
        }

		public override uint[] GetSubtiles(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            // Add animated tiles
            {				
                int idx = CalculateIndex(tilemap, gridX, gridY, tileData);

                TilesetBrush brush = Tileset.FindBrush(Tileset.GetBrushIdFromTileData(TileIds[idx]));
                if (brush && brush.IsAnimated())
                {
                    TilemapChunk.RegisterAnimatedBrush(brush);
                }
            }
            return null;
        }

        #endregion
    }
}