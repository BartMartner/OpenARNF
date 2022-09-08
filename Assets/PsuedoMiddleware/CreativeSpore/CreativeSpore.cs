using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    [Serializable]
    public class Tile
    {
        public Rect uv;
        public TileColliderData collData;
        public ParameterContainer paramContainer = new ParameterContainer();
    }

    public class ParameterContainer
    {
        public bool GetBoolParam(string id)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public struct TileColliderData
    {
        /// <summary>
        /// The collider vertex array. Only valid for type eTileCollider.Polygon, for other types use GetVertices method.
        /// </summary>
        public Vector2[] vertices;
        public eTileCollider type;

        public Vector2[] GetVertices()
        {
            throw new NotImplementedException();
        }

        public static Vector2 SnapVertex(Vector2 vertex, Tileset tileset)
        {
            throw new NotImplementedException();
        }

        public void SnapVertices(Tileset tileset)
        {
            throw new NotImplementedException();
        }

        public void ApplyFlippingFlags(uint tileData)
        {
            throw new NotImplementedException();
        }

        public void RemoveFlippingFlags(uint tileData)
        {
            throw new NotImplementedException();
        }

        public void FlipH()
        {
            throw new NotImplementedException();
        }

        public void FlipV()
        {
            throw new NotImplementedException();
        }

        public void Rot90()
        {
            throw new NotImplementedException();
        }

        public void Rot90Back()
        {
            throw new NotImplementedException();
        }
    }



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

    public class TilemapUtils
    {
        public static Vector2 GetGridWorldPos(STETilemap tilemap, int x, int y)
        {
            throw new NotImplementedException();
        }
    }

    public class RandomBrush : TilesetBrush
    {
        public List<RandomTileData> RandomTileList
        {
            get { throw new NotImplementedException(); }
        }
    }

    [System.Serializable]
    public class RandomTileData
    {
        public uint tileData;
    }

    [System.Flags]
    public enum eTileFlags
    {
        None = 0,
        Updated = 1,
        Rot90 = 2,
        FlipV = 4,
        FlipH = 8,
    }

    public enum eColliderType
    {
        None,
        _2D,
        _3D
    };

    public enum e2DColliderType
    {
        EdgeCollider2D,
        PolygonCollider2D
    };

    public enum eTileCollider
    {
        None,
        Polygon,
    }
}