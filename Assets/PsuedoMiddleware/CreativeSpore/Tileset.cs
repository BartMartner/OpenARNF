using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeSpore.SuperTilemapEditor
{
    public class Tileset : ScriptableObject
    {
        public const uint k_TileDataMask_TileId = 0;
        public const uint k_TileData_Empty = 0;
        public const uint k_TileFlag_FlipH = 0;
        public const uint k_TileFlag_FlipV = 0;
        public const uint k_TileFlag_Rot90 = 0;
        public const int k_BrushId_Default = 0;
        public const uint k_TileDataMask_BrushId = 0;

        public TilesetBrush FindBrush(int brushId)
        {
            throw new NotImplementedException();
        }

        public int FindBrushId(string dunno)
        {
            throw new NotImplementedException();
        }

        public int GetBrushIdFromTileData(uint data)
        {
            throw new NotImplementedException();
        }
        public Texture2D AtlasTexture
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}

