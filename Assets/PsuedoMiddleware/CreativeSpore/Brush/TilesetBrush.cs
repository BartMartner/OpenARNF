using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{
    [System.Flags]
    public enum eAutotilingMode
    {
        /// <summary>
        /// Autotiles with brushes of the same type
        /// </summary>
        Self = 1,
        /// <summary>
        /// Autotiles  with tiles of different brush or any tile but not with empty cells
        /// </summary>
        Other = 1 << 1,
        /// <summary>
        /// Checks the Group Autotiling Mask to see if the relation between this brush and the neighbor brush is checked to do the autotiling
        /// </summary>
        Group = 1 << 2,
        /// <summary>
        /// Autotiles  with empty cells
        /// </summary>
        EmptyCells = 1 << 3,
        /// <summary>
        /// Autotiles with cells placed out of the tilemap bounds
        /// </summary>
        TilemapBounds = 1 << 4,
        /// <summary>
        /// Avoid autotiling with other brushes
        /// </summary>
        SkipOtherBrushes = 1 << 5,
    }

    public class TilesetBrush : ScriptableObject, IBrush
    {
        public Tileset Tileset;
        public ParameterContainer Params = new ParameterContainer();
        public int Group { get { return m_group; } }
        public eAutotilingMode AutotilingMode { get { return m_autotilingMode; } }
        public bool ShowInPalette { get { return m_showInPalette; } set { m_showInPalette = value; } }

        [SerializeField]
        private int m_group = 0;        
        [SerializeField]
        private eAutotilingMode m_autotilingMode = eAutotilingMode.Self;
        [SerializeField]
        private bool m_showInPalette = true;


        public bool AutotileWith(int selfBrushId, int otherBrushId)
        {
            if (
                ((AutotilingMode & eAutotilingMode.Self) != 0 && selfBrushId == otherBrushId) ||
                ((AutotilingMode & eAutotilingMode.Other) != 0 && otherBrushId != selfBrushId && otherBrushId != (Tileset.k_TileDataMask_BrushId >> 16))
            )
            {
                return true;
            }
            if ((AutotilingMode & eAutotilingMode.Group) != 0)
            {
                TilesetBrush brush = Tileset.FindBrush(otherBrushId);
                if (brush)
                    return Tileset.GetGroupAutotiling(Group, brush.Group);
                else if (otherBrushId == Tileset.k_BrushId_Default)
                    return Tileset.GetGroupAutotiling(Group, 0); //with normal tiles, use default group (0) //TODO: this is old code, now it is possible to change tile group. Check if this method is useful after removing it from TilemapChunk
            }
            return false;
        }

        public bool AutotileWith(STETilemap tilemap, int selfBrushId, int gridX, int gridY)
        {
            bool isOutOfBounds = gridX > tilemap.MaxGridX || gridX < tilemap.MinGridX || gridY > tilemap.MaxGridY || gridY < tilemap.MinGridY;
            if ((AutotilingMode & eAutotilingMode.TilemapBounds) != 0 && isOutOfBounds) return true;
            uint otherTileData = tilemap.GetTileData(gridX, gridY);
            return AutotileWith(tilemap.Tileset, selfBrushId, otherTileData);
        }

        public bool AutotileWith(Tileset tileset, int selfBrushId, uint otherTileData)
        {
            int otherBrushId = (int)((uint)(otherTileData & Tileset.k_TileDataMask_BrushId) >> 16);
            if (otherBrushId != Tileset.k_BrushId_Default && otherBrushId != selfBrushId && (AutotilingMode & eAutotilingMode.SkipOtherBrushes) != 0)
                return false;
            if ((AutotilingMode & eAutotilingMode.EmptyCells) != 0 && otherTileData == Tileset.k_TileData_Empty) return true;
            if ((AutotilingMode & eAutotilingMode.Group) != 0)
            {
                Tile tile = tileset.GetTile((int)(otherTileData & Tileset.k_TileDataMask_TileId));
                if (tile != null && Tileset.GetGroupAutotiling(Group, tile.autilingGroup)) return true;
            }
            return AutotileWith(selfBrushId, otherBrushId);
        }

        protected static bool s_refreshingLinkedBrush = false; //avoid infinite loop
        /// <summary>
        /// Returns a new tiledata after calling the refresh method of the linked brush. 
        /// This is used to give support for tiles in a brush that are linked to another brush. Ex: a Road Brush with a tile linked to a random brush.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="tileData"></param>
        /// <returns></returns>
        public uint RefreshLinkedBrush(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            if (s_refreshingLinkedBrush) return tileData;

            int brushId = Tileset.GetBrushIdFromTileData(tileData);
            TilesetBrush brush = Tileset.FindBrush(brushId);
            if (brush)
            {
                s_refreshingLinkedBrush = true;
                tileData = ApplyAndMergeTileFlags(brush.Refresh(tilemap, gridX, gridY, tileData), tileData);
                s_refreshingLinkedBrush = false;
            }
            return tileData;
        }

        /// <summary>
        /// Merge flags and keeps rotation coherence
        /// </summary>
        /// <returns></returns>
        public static uint ApplyAndMergeTileFlags(uint tileData, uint tileDataFlags)
        {
            tileDataFlags &= Tileset.k_TileDataMask_Flags;
            if ((tileData & Tileset.k_TileFlag_Rot90) != 0)
            {
                if ((tileDataFlags & Tileset.k_TileFlag_FlipH) != 0) tileData ^= Tileset.k_TileFlag_FlipV;
                if ((tileDataFlags & Tileset.k_TileFlag_FlipV) != 0) tileData ^= Tileset.k_TileFlag_FlipH;
                if ((tileDataFlags & Tileset.k_TileFlag_Rot90) != 0)
                {
                    tileData ^= 0xE0000000;
                }
            }
            else
            {
                tileData ^= tileDataFlags;
            }
            return tileData;
        }

        #region IBrush

        public virtual uint PreviewTileData()
        {
            return Tileset.k_TileData_Empty;
        }

        public virtual uint OnPaint(TilemapChunk chunk, int chunkGx, int chunkGy, uint tileData)
        {
            return tileData;
        }

        public virtual void OnErase(TilemapChunk chunk, int chunkGx, int chunkGy, uint tileData, int brushId)
        {
            ;
        }

        public virtual uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            return tileData;
        }

        public virtual bool IsAnimated()
        {
            return false;
        }

        public virtual Rect GetAnimUV()
        {
            int tileId = (int)(PreviewTileData() & Tileset.k_TileDataMask_TileId);
            return Tileset && tileId != Tileset.k_TileId_Empty ? Tileset.Tiles[tileId].uv : default(Rect);
        }

        public virtual int GetAnimFrameIdx()
        {
            return 0;
        }

        Vector2[] m_uvWithFlags = new Vector2[4];
        int m_lastFrameToken;
        public virtual Vector2[] GetAnimUVWithFlags(float innerPadding = 0f)
        {
            if (GetAnimFrameIdx() == m_lastFrameToken)
                return m_uvWithFlags;
            else
                m_lastFrameToken = GetAnimFrameIdx();

            uint tileData = GetAnimTileData();
            Rect tileUV = GetAnimUV();

            bool flipH = (tileData & Tileset.k_TileFlag_FlipH) != 0;
            bool flipV = (tileData & Tileset.k_TileFlag_FlipV) != 0;
            bool rot90 = (tileData & Tileset.k_TileFlag_Rot90) != 0;

            //NOTE: xMinMax and yMinMax is opposite if width or height is negative
            float u0 = tileUV.xMin + Tileset.AtlasTexture.texelSize.x * innerPadding;
            float v0 = tileUV.yMin + Tileset.AtlasTexture.texelSize.y * innerPadding;
            float u1 = tileUV.xMax - Tileset.AtlasTexture.texelSize.x * innerPadding;
            float v1 = tileUV.yMax - Tileset.AtlasTexture.texelSize.y * innerPadding;

            if (flipV)
            {
                float v = v0;
                v0 = v1;
                v1 = v;
            }
            if (flipH)
            {
                float u = u0;
                u0 = u1;
                u1 = u;
            }

            if (rot90)
            {
                m_uvWithFlags[0] = new Vector2(u1, v0);
                m_uvWithFlags[1] = new Vector2(u1, v1);
                m_uvWithFlags[2] = new Vector2(u0, v0);
                m_uvWithFlags[3] = new Vector2(u0, v1);
            }
            else
            {
                m_uvWithFlags[0] = new Vector2(u0, v0);
                m_uvWithFlags[1] = new Vector2(u1, v0);
                m_uvWithFlags[2] = new Vector2(u0, v1);
                m_uvWithFlags[3] = new Vector2(u1, v1);
            }
            return m_uvWithFlags;
        }

        public virtual uint GetAnimTileData()
        {
            return PreviewTileData();
        }

        public virtual uint[] GetSubtiles(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            return null;
        }

        public virtual Vector2[] GetMergedSubtileColliderVertices(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            return null;
        }

        #endregion        
    }
}
