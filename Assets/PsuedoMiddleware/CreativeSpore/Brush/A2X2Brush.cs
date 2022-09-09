using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{
    //TODO: use this instead CreateAsset method in the Editor class for all assets
    //[CreateAssetMenu(fileName = "New A2X2Brush", menuName = "SuperTilemapEditor/Brush/A2X2Brush")]
    public class A2X2Brush : TilesetBrush
    {
        // '╔', '╗' | 2, 3,
        // '╚', '╝' | 0, 1,
        public uint[] TileIds = new uint[] // //NOTE: tileIds now contains tileData, not just tileIds
    {
        Tileset.k_TileData_Empty, // 3
        Tileset.k_TileData_Empty, // 6
        Tileset.k_TileData_Empty, // 9
        Tileset.k_TileData_Empty, // 12
    };

        #region IBrush

        public override uint PreviewTileData()
        {
            return TileIds[0];
        }

        public override uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
            //NOTE: Now, taking TileIds[0] by default, it means the tile collider will be taken from TileIds[0]
            return (tileData & Tileset.k_TileDataMask_Flags) | ((uint)(brushId << 16) | (TileIds[0] & Tileset.k_TileDataMask_TileId));
        }

        public override uint[] GetSubtiles(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            if (System.Array.IndexOf(TileIds, Tileset.k_TileData_Empty) >= 0)
            {
                return null;
            }

            int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
            bool autotiling_N = AutotileWith(tilemap, brushId, gridX, gridY + 1);
            bool autotiling_E = AutotileWith(tilemap, brushId, gridX + 1, gridY);
            bool autotiling_S = AutotileWith(tilemap, brushId, gridX, gridY - 1);
            bool autotiling_W = AutotileWith(tilemap, brushId, gridX - 1, gridY);

            // diagonals
            bool autotiling_NE = AutotileWith(tilemap, brushId, gridX + 1, gridY + 1);
            bool autotiling_SE = AutotileWith(tilemap, brushId, gridX + 1, gridY - 1);
            bool autotiling_SW = AutotileWith(tilemap, brushId, gridX - 1, gridY - 1);
            bool autotiling_NW = AutotileWith(tilemap, brushId, gridX - 1, gridY + 1);

            uint[] subTileData = new uint[4];
            subTileData[0] = (autotiling_SW && autotiling_S && autotiling_W) ? TileIds[3] : TileIds[0];
            subTileData[1] = (autotiling_SE && autotiling_S && autotiling_E) ? TileIds[2] : TileIds[1];
            subTileData[2] = (autotiling_NW && autotiling_N && autotiling_W) ? TileIds[1] : TileIds[2];
            subTileData[3] = (autotiling_NE && autotiling_N && autotiling_E) ? TileIds[0] : TileIds[3];

            return subTileData;
        }

        #endregion
    }
}