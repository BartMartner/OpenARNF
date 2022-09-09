using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CreativeSpore.SuperTilemapEditor
{
    public class STETilemap : MonoBehaviour
    {
        public const int k_chunkSize = 60;
        public const string k_UndoOpName = "Paint Op. ";

        public static bool DisableTilePrefabCreation = false;

        #region Public Events
        public delegate void OnMeshUpdatedDelegate(STETilemap source);
        public OnMeshUpdatedDelegate OnMeshUpdated;
        public delegate void OnTileChangedDelegate(STETilemap tilemap, int gridX, int gridY, uint tileData);
        public OnTileChangedDelegate OnTileChanged;
        #endregion

        public Tileset Tileset
        {
            get { return m_tileset; }
            set
            {
                bool hasChanged = m_tileset != value;
                m_tileset = value;
                if (hasChanged)
                {
                    if (Tileset != null && CellSize == default(Vector2))
                    {
                        CellSize = m_tileset.TilePxSize / m_tileset.PixelsPerUnit;
                    }
                }
            }
        }

        public Vector2 CellSize { get { return m_cellSize; } set { m_cellSize = value; } }

        /// <summary>
        /// Returns the minimum horizontal grid position of the tilemap area
        /// </summary>
        public int MinGridX { get { return m_minGridX; } /*set { m_minGridX = Mathf.Min(0, value); }*/ }
        /// <summary>
        /// Returns the minimum vertical grid position of the tilemap area
        /// </summary>
        public int MinGridY { get { return m_minGridY; } /*set { m_minGridY = Mathf.Min(0, value); }*/ }
        /// <summary>
        /// Returns the maximum horizontal grid position of the tilemap area
        /// </summary>
        public int MaxGridX { get { return m_maxGridX; } /*set { m_maxGridX = Mathf.Max(0, value); }*/ }
        /// <summary>
        /// Returns the maximum vertical grid position of the tilemap area
        /// </summary>
        public int MaxGridY { get { return m_maxGridY; } /*set { m_maxGridY = Mathf.Max(0, value); }*/ }
        /// <summary>
        /// Returns the horizontal size of the grid in tiles
        /// </summary>
        public int GridWidth { get { return m_maxGridX - m_minGridX + 1; } }
        /// <summary>
        /// Returns the vertical size of the grid in tiles
        /// </summary>
        public int GridHeight { get { return m_maxGridY - m_minGridY + 1; } }
        /// <summary>
        /// Returns the parent tilemap group the tilemap is children of
        /// </summary>
        public TilemapGroup ParentTilemapGroup { get { return m_parentTilemapGroup; } }

        /// <summary>
        /// Color applied to the material before rendering
        /// </summary>
        public Color TintColor
        {
            get { return m_tintColor; }
            set { m_tintColor = value; }
        }

        public Material Material
        {
            get
            {
                if (m_material == null)
                {
                    m_material = TilemapUtils.FindDefaultSpriteMaterial();
                }
                return m_material;
            }

            set
            {
                if (value != null && m_material != value)
                {
                    m_material = value;
                    Refresh();
                }
            }
        }

        public bool PixelSnap { get { return m_pixelSnap; } set { m_pixelSnap = value; } }
        public bool IsVisible
        {
            get
            {
                return m_isVisible;
            }
            set
            {
                bool prevValue = m_isVisible;
                m_isVisible = value;
                if (m_isVisible != prevValue)
                {
                    UpdateMesh();
                }
            }
        }

        public bool IsUndoEnabled = false;
        public enum eSortOrder
        {
            BottomLeft, //default
            //BottomRight,
            //TopLeft,
            TopRight //reverse triangles (working only locally to a tilemap chunk)
        }
        [Tooltip("Sort order for all tiles rendered by the Tilemap Chunk")]

        public eSortOrder SortOrder = eSortOrder.BottomLeft;
        //NOTE: the inner padding fix the zoom imperfection, but as long as zoom is bigger, the bigger this value has to be        
        /// <summary>
        /// The size, in pixels, the tile UV will be stretched. Use this to fix pixel precision artifacts when tiles have no padding border in the atlas.
        /// </summary>
        public float InnerPadding = 0f;
        /// <summary>
        /// The depth size of the collider. You need to call Refresh(false, true) after changing this value to refresh the collider.
        /// </summary>
        public float ColliderDepth = 0.1f;
        /// <summary>
        /// Set the colliders for this tilemap. You need to call the Refresh method to update all the tilemap chunks after changing this parameter.
        /// </summary>
        public eColliderType ColliderType = eColliderType.None;
        /// <summary>
        /// The type of collider used when ColliderType is eColliderType._2D
        /// </summary>
        public e2DColliderType Collider2DType = e2DColliderType.EdgeCollider2D;
        /// <summary>
        /// Sets the isTrigger property of the collider. You need call Refresh to update the colliders after changing it.
        /// </summary>
        public bool IsTrigger { get { return m_isTrigger; } set { m_isTrigger = value; } }
        /// <summary>
        /// The PhysicsMaterial that is applied to this tilemap colliders.
        /// </summary>
        public PhysicMaterial PhysicMaterial { get { return m_physicMaterial; } set { m_physicMaterial = value; } }
        /// <summary>
        /// The PhysicsMaterial2D that is applied to this tilemap colliders.
        /// </summary>
        public PhysicsMaterial2D PhysicMaterial2D { get { return m_physicMaterial2D; } set { m_physicMaterial2D = value; } }

        [SerializeField, SortingLayer]
        private int m_sortingLayer = 0;
        [SerializeField]
        private string m_sortingLayerName = "Default";
        [SerializeField]
        private int m_orderInLayer = 0;
        [SerializeField]
        private Material m_material;
        [SerializeField]
        private Color m_tintColor;
        [SerializeField]
        private bool m_pixelSnap;
        [SerializeField]
        private bool m_isVisible = true;
        [SerializeField]
        private bool m_allowPaintingOutOfBounds = true;
        [SerializeField]
        private bool m_autoShrink = false;
        [SerializeField, Tooltip("Set to false when painting on big maps to improve performance.")]
        private bool m_enableUndoWhilePainting = true;
        [SerializeField]
        private bool m_isTrigger = false;
        [SerializeField]
        private PhysicMaterial m_physicMaterial;
        [SerializeField]
        private PhysicsMaterial2D m_physicMaterial2D;

        [SerializeField]
        Vector2 m_cellSize;
        [SerializeField]
        private Bounds m_mapBounds;
        [SerializeField]
        private Tileset m_tileset;
        [SerializeField]
        private int m_minGridX;
        [SerializeField]
        private int m_minGridY;
        [SerializeField]
        private int m_maxGridX;
        [SerializeField]
        private int m_maxGridY;
        [SerializeField]
        private TilemapGroup m_parentTilemapGroup;

        private bool m_markForUpdateMesh = false;

        /// <summary>
        /// Returns the brush at the grid position
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public TilesetBrush GetBrush(int gridX, int gridY)
        {
            uint tileData = GetTileData(gridX, gridY);
            int brushId = Tileset.GetBrushIdFromTileData(tileData);
            return Tileset.FindBrush(brushId);
        }

        /// <summary>
        /// Returns the tile at the local position
        /// </summary>
        /// <param name="vLocalPos"></param>
        /// <returns></returns>
        public Tile GetTile(Vector2 vLocalPos)
        {
            int gridX = BrushUtil.GetGridX(vLocalPos, CellSize);
            int gridY = BrushUtil.GetGridY(vLocalPos, CellSize);
            return GetTile(gridX, gridY);
        }

        /// <summary>
        /// Returns the tile at the grid position
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public Tile GetTile(int gridX, int gridY)
        {
            uint tileData = GetTileData(gridX, gridY);
            int tileId = Tileset.GetTileIdFromTileData(tileData);
            return Tileset.GetTile(tileId);
        }

        /// <summary>
        /// Set a tile data using a tilemap local position
        /// </summary>
        /// <param name="vLocalPos"></param>
        /// <param name="tileData"></param>
        public void SetTileData(Vector2 vLocalPos, uint tileData)
        {
            int gridX = BrushUtil.GetGridX(vLocalPos, CellSize);
            int gridY = BrushUtil.GetGridY(vLocalPos, CellSize);
            SetTileData(gridX, gridY, tileData);
        }

        /// <summary>
        /// Set a tile data in the grid position
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="tileData"></param>
        public void SetTileData(int gridX, int gridY, uint tileData)
        {
            TilemapChunk chunk = GetOrCreateTileChunk(gridX, gridY, true);
            int chunkGridX = (gridX < 0 ? -gridX - 1 : gridX) % k_chunkSize;
            int chunkGridY = (gridY < 0 ? -gridY - 1 : gridY) % k_chunkSize;
            if (gridX < 0) chunkGridX = k_chunkSize - 1 - chunkGridX;
            if (gridY < 0) chunkGridY = k_chunkSize - 1 - chunkGridY;
            if (m_allowPaintingOutOfBounds || (gridX >= m_minGridX && gridX <= m_maxGridX && gridY >= m_minGridY && gridY <= m_maxGridY))
            {
                chunk.SetTileData(chunkGridX, chunkGridY, tileData);
                if (OnTileChanged != null) OnTileChanged(this, gridX, gridY, tileData);
                // Update map bounds
                //if (tileData != Tileset.k_TileData_Empty) // commented to update the brush bounds when copying empty tiles
                {
                    m_minGridX = Mathf.Min(m_minGridX, gridX);
                    m_maxGridX = Mathf.Max(m_maxGridX, gridX);
                    m_minGridY = Mathf.Min(m_minGridY, gridY);
                    m_maxGridY = Mathf.Max(m_maxGridY, gridY);
                }
                //--
            }
        }

        /// <summary>
        /// Returns the tile data at the grid position
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public uint GetTileData(int gridX, int gridY)
        {
            TilemapChunk chunk = GetOrCreateTileChunk(gridX, gridY);
            if (chunk == null)
            {
                return Tileset.k_TileData_Empty;
            }
            else
            {
                int chunkGridX = (gridX < 0 ? -gridX - 1 : gridX) % k_chunkSize;
                int chunkGridY = (gridY < 0 ? -gridY - 1 : gridY) % k_chunkSize;
                if (gridX < 0) chunkGridX = k_chunkSize - 1 - chunkGridX;
                if (gridY < 0) chunkGridY = k_chunkSize - 1 - chunkGridY;
                return chunk.GetTileData(chunkGridX, chunkGridY);
            }
        }

        /// <summary>
        /// Updates the render mesh and mesh collider of all tile chunks. This should be called once after making all modifications to the tilemap with SetTileData.
        /// </summary>
        public void UpdateMesh()
        {
            m_markForUpdateMesh = true;
        }

        static List<TilemapChunk> s_chunkList = new List<TilemapChunk>(50);
        /// <summary>
        /// Update the render mesh and mesh collider of all tile chunks. This should be called once after making all modifications to the tilemap with SetTileData.
        /// </summary>
        public void UpdateMeshImmediate()
        {
            RecalculateMapBounds();

            s_chunkList.Clear();
            var valueIter = m_dicChunkCache.Values.GetEnumerator();
            while (valueIter.MoveNext())
            {
                TilemapChunk chunk = valueIter.Current;
                if (chunk)
                {
                    if (!chunk.UpdateMesh())
                    {

                    }
                    else
                    {
                        //chunk.UpdateColliderMesh();
                        s_chunkList.Add(chunk);
                    }
                }
            }

            if (m_autoShrink)
                Trim();

            // UpdateColliderMesh is called after calling UpdateMesh of all tilechunks, because UpdateColliderMesh needs the tileId to be updated 
            // ( remember a brush sets neighbours tile id to empty, so UpdateColliderMesh won't be able to know the collider type )
            for (int i = 0; i < s_chunkList.Count; ++i)
            {
                s_chunkList[i].UpdateColliders();
            }

            if (OnMeshUpdated != null) OnMeshUpdated(this);
        }

        public void Refresh(bool refreshMesh = true, bool refreshMeshCollider = true, bool refreshTileObjects = false, bool invalidateBrushes = false)
        {
            BuildTilechunkDictionary();
            var valueIter = m_dicChunkCache.Values.GetEnumerator();
            while (valueIter.MoveNext())
            {
                TilemapChunk chunk = valueIter.Current;
                if (chunk)
                {
                    if (refreshMesh) chunk.InvalidateMesh();
                    if (refreshMeshCollider) chunk.InvalidateMeshCollider();
                    if (refreshTileObjects) chunk.RefreshTileObjects();
                    if (invalidateBrushes) chunk.InvalidateBrushes();
                }
            }
            UpdateMesh();
        }

        /// <summary>
        /// Sets the map limits
        /// </summary>
        /// <param name="minGridX"></param>
        /// <param name="minGridY"></param>
        /// <param name="maxGridX"></param>
        /// <param name="maxGridY"></param>
        public void SetMapBounds(int minGridX, int minGridY, int maxGridX, int maxGridY)
        {
            m_minGridX = Mathf.Min(minGridX, 0);
            m_minGridY = Mathf.Min(minGridY, 0);
            m_maxGridX = Mathf.Max(maxGridX, 0);
            m_maxGridY = Mathf.Max(maxGridY, 0);
            RecalculateMapBounds();
        }

        /// <summary>
        /// Recalculate the bounding volume of the map from the grid limits
        /// </summary>
        public void RecalculateMapBounds()
        {
            // Fix grid limits if necessary
            m_minGridX = Mathf.Min(m_minGridX, 0);
            m_minGridY = Mathf.Min(m_minGridY, 0);
            m_maxGridX = Mathf.Max(m_maxGridX, 0);
            m_maxGridY = Mathf.Max(m_maxGridY, 0);

            Vector2 minTilePos = Vector2.Scale(new Vector2(m_minGridX, m_minGridY), CellSize);
            Vector2 maxTilePos = Vector2.Scale(new Vector2(m_maxGridX, m_maxGridY), CellSize);
            Vector3 savedSize = m_mapBounds.size;
            m_mapBounds.min = m_mapBounds.max = Vector2.zero;
            m_mapBounds.Encapsulate(minTilePos);
            m_mapBounds.Encapsulate(minTilePos + CellSize);
            m_mapBounds.Encapsulate(maxTilePos);
            m_mapBounds.Encapsulate(maxTilePos + CellSize);
            if (savedSize != m_mapBounds.size)
            {
                var valueIter = m_dicChunkCache.Values.GetEnumerator();
                while (valueIter.MoveNext())
                {
                    TilemapChunk chunk = valueIter.Current;
                    if (chunk)
                    {
                        chunk.InvalidateBrushes(); //TODO: this is very slow. Find a way to update only the tiles on the edges?
                    }
                }
            }
        }

        /// <summary>
        /// Flip the tilemap vertically
        /// </summary>
        /// <param name="changeFlags"></param>
        public void FlipV(bool changeFlags)
        {
            List<uint> flippedList = new List<uint>(GridWidth * GridHeight);
            for (int gy = MinGridY; gy <= MaxGridY; ++gy)
            {
                for (int gx = MinGridX; gx <= MaxGridX; ++gx)
                {
                    int flippedGy = GridHeight - 1 - gy;
                    flippedList.Add(GetTileData(gx, flippedGy));
                }
            }

            int idx = 0;
            for (int gy = MinGridY; gy <= MaxGridY; ++gy)
            {
                for (int gx = MinGridX; gx <= MaxGridX; ++gx, ++idx)
                {
                    uint flippedTileData = flippedList[idx];
                    if (
                        changeFlags
                        && (flippedTileData != Tileset.k_TileData_Empty)
                        && (flippedTileData & Tileset.k_TileDataMask_BrushId) == 0 // don't activate flip flags on brushes
                        )
                    {
                        flippedTileData = TilesetBrush.ApplyAndMergeTileFlags(flippedTileData, Tileset.k_TileFlag_FlipV);
                    }
                    SetTileData(gx, gy, flippedTileData);
                }
            }
        }

        /// <summary>
        /// Flip the map horizontally
        /// </summary>
        /// <param name="changeFlags"></param>
        public void FlipH(bool changeFlags)
        {
            List<uint> flippedList = new List<uint>(GridWidth * GridHeight);
            for (int gx = MinGridX; gx <= MaxGridX; ++gx)
            {
                for (int gy = MinGridY; gy <= MaxGridY; ++gy)
                {
                    int flippedGx = GridWidth - 1 - gx;
                    flippedList.Add(GetTileData(flippedGx, gy));
                }
            }

            int idx = 0;
            for (int gx = MinGridX; gx <= MaxGridX; ++gx)
            {
                for (int gy = MinGridY; gy <= MaxGridY; ++gy, ++idx)
                {
                    uint flippedTileData = flippedList[idx];
                    if (
                        changeFlags
                        && (flippedTileData != Tileset.k_TileData_Empty)
                        && (flippedTileData & Tileset.k_TileDataMask_BrushId) == 0 // don't activate flip flags on brushes
                        )
                    {
                        flippedTileData = TilesetBrush.ApplyAndMergeTileFlags(flippedTileData, Tileset.k_TileFlag_FlipH);
                    }
                    SetTileData(gx, gy, flippedTileData);
                }
            }
        }

        /// <summary>
        /// Rotate the map 90 degrees clockwise
        /// </summary>
        /// <param name="changeFlags"></param>
        public void Rot90(bool changeFlags)
        {
            List<uint> flippedList = new List<uint>(GridWidth * GridHeight);
            for (int gy = MinGridY; gy <= MaxGridY; ++gy)
            {
                for (int gx = MinGridX; gx <= MaxGridX; ++gx)
                {
                    flippedList.Add(GetTileData(gx, gy));
                }
            }

            int minGridX = MinGridX;
            int minGridY = MinGridY;
            int maxGridX = MaxGridY;
            int maxGridY = MaxGridX;
            ClearMap();

            int idx = 0;
            for (int gx = minGridX; gx <= maxGridX; ++gx)
            {
                for (int gy = maxGridY; gy >= minGridY; --gy, ++idx)
                {
                    uint flippedTileData = flippedList[idx];
                    if (
                        changeFlags
                        && (flippedTileData != Tileset.k_TileData_Empty)
                        && (flippedTileData & Tileset.k_TileDataMask_BrushId) == 0 // don't activate flip flags on brushes
                        )
                    {
                        flippedTileData = TilesetBrush.ApplyAndMergeTileFlags(flippedTileData, Tileset.k_TileFlag_Rot90);
                    }
                    SetTileData(gx, gy, flippedTileData);
                }
            }
        }

        public bool InvalidateChunkAt(int gridX, int gridY, bool invalidateMesh = true, bool invalidateMeshCollider = true)
        {
            TilemapChunk chunk = GetOrCreateTileChunk(gridX, gridY);
            if (chunk != null)
            {
                chunk.InvalidateMesh();
                chunk.InvalidateMeshCollider();
                return true;
            }
            return false;
        }

        public void Erase(Vector3 p)
        {
            throw new NotImplementedException();
        }

        public void Trim()
        {
            throw new NotImplementedException();
        }
        public void ClearMap(bool keepBounds = false)
        {
            if (!keepBounds)
            {
                m_mapBounds = new Bounds();
                m_maxGridX = m_maxGridY = m_minGridX = m_minGridY = 0;
            }
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        /// <summary>
        /// Set a tile data at the grid position
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="tileId"></param>
        /// <param name="brushId"></param>
        /// <param name="flags"></param>
        public void SetTile(int gridX, int gridY, int tileId, int brushId = Tileset.k_BrushId_Default, eTileFlags flags = eTileFlags.None)
        {
            uint tileData = ((uint)flags << 28) | (((uint)brushId << 16) & Tileset.k_TileDataMask_BrushId) | ((uint)tileId & Tileset.k_TileDataMask_TileId);
            SetTileData(gridX, gridY, tileData);
        }

        #region Private Methods 
        Dictionary<uint, TilemapChunk> m_dicChunkCache = new Dictionary<uint, TilemapChunk>();
        private TilemapChunk GetOrCreateTileChunk(int gridX, int gridY, bool createIfDoesntExist = false)
        {
            if (m_dicChunkCache.Count == 0 && transform.childCount > 0)
                BuildTilechunkDictionary();

            int chunkX = (gridX < 0 ? (gridX + 1 - k_chunkSize) : gridX) / k_chunkSize;
            int chunkY = (gridY < 0 ? (gridY + 1 - k_chunkSize) : gridY) / k_chunkSize;

            TilemapChunk tilemapChunk = null;

            uint key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));
            m_dicChunkCache.TryGetValue(key, out tilemapChunk);

            if (tilemapChunk == null && createIfDoesntExist)
            {
                string chunkName = chunkX + "_" + chunkY;
                GameObject chunkObj = new GameObject(chunkName);
                tilemapChunk = chunkObj.AddComponent<TilemapChunk>(); //NOTE: this call TilemapChunk.OnEnable before initializing the TilemapChunk. Make all changes after this.
                chunkObj.transform.parent = transform;
                chunkObj.transform.localPosition = new Vector2(chunkX * k_chunkSize * CellSize.x, chunkY * k_chunkSize * CellSize.y);
                chunkObj.transform.localRotation = Quaternion.identity;
                chunkObj.transform.localScale = Vector3.one;
                chunkObj.hideFlags = gameObject.hideFlags | HideFlags.HideInHierarchy; //NOTE: note the flags inheritance. BrushBehaviour object is not saved, so chunks are left orphans unless this inheritance is done
                // Reset is not called after AddComponent while in play
                if (Application.isPlaying)
                {
                    tilemapChunk.Reset();
                }
                tilemapChunk.ParentTilemap = this;
                tilemapChunk.GridPosX = chunkX * k_chunkSize;
                tilemapChunk.GridPosY = chunkY * k_chunkSize;
                tilemapChunk.SetDimensions(k_chunkSize, k_chunkSize);
                tilemapChunk.SetSharedMaterial(Material);
                tilemapChunk.SortingLayerID = m_sortingLayer;
                tilemapChunk.OrderInLayer = m_orderInLayer;
                tilemapChunk.UpdateRendererProperties();

                m_dicChunkCache[key] = tilemapChunk;
            }

            return tilemapChunk;
        }

        private void BuildTilechunkDictionary()
        {
            m_dicChunkCache.Clear();
            for (int i = 0; i < transform.childCount; ++i)
            {
                TilemapChunk chunk = transform.GetChild(i).GetComponent<TilemapChunk>();
                if (chunk)
                {
                    int chunkX = (chunk.GridPosX < 0 ? (chunk.GridPosX + 1 - k_chunkSize) : chunk.GridPosX) / k_chunkSize;
                    int chunkY = (chunk.GridPosY < 0 ? (chunk.GridPosY + 1 - k_chunkSize) : chunk.GridPosY) / k_chunkSize;
                    uint key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));
                    m_dicChunkCache[key] = chunk;
                }
            }
        }
#endregion
    }
}