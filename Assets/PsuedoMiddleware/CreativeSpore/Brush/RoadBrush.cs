using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CreativeSpore.SuperTilemapEditor
{

    public class RoadBrush : TilesetBrush
    {
        // '°', '├', '═', '┤', | 0, 2, 10, 8,
        // '┬', '╔', '╦', '╗', | 4, 6, 14, 12,
        // '║', '╠', '╬', '╣', | 5, 7, 15, 13,
        // '┴', '╚', '╩', '╝', | 1, 3, 11, 9,
        public uint[] TileIds = Enumerable.Repeat(Tileset.k_TileData_Empty, 16).ToArray(); //NOTE: tileIds now contains tileData, not just tileIds

        #region IBrush

        public override uint PreviewTileData()
        {
            return TileIds[0];
        }

        public override uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
            bool autotiling_N = AutotileWith(tilemap, brushId, gridX, gridY + 1);
            bool autotiling_E = AutotileWith(tilemap, brushId, gridX + 1, gridY);
            bool autotiling_S = AutotileWith(tilemap, brushId, gridX, gridY - 1);
            bool autotiling_W = AutotileWith(tilemap, brushId, gridX - 1, gridY);

            int idx = 0;
            if (autotiling_N) idx = 1;
            if (autotiling_E) idx |= 2;
            if (autotiling_S) idx |= 4;
            if (autotiling_W) idx |= 8;

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
                int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
                bool autotiling_N = AutotileWith(tilemap, brushId, gridX, gridY + 1);
                bool autotiling_E = AutotileWith(tilemap, brushId, gridX + 1, gridY);
                bool autotiling_S = AutotileWith(tilemap, brushId, gridX, gridY - 1);
                bool autotiling_W = AutotileWith(tilemap, brushId, gridX - 1, gridY);

                int idx = 0;
                if (autotiling_N) idx = 1;
                if (autotiling_E) idx |= 2;
                if (autotiling_S) idx |= 4;
                if (autotiling_W) idx |= 8;

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