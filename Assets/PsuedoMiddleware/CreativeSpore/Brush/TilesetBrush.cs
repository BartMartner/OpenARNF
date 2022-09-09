using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeSpore.SuperTilemapEditor
{
    public class TilesetBrush : ScriptableObject
    {
        public Tileset Tileset;
        public TilemapChunk TilemapChunk;

        public bool IsAnimated()
        {
            throw new NotImplementedException();
        }

        public virtual uint PreviewTileData()
        {
            throw new NotImplementedException();
        }

        public virtual uint[] GetSubtiles(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            throw new NotImplementedException();
        }

        public virtual uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            throw new NotImplementedException();
        }

        public uint RefreshLinkedBrush(STETilemap tilemap, int x, int y, uint id)
        {
            throw new NotImplementedException();
        }
    }
}
