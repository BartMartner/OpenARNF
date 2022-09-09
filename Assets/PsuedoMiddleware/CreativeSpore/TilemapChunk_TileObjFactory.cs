using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{
    partial class TilemapChunk
    {
        const string k_OnTilePrefabCreation = "OnTilePrefabCreation";

        public struct OnTilePrefabCreationData
        {
            public STETilemap ParentTilemap;
            public int GridX;
            public int GridY;
            public ParameterContainer Parameters{ get { return TilemapUtils.GetParamsFromTileData(ParentTilemap, ParentTilemap.GetTileData(GridX, GridY)); } }
            public Tile Tile { get { return ParentTilemap.GetTile(GridX, GridY); }  }
            public TilesetBrush Brush { get { return ParentTilemap.GetBrush(GridX, GridY); } }
        }

        [System.Serializable]
        private class TileObjData
        {
            public int tilePos;
            public TilePrefabData tilePrefabData;
            public GameObject obj = null;
        }                

        /// <summary>
        /// Update all tile objects if tile prefab data was changed and create tile objects for tiles with new prefab data.
        /// Call this method only when a tile prefab data has been changed to update changes. You may need to call UpdateMesh after this.
        /// </summary>
        public void RefreshTileObjects()
        {
            // Destroy tile objects where tile prefab is now null
            for (int i = 0; i < m_tileObjList.Count; ++i)
            {
                TileObjData tileObjData = m_tileObjList[i];
                uint tileData = m_tileDataList[tileObjData.tilePos];
                int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                Tile tile = Tileset.GetTile( tileId );
                if (tile == null || tile.prefabData.prefab == null)
                {
                    DestroyTileObject(tileObjData.tilePos);
                }
            }

            // Recreate or update all tile objects
            for (int tileIdx = 0; tileIdx < m_tileDataList.Count; ++tileIdx)
            {
                uint tileData = m_tileDataList[tileIdx];
                int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                Tile tile = Tileset.GetTile(tileId);
                if (tile != null && tile.prefabData.prefab != null)
                {
                    CreateTileObject(tileIdx, tile.prefabData);
                }
            }
        }

        private TileObjData FindTileObjDataByTileIdx(int tileIdx)
        {
            for(int i = 0; i < m_tileObjList.Count; ++i)
            {
                TileObjData data = m_tileObjList[i];
                if( data.tilePos == tileIdx ) return data;
            }
            return null;
        }

        private GameObject CreateTileObject(int tileIdx, TilePrefabData tilePrefabData)
        {
            if (tilePrefabData.prefab != null)
            {
                TileObjData tileObjData = FindTileObjDataByTileIdx(tileIdx);
                GameObject tileObj = null;
                int gx = tileIdx % m_width;
                int gy = tileIdx / m_width;
                if (tileObjData == null || tileObjData.tilePrefabData != tilePrefabData || tileObjData.obj == null)
                {                    
#if UNITY_EDITOR
                    tileObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefabData.prefab);
                    // allow destroy the object with undo operations
                    if (ParentTilemap.IsUndoEnabled)
                    {
                        UnityEditor.Undo.RegisterCreatedObjectUndo(tileObj, STETilemap.k_UndoOpName + ParentTilemap.name);
                    }
#else
                    tileObj = (GameObject)Instantiate(tilePrefabData.prefab, Vector3.zero, transform.rotation);
#endif
                    _SetTileObjTransform(tileObj, gx, gy, tilePrefabData, m_tileDataList[tileIdx]);
                    if (tileObjData != null)
                    {
                        m_tileObjToBeRemoved.Add(tileObjData.obj);
                        tileObjData.obj = tileObj;
                        tileObjData.tilePrefabData = tilePrefabData;
                    }
                    else
                    {
                        m_tileObjList.Add(new TileObjData() { tilePos = tileIdx, obj = tileObj, tilePrefabData = tilePrefabData });
                    }
                    tileObj.SendMessage(k_OnTilePrefabCreation, 
                        new OnTilePrefabCreationData() 
                        { 
                            ParentTilemap = ParentTilemap, 
                            GridX = GridPosX + gx, GridY = GridPosY + gy 
                        }, SendMessageOptions.DontRequireReceiver);
                    return tileObj;
                }
                else if (tileObjData.obj != null)
                {
#if UNITY_EDITOR && !UNITY_2018_3_OR_NEWER
                    //+++ Break tilemap prefab and restore tile prefab link
                    GameObject parentPrefab = UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(tileObjData.obj);
                    if (parentPrefab != tileObjData.obj)
                    {
                        DestroyImmediate(tileObjData.obj);
                        tileObjData.obj = UnityEditor.PrefabUtility.InstantiatePrefab(tileObjData.tilePrefabData.prefab) as GameObject;
                    }
                    ///---
#endif
                    _SetTileObjTransform(tileObjData.obj, gx, gy, tilePrefabData, m_tileDataList[tileIdx]);
                    tileObjData.obj.SendMessage(k_OnTilePrefabCreation,
                        new OnTilePrefabCreationData()
                        {
                            ParentTilemap = ParentTilemap,
                            GridX = GridPosX + gx,
                            GridY = GridPosY + gy
                        }, SendMessageOptions.DontRequireReceiver);
                    return tileObjData.obj;
                }
            }
            return null;
        }

        private void _SetTileObjTransform(GameObject tileObj, int gx, int gy, TilePrefabData tilePrefabData, uint tileData)
        {
            Vector3 chunkLocPos = new Vector3((gx + .5f) * CellSize.x, (gy + .5f) * CellSize.y, tilePrefabData.prefab.transform.position.z);
            if (tilePrefabData.offsetMode == TilePrefabData.eOffsetMode.Pixels)
            {
                float ppu = Tileset.TilePxSize.x / CellSize.x;
                chunkLocPos += tilePrefabData.offset / ppu;
            }
            else //if (tilePrefabData.offsetMode == TilePrefabData.eOffsetMode.Units)
            {
                chunkLocPos += tilePrefabData.offset;
            }
            Vector3 worldPos = transform.TransformPoint(chunkLocPos);

            tileObj.transform.position = worldPos;
            tileObj.transform.rotation = tilePrefabData.prefab.transform.rotation;
            tileObj.transform.parent = transform.parent;
            tileObj.transform.localRotation = tilePrefabData.prefab.transform.localRotation * Quaternion.Euler(tilePrefabData.rotation);
            tileObj.transform.localScale = tilePrefabData.prefab.transform.localScale;
            //+++ Apply tile flags
            Vector3 localScale = tileObj.transform.localScale;
            if ((tileData & Tileset.k_TileFlag_Rot90) != 0)
                tileObj.transform.localRotation *= Quaternion.AngleAxis(-90, transform.forward);
            //For Rot180 and Rot270 avoid changing the scale
            if (((tileData & Tileset.k_TileFlag_FlipH) != 0) && ((tileData & Tileset.k_TileFlag_FlipV) != 0))
                tileObj.transform.localRotation *= Quaternion.AngleAxis(-180, transform.forward);
            else
            {
                if ((tileData & Tileset.k_TileFlag_FlipH) != 0)
                    localScale.x = -tileObj.transform.localScale.x;
                if ((tileData & Tileset.k_TileFlag_FlipV) != 0)
                    localScale.y = -tileObj.transform.localScale.y;
            }
            tileObj.transform.localScale = localScale;
            //---
        }

        private void DestroyTileObject(int tileIdx)
        {
            TileObjData tileObjData = FindTileObjDataByTileIdx(tileIdx);
            if (tileObjData != null)
            {
                m_tileObjToBeRemoved.Add(tileObjData.obj);
                m_tileObjList.Remove(tileObjData);
            }
        }

        /// <summary>
        /// Call DestroyTileObject(int tileIdx) to destroy tile objects. This should be called only by UpdateMesh.
        /// NOTE: this delayed destruction is fixing an undo / redo issue
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyTileObject(GameObject obj)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
    }
}
