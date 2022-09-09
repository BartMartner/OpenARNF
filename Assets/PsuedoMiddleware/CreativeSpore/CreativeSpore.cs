using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CreativeSpore.SuperTilemapEditor
{
    public class TilemapUtils
    {      
        /// <summary>
        /// Get the world position for the center of a given grid cell position.
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        static public Vector3 GetGridWorldPos(STETilemap tilemap, int gridX, int gridY)
        {
            return tilemap.transform.TransformPoint(new Vector2((gridX + .5f) * tilemap.CellSize.x, (gridY + .5f) * tilemap.CellSize.y));
        }

        static public Vector3 GetGridWorldPos(int gridX, int gridY, Vector2 cellSize)
        {
            return new Vector2((gridX + .5f) * cellSize.x, (gridY + .5f) * cellSize.y);
        }


        public static Material FindDefaultSpriteMaterial()
        {
#if UNITY_EDITOR && (UNITY_5_4 || UNITY_5_5_OR_NEWER)
            return UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
#else
            return Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
#endif
        }

        /// <summary>
        /// Get the parameter container from tileData if tileData contains a tile with parameters or Null in other case
        /// </summary>
        static public ParameterContainer GetParamsFromTileData(STETilemap tilemap, uint tileData)
        {
            return GetParamsFromTileData(tilemap.Tileset, tileData);
        }

        /// <summary>
        /// Get the parameter container from tileData if tileData contains a tile with parameters or Null in other case
        /// </summary>
        static public ParameterContainer GetParamsFromTileData(Tileset tileset, uint tileData)
        {
            int brushId = Tileset.GetBrushIdFromTileData(tileData);
            TilesetBrush brush = tileset.FindBrush(brushId);
            if (brush)
            {
                return brush.Params;
            }
            else
            {
                int tileId = Tileset.GetTileIdFromTileData(tileData);
                Tile tile = tileset.GetTile(tileId);
                if (tile != null)
                {
                    return tile.paramContainer;
                }
            }
            return null;
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
}