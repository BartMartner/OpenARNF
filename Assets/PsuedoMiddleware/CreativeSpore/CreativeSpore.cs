using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CreativeSpore.SuperTilemapEditor
{
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

    public class TilemapUtils
    {
        public static Vector2 GetGridWorldPos(STETilemap tilemap, int x, int y)
        {
            throw new NotImplementedException();
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