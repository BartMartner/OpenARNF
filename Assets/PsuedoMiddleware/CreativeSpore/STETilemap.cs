using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeSpore.SuperTilemapEditor
{
    public class STETilemap : MonoBehaviour
    {
        public delegate void OnTileChangedDelegate(STETilemap tilemap, int gridX, int gridY, uint tileData);
        public OnTileChangedDelegate OnTileChanged;

        public Tileset Tileset
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Vector2 CellSize
        {
            get { throw new NotImplementedException(); }
        }

        public eColliderType ColliderType
        {
            get { throw new NotImplementedException(); }
        }

        public int MinGridX
        {
            get { throw new NotImplementedException(); }
        }

        public int MaxGridX
        {
            get { throw new NotImplementedException(); }
        }

        public int MinGridY
        {
            get { throw new NotImplementedException(); }
        }

        public int MaxGridY
        {
            get { throw new NotImplementedException(); }
        }

        public bool AutoTrim
        {
            get { throw new NotImplementedException(); }
        }

        public Color TintColor
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public Material Material
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public bool PixelSnap
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string SortingLayerName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int OrderInLayer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public e2DColliderType Collider2DType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void SetMapBounds(int minX, int minY, int maxX, int maxY)
        {
            throw new NotImplementedException();
        }

        public TilesetBrush GetBrush(int x, int y)
        {
            throw new NotImplementedException();
        }

        public Tile GetTile(int x, int y)
        {
            throw new NotImplementedException();
        }

        public Tile GetTile(Vector3 p)
        {
            throw new NotImplementedException();
        }

        public void SetTileData(int x, int y, uint data)
        {
            throw new NotImplementedException();
        }

        public void SetTileData(Vector2 pos, uint data)
        {
            throw new NotImplementedException();
        }

        public uint GetTileData(Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public uint GetTileData(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void UpdateMesh()
        {
            throw new NotImplementedException();
        }

        public void Refresh(bool someBool = false)
        {
            throw new NotImplementedException();
        }

        public void Erase(Vector3 p)
        {
            throw new NotImplementedException();
        }

        public void Trim()
        {
            throw new NotImplementedException();
        }

        public void SetTile(Vector2 vLocalPos, int tileId, int brushId = Tileset.k_BrushId_Default, eTileFlags flags = eTileFlags.None)
        {
            throw new NotImplementedException();
        }

        public void SetTile(int gridX, int gridY, int tileId, int brushId = Tileset.k_BrushId_Default, eTileFlags flags = eTileFlags.None)
        {
            throw new NotImplementedException();
        }
    }
}