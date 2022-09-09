using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CreativeSpore.SuperTilemapEditor
{
    partial class TilemapChunk
    {        
        /// <summary>
        /// Next time UpdateMesh is called, the tile mesh will be rebuild
        /// </summary>
        public void InvalidateMesh()
        {
            m_needsRebuildMesh = true;
        }

        /// <summary>
        /// Invalidates brushes, so all tiles with brushes call again the brush refresh method on next UpdateMesh call
        /// </summary>
        public void InvalidateBrushes()
        {
            m_invalidateBrushes = true;
        }

        public void SetSharedMaterial(Material material)
        {
            m_meshRenderer.sharedMaterial = material;
            m_needsRebuildMesh = true;
        }

        private bool m_needsRebuildMesh = false;
        private bool m_needsRebuildMeshColor = false;
        private bool m_invalidateBrushes = false;
        /// <summary>
        /// Update the mesh and return false if all tiles are empty
        /// </summary>
        /// <returns></returns>
        public bool UpdateMesh(bool onlyPreview = false)
        {
            if (ParentTilemap == null)
            {
                if (transform.parent == null) gameObject.hideFlags = HideFlags.None; //Unhide orphan tilechunks. This shouldn't happen
                ParentTilemap = transform.parent.GetComponent<STETilemap>();
            }
            if(gameObject.layer != ParentTilemap.gameObject.layer)
                gameObject.layer = ParentTilemap.gameObject.layer;
            if (!gameObject.CompareTag(ParentTilemap.gameObject.tag))
                gameObject.tag = ParentTilemap.gameObject.tag;
            transform.localPosition = new Vector2(GridPosX * CellSize.x, GridPosY * CellSize.y);

            if (m_meshFilter.sharedMesh == null)
            {
                //Debug.Log("Creating new mesh for " + name);
                m_meshFilter.sharedMesh = new Mesh();
                m_meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
                m_meshFilter.sharedMesh.name = ParentTilemap.name + "_mesh";
                m_needsRebuildMesh = true;
            }

            //NOTE: else above
            {
                m_meshRenderer.sharedMaterial = ParentTilemap.Material;
            }
            m_meshRenderer.enabled = ParentTilemap.IsVisible;
            if (m_needsRebuildMesh)
            {
                m_needsRebuildMesh = false;                
                if (FillMeshData())
                {
                    m_invalidateBrushes = false;
                    Mesh mesh = m_meshFilter.sharedMesh;
                    mesh.Clear();

                    UpdateSortOrder();

                    mesh.SetVertices(s_vertices);
                    mesh.SetTriangles(s_triangles, 0);
                    mesh.SetUVs(0, m_uv);
                    if (s_colors32 != null && s_colors32.Count != 0)
                        mesh.SetColors(s_colors32);
                    else
                        mesh.SetColors((List<Color32>)null);

                    if (!onlyPreview)
                    {
                        mesh.RecalculateNormals(); //NOTE: allow directional lights to work properly
                        TangentSolver(mesh); //NOTE: allow bumped shaders to work with directional lights
                    }
                }
                else
                {
                    return false;
                }
            }
            else if(m_needsRebuildMeshColor)
            {
                UpdateMeshVertexColor();

                Mesh mesh = m_meshFilter.sharedMesh;
                if (s_colors32 != null && s_colors32.Count != 0)
                    mesh.SetColors(s_colors32);
                else
                    mesh.SetColors((List<Color32>)null);
            }
            m_needsRebuildMeshColor = false;
            return true;
        }

        //NOTE: This is not working, only TopRight is working, because the size of s_triangles is not m_width * m_height
        private void UpdateSortOrder()
        {
            if(ParentTilemap.SortOrder == STETilemap.eSortOrder.TopRight)
            {
                // Reverse the list in chunks of 6 elements (2 triangles = tile)
                for (int i = 0, j = s_triangles.Count - 6; i < j; i += 6, j -= 6)
                {
                    for (int k = 0; k < 6; ++k)
                    {
                        int temp = s_triangles[i + k];
                        s_triangles[i + k] = s_triangles[j + k];
                        s_triangles[j + k] = temp;
                    }
                }
            }
        }

        //ref: https://github.com/danielbuechele/SumoVizUnity/blob/master/Assets/Helper/TangentSolver.cs
        // This script has been simplified to be used with tiles were the tangent is always (1, 0, 0, -1)
        private void TangentSolver(Mesh mesh)
	    {
		    int vertexCount = mesh.vertexCount;
		    Vector4[] tangents = new Vector4[vertexCount];
            //ref: https://github.com/danielbuechele/SumoVizUnity/blob/master/Assets/Helper/TangentSolver.cs
            //NOTE: fix issues when using a bumped shader
		    for (int i = 0; i < (vertexCount); i++)
		    {		
			    tangents[i].x = 1f;
			    //tangents[i].y = 0f;
			    //tangents[i].z = 0f;
			    tangents[i].w = -1f;
		    }
		    mesh.tangents = tangents;
	    }

        private void DestroyMeshIfNeeded()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh != null
                && (meshFilter.sharedMesh.hideFlags & HideFlags.DontSave) != 0)
            {
                //Debug.Log("Destroy Mesh of " + name);
                DestroyImmediate(meshFilter.sharedMesh);
            }
        }

        static TilemapChunk s_currUpdatedTilechunk;
        static int s_currUVVertex;
        public static void RegisterAnimatedBrush(IBrush brush, int subTileIdx = -1)
        {
            if (s_currUpdatedTilechunk)
            {
                if (subTileIdx >= 0)
                    s_currUpdatedTilechunk.m_animatedTiles.Add(new AnimTileData() { VertexIdx = s_currUVVertex + (subTileIdx << 2), Brush = brush, SubTileIdx = subTileIdx });
                else
                    s_currUpdatedTilechunk.m_animatedTiles.Add(new AnimTileData() { VertexIdx = s_currUVVertex, Brush = brush, SubTileIdx = subTileIdx });
            }
        }

        /*
        private void DummyDeepProfilingFix()
        {
            // For some reason, in Unity 2017.3.1f1, the Deep Profiling crashes unless FillMeshData call any method, even a dummy method like this
            // Other weird thing is, the crash doesn't happens if one case of the switch statement is commented
            // FINALLY: the fix was to change the Switch for if-else statements. I keep this notes just in case to remember about this weird issue.
        }
        */

        /// <summary>
        /// Fill the mesh data and return false if all tiles are empty
        /// </summary>
        /// <returns></returns>
        private bool FillMeshData()
        {
            //Debug.Log( "[" + ParentTilemap.name + "] FillData -> " + name);
            //DummyDeepProfilingFix();

            if (!Tileset || !Tileset.AtlasTexture)
            {
                return false;
            }
            s_currUpdatedTilechunk = this;

            int totalTiles = m_width * m_height;
            if (s_vertices == null) s_vertices = new List<Vector3>(totalTiles * 4);
            else s_vertices.Clear();
            if (s_triangles == null) s_triangles = new List<int>(totalTiles * 6);
            else s_triangles.Clear();
            if (s_colors32 == null) s_colors32 = new List<Color32>(totalTiles * 4);
            else s_colors32.Clear();
            if (m_uv == null) m_uv = new List<Vector2>(totalTiles * 4);
            else m_uv.Clear();

            Vector2[] subTileOffset = new Vector2[]
            {
                new Vector2( 0f, 0f ),
                new Vector2( CellSize.x / 2f, 0f ),
                new Vector2( 0f, CellSize.y / 2f ),
                new Vector2( CellSize.x / 2f, CellSize.y / 2f ),
            };
            Vector2 subTileSize = CellSize / 2f;
            m_animatedTiles.Clear();
            bool isEmpty = true;
            for (int ty = 0, tileIdx = 0; ty < m_height; ++ty)
            {
                for (int tx = 0; tx < m_width; ++tx, ++tileIdx)
                {
                    uint tileData = m_tileDataList[tileIdx];
                    if (tileData != Tileset.k_TileData_Empty)
                    {
                        int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
                        int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                        Tile tile = Tileset.GetTile(tileId);
                        TilesetBrush tileBrush = null;
                        if(tileId >= 0 && tile == null && brushId <= 0)
                        {
                            Debug.LogWarning(ParentTilemap.name + "\\" + name + ": TileId " + tileId + " not found! GridPos(" + (GridPosX + tx) + "," + (GridPosY + ty) + ") tilaData 0x" + tileData.ToString("X"));
                            m_tileDataList[tileIdx] = Tileset.k_TileData_Empty;
                        }
                        if (brushId > 0)
                        {
                            tileBrush = Tileset.FindBrush(brushId);
                            if (tileBrush == null)
                            {
                                Debug.LogWarning(ParentTilemap.name + "\\" + name + ": BrushId " + brushId + " not found! GridPos(" + (GridPosX + tx) + "," + (GridPosY + ty) + ") tilaData 0x" + tileData.ToString("X"));
                                m_tileDataList[tileIdx] = tileData & ~Tileset.k_TileDataMask_BrushId;
                            }
                            if (tileBrush != null && (m_invalidateBrushes || (tileData & Tileset.k_TileFlag_Updated) == 0))
                            {
                                tileData = tileBrush.Refresh(ParentTilemap, GridPosX + tx, GridPosY + ty, tileData);
                                //+++NOTE: this code add support for animated brushes inside a random brush
                                // Collateral effects of supporting changing the brush id in Refresh:
                                // - When the random brush select a tile data with another brush id, this tile won't be a random tile any more
                                // - If the tilemap is refreshed several times, and at least a tile data contains another brush id, then all tiles will loose the brush id of the random brush
                                if (BrushBehaviour.Instance.BrushTilemap == ParentTilemap) // avoid changing brushId when updating the BrushTilemap
                                {
                                    tileData &= ~Tileset.k_TileDataMask_BrushId;
                                    tileData |= (uint)( brushId << 16 );
                                }
                                int newBrushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
                                if(brushId != newBrushId)
                                {
                                    brushId = newBrushId;
                                    tileBrush = Tileset.FindBrush(brushId);
                                }
                                //---
                                tileData |= Tileset.k_TileFlag_Updated;// set updated flag
                                m_tileDataList[tileIdx] = tileData; // update tileData                                
                                tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                                tile = Tileset.GetTile(tileId);
                                // update created objects
                                if (tile != null && tile.prefabData.prefab != null)
                                    CreateTileObject(tileIdx, tile.prefabData);
                                else
                                    DestroyTileObject(tileIdx);
                            }
                        }

                        isEmpty = false;

                        if (tileBrush != null && tileBrush.IsAnimated())
                        {
                            m_animatedTiles.Add(new AnimTileData() { VertexIdx = s_vertices.Count, Brush = tileBrush, SubTileIdx = -1 });
                        }
                        
                        s_currUVVertex = s_vertices.Count;
                        Rect tileUV;
                        uint[] subtileData = tileBrush != null ? tileBrush.GetSubtiles(ParentTilemap, GridPosX + tx, GridPosY + ty, tileData) : null;
                        if (subtileData == null)
                        {
                            if (tile != null)
                            {
                                if (tile.prefabData.prefab == null || tile.prefabData.showTileWithPrefab //hide the tiles with prefabs ( unless showTileWithPrefab is true )
                                    || tileBrush && tileBrush.IsAnimated()) // ( skip if it's an animated brush )
                                {
                                    tileUV = tile.uv;
                                    _AddTileToMesh(tileUV, tx, ty, tileData, Vector2.zero, CellSize);
                                    if (m_tileColorList != null && m_tileColorList.Count > tileIdx)
                                    {
                                        TileColor32 tileColor32 = m_tileColorList[tileIdx];
                                        s_colors32.Add(tileColor32.c0);
                                        s_colors32.Add(tileColor32.c1);
                                        s_colors32.Add(tileColor32.c2);
                                        s_colors32.Add(tileColor32.c3);
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < subtileData.Length; ++i)
                            {
                                uint subTileData = subtileData[i];
                                int subTileId = (int)(subTileData & Tileset.k_TileDataMask_TileId);
                                Tile subTile = Tileset.GetTile(subTileId);
                                tileUV = subTile != null ? subTile.uv : default(Rect);
                                //if (tileUV != default(Rect)) //NOTE: if this is uncommented, there won't be coherence with geometry ( 16 vertices per tiles with subtiles ). But it means also, the tile shouldn't be null.
                                {
                                    _AddTileToMesh(tileUV, tx, ty, subTileData, subTileOffset[i], subTileSize, i);
                                    if (m_tileColorList != null && m_tileColorList.Count > tileIdx)
                                    {
                                        TileColor32 tileColor32 = m_tileColorList[tileIdx];
                                        Color32 middleColor = new Color32(
                                            System.Convert.ToByte((tileColor32.c0.r + tileColor32.c1.r + tileColor32.c2.r + tileColor32.c3.r) >> 2),
                                            System.Convert.ToByte((tileColor32.c0.g + tileColor32.c1.g + tileColor32.c2.g + tileColor32.c3.g) >> 2),
                                            System.Convert.ToByte((tileColor32.c0.b + tileColor32.c1.b + tileColor32.c2.b + tileColor32.c3.b) >> 2),
                                            System.Convert.ToByte((tileColor32.c0.a + tileColor32.c1.a + tileColor32.c2.a + tileColor32.c3.a) >> 2)
                                            );
                                        //switch(i) // FIX Deep Profiling crash in Unity 2017.3.1f1 see: DummyDeepProfilingFix notes
                                        {
                                            if (i == 0)
                                            {
                                                s_colors32.Add(tileColor32.c0);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c1, tileColor32.c0, .5f));
                                                s_colors32.Add(Color32.Lerp(tileColor32.c2, tileColor32.c0, .5f));
                                                s_colors32.Add(middleColor);
                                            }
                                            else if (i == 1)
                                            {
                                                s_colors32.Add(Color32.Lerp(tileColor32.c0, tileColor32.c1, .5f));
                                                s_colors32.Add(tileColor32.c1);
                                                s_colors32.Add(middleColor);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c3, tileColor32.c1, .5f));
                                            }
                                            else if (i == 2)
                                            {
                                                s_colors32.Add(Color32.Lerp(tileColor32.c0, tileColor32.c2, .5f));
                                                s_colors32.Add(middleColor);
                                                s_colors32.Add(tileColor32.c2);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c3, tileColor32.c2, .5f));
                                            }
                                            else if (i == 3)
                                            {
                                                s_colors32.Add(middleColor);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c1, tileColor32.c3, .5f));
                                                s_colors32.Add(Color32.Lerp(tileColor32.c2, tileColor32.c3, .5f));
                                                s_colors32.Add(tileColor32.c3);
                                            }
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //NOTE: the destruction of tileobjects needs to be done here to avoid a Undo/Redo bug. Check inside DestroyTileObject for more information.
            for (int i = 0; i < m_tileObjToBeRemoved.Count; ++i)
            {
                DestroyTileObject(m_tileObjToBeRemoved[i]);
            }
            m_tileObjToBeRemoved.Clear();
            s_currUpdatedTilechunk = null;
            return !isEmpty;
        }

        static Vector2[] s_tileUV = new Vector2[4];
        private void _AddTileToMesh(Rect tileUV, int tx, int ty, uint tileData, Vector2 subtileOffset, Vector2 subtileCellSize, int subTileIdx = -1)
        {
            float px0 = tx * CellSize.x + subtileOffset.x;
            float py0 = ty * CellSize.y + subtileOffset.y;
            //NOTE: px0 and py0 values are not used to avoid float errors and line artifacts. Don't forget Pixel Snap has to be disabled as well.
            float px1 = tx * CellSize.x + subtileOffset.x + subtileCellSize.x;
            float py1 = ty * CellSize.y + subtileOffset.y + subtileCellSize.y;

            //TODO: use a property in the tileset to enable/disable the aspect ratio fix
            // Add also an option to change the tile pivot
            Tileset tileset = ParentTilemap.Tileset;
            Texture2D atlasTexture = tileset.AtlasTexture;
            int pixelW = Mathf.RoundToInt(tileUV.width * atlasTexture.width);
            int pixelH = Mathf.RoundToInt(tileUV.height * atlasTexture.height);            
            if(pixelW != tileset.TilePxSize.x)
            {
                px1 = px0 + subtileOffset.x + subtileCellSize.x * pixelW / tileset.TilePxSize.x;
            }
            if (pixelH != tileset.TilePxSize.y)
            {
                py1 = py0 + subtileOffset.y + subtileCellSize.y * pixelH / tileset.TilePxSize.y;
            }

            int vertexIdx = s_vertices.Count;
            s_vertices.Add(new Vector3(px0, py0, 0));
            s_vertices.Add(new Vector3(px1, py0, 0));
            s_vertices.Add(new Vector3(px0, py1, 0));
            s_vertices.Add(new Vector3(px1, py1, 0));

            s_triangles.Add(vertexIdx + 3);
            s_triangles.Add(vertexIdx + 0);
            s_triangles.Add(vertexIdx + 2);
            s_triangles.Add(vertexIdx + 0);
            s_triangles.Add(vertexIdx + 3);
            s_triangles.Add(vertexIdx + 1);

            bool flipH = (tileData & Tileset.k_TileFlag_FlipH) != 0;
            bool flipV = (tileData & Tileset.k_TileFlag_FlipV) != 0;
            bool rot90 = (tileData & Tileset.k_TileFlag_Rot90) != 0;

            //NOTE: xMinMax and yMinMax is opposite if width or height is negative
            float u0 = tileUV.xMin + Tileset.AtlasTexture.texelSize.x * InnerPadding;
            float v0 = tileUV.yMin + Tileset.AtlasTexture.texelSize.y * InnerPadding;
            float u1 = tileUV.xMax - Tileset.AtlasTexture.texelSize.x * InnerPadding;
            float v1 = tileUV.yMax - Tileset.AtlasTexture.texelSize.y * InnerPadding;

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
                s_tileUV[0] = new Vector2(u1, v0);
                s_tileUV[1] = new Vector2(u1, v1);
                s_tileUV[2] = new Vector2(u0, v0);
                s_tileUV[3] = new Vector2(u0, v1);
            }
            else
            {
                s_tileUV[0] = new Vector2(u0, v0);
                s_tileUV[1] = new Vector2(u1, v0);
                s_tileUV[2] = new Vector2(u0, v1);
                s_tileUV[3] = new Vector2(u1, v1);
            }
            // When using subtiles, the UV positions are halved. This is done making an average between 
            // the uv position in the subtileIdx position and the rest of positions
            if (subTileIdx >= 0)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (i == subTileIdx) continue;
                    s_tileUV[i] = (s_tileUV[i] + s_tileUV[subTileIdx]) / 2f;
                }
            }
            for (int i = 0; i < 4; ++i)
            {
                m_uv.Add(s_tileUV[i]);
            }
        }

        private void UpdateMeshVertexColor()
        {
            //Debug.Log( "[" + ParentTilemap.name + "] FillData -> " + name);
            if (!Tileset || !Tileset.AtlasTexture)
            {
                return;
            }

            int totalTiles = m_width * m_height;
            if (s_colors32 == null) s_colors32 = new List<Color32>(totalTiles * 4);
            else s_colors32.Clear();

            for (int ty = 0, tileIdx = 0; ty < m_height; ++ty)
            {
                for (int tx = 0; tx < m_width; ++tx, ++tileIdx)
                {
                    uint tileData = m_tileDataList[tileIdx];
                    if (tileData != Tileset.k_TileData_Empty)
                    {
                        int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
                        int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                        Tile tile = Tileset.GetTile(tileId);
                        TilesetBrush tileBrush = null;
                        if (brushId > 0)
                        {
                            tileBrush = Tileset.FindBrush(brushId);                            
                        }

                        uint[] subtileData = tileBrush != null ? tileBrush.GetSubtiles(ParentTilemap, GridPosX + tx, GridPosY + ty, tileData) : null;
                        if (subtileData == null)
                        {
                            if (tile != null)
                            {
                                if (tile.prefabData.prefab == null || tile.prefabData.showTileWithPrefab //hide the tiles with prefabs ( unless showTileWithPrefab is true )
                                    || tileBrush && tileBrush.IsAnimated()) // ( skip if it's an animated brush )
                                {
                                    if (m_tileColorList != null && m_tileColorList.Count > tileIdx)
                                    {
                                        TileColor32 tileColor32 = m_tileColorList[tileIdx];
                                        s_colors32.Add(tileColor32.c0);
                                        s_colors32.Add(tileColor32.c1);
                                        s_colors32.Add(tileColor32.c2);
                                        s_colors32.Add(tileColor32.c3);
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < subtileData.Length; ++i)
                            {
                                //if (tileUV != default(Rect)) //NOTE: if this is uncommented, there won't be coherence with geometry ( 16 vertices per tiles with subtiles ). But it means also, the tile shouldn't be null.
                                {
                                    if (m_tileColorList != null && m_tileColorList.Count > tileIdx)
                                    {
                                        TileColor32 tileColor32 = m_tileColorList[tileIdx];
                                        Color32 middleColor = new Color32(
                                            System.Convert.ToByte((tileColor32.c0.r + tileColor32.c1.r + tileColor32.c2.r + tileColor32.c3.r) >> 2),
                                            System.Convert.ToByte((tileColor32.c0.g + tileColor32.c1.g + tileColor32.c2.g + tileColor32.c3.g) >> 2),
                                            System.Convert.ToByte((tileColor32.c0.b + tileColor32.c1.b + tileColor32.c2.b + tileColor32.c3.b) >> 2),
                                            System.Convert.ToByte((tileColor32.c0.a + tileColor32.c1.a + tileColor32.c2.a + tileColor32.c3.a) >> 2)
                                            );
                                        switch (i)
                                        {
                                            case 0:
                                                s_colors32.Add(tileColor32.c0);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c1, tileColor32.c0, .5f));
                                                s_colors32.Add(Color32.Lerp(tileColor32.c2, tileColor32.c0, .5f));
                                                s_colors32.Add(middleColor);
                                                break;
                                            case 1:
                                                s_colors32.Add(Color32.Lerp(tileColor32.c0, tileColor32.c1, .5f));
                                                s_colors32.Add(tileColor32.c1);
                                                s_colors32.Add(middleColor);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c3, tileColor32.c1, .5f));
                                                break;
                                            case 2:
                                                s_colors32.Add(Color32.Lerp(tileColor32.c0, tileColor32.c2, .5f));
                                                s_colors32.Add(middleColor);
                                                s_colors32.Add(tileColor32.c2);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c3, tileColor32.c2, .5f));
                                                break;
                                            case 3:
                                                s_colors32.Add(middleColor);
                                                s_colors32.Add(Color32.Lerp(tileColor32.c1, tileColor32.c3, .5f));
                                                s_colors32.Add(Color32.Lerp(tileColor32.c2, tileColor32.c3, .5f));
                                                s_colors32.Add(tileColor32.c3);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
