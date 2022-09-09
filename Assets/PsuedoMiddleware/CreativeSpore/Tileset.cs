using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CreativeSpore.SuperTilemapEditor
{

    public enum eTileCollider
    {
        None = 0, //default value should be this one
        Full,
        Polygon
    }

    [Serializable]
    public struct TileColliderData
    {
        /// <summary>
        /// The collider vertex array. Only valid for type eTileCollider.Polygon, for other types use GetVertices method.
        /// </summary>
        public Vector2[] vertices;
        public eTileCollider type;
        public TileColliderData Clone()
        {
            if (this.vertices == null) this.vertices = new Vector2[0];
            Vector2[] clonedVertices = new Vector2[this.vertices.Length];
            vertices.CopyTo(clonedVertices, 0);

            return new TileColliderData { vertices = clonedVertices, type = type };
        }

        private static Vector2[] s_fullCollTileVertices = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        /// <summary>
        /// Gets the collider vertex array if type is not eTileCollider.None. 
        /// The difference with vertices property is, this will always return a valid array event if the type is eTileCollider.Full.
        /// vertices is only valid when type is eTileCollider.Polygon
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetVertices()
        {
            switch (type)
            {
                case eTileCollider.None: return null;
                case eTileCollider.Full: return s_fullCollTileVertices;
                case eTileCollider.Polygon: return vertices;
                default: return null;
            }
        }

        public static Vector2 SnapVertex(Vector2 vertex, Tileset tileset)
        {
            vertex.x = Mathf.Clamp01(Mathf.RoundToInt(vertex.x * tileset.TilePxSize.x) / tileset.TilePxSize.x);
            vertex.y = Mathf.Clamp01(Mathf.RoundToInt(vertex.y * tileset.TilePxSize.y) / tileset.TilePxSize.y);
            return vertex;
        }

        /// <summary>
        /// Snap vertices positions according to the tileset tile size in pixels. This way, the tile colliders will be seamless avoiding precision erros.
        /// </summary>
        /// <param name="tileset"></param>
        public void SnapVertices(Tileset tileset)
        {
            if (vertices != null)
                for (int i = 0; i < vertices.Length; ++i)
                    vertices[i] = SnapVertex(vertices[i], tileset);
        }

        public void ApplyFlippingFlags(uint tileData)
        {
            if ((tileData & Tileset.k_TileFlag_FlipH) != 0) FlipH();
            if ((tileData & Tileset.k_TileFlag_FlipV) != 0) FlipV();
            if ((tileData & Tileset.k_TileFlag_Rot90) != 0) Rot90();
        }

        public void RemoveFlippingFlags(uint tileData)
        {
            if ((tileData & Tileset.k_TileFlag_Rot90) != 0) Rot90Back();
            if ((tileData & Tileset.k_TileFlag_FlipV) != 0) FlipV();
            if ((tileData & Tileset.k_TileFlag_FlipH) != 0) FlipH();
        }

        public void FlipH()
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].x = 1f - vertices[i].x;
            }
            Array.Reverse(vertices);
        }

        public void FlipV()
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].y = 1f - vertices[i].y;
            }
            Array.Reverse(vertices);
        }

        public void Rot90()
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                float tempX = vertices[i].x;
                vertices[i].x = vertices[i].y;
                vertices[i].y = tempX;
                vertices[i].y = 1f - vertices[i].y;
            }
        }

        public void Rot90Back()
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].y = 1f - vertices[i].y;
                float tempX = vertices[i].x;
                vertices[i].x = vertices[i].y;
                vertices[i].y = tempX;
            }
        }
    }

    [Serializable]
    public struct TilePrefabData
    {
        public enum eOffsetMode
        {
            Pixels,
            Units,
        };

        public GameObject prefab;
        public Vector3 offset;
        public Vector3 rotation;
        public eOffsetMode offsetMode;
        /// <summary>
        /// If the tile should be hidden or not if the prefab is attached
        /// </summary>
        public bool showTileWithPrefab;
        public bool showPrefabPreviewInTilePalette;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;

            TilePrefabData other = (TilePrefabData)obj;
            return (other.prefab == this.prefab) && (other.offset == this.offset) && (other.offsetMode == this.offsetMode);
        }

        public override int GetHashCode() { return base.GetHashCode(); }
        public static bool operator ==(TilePrefabData c1, TilePrefabData c2) { return c1.Equals(c2); }
        public static bool operator !=(TilePrefabData c1, TilePrefabData c2) { return !c1.Equals(c2); }
    }

    [Serializable]
    public class Tile
    {
        public Rect uv;
        public TileColliderData collData;
        public ParameterContainer paramContainer = new ParameterContainer();
        public TilePrefabData prefabData;
        public int autilingGroup = 0;
    }

    [Serializable]
    public class TileSelection
    {
        public List<uint> selectionData { get { return m_tileIds; } }
        public int rowLength { get { return m_rowLength; } }
        public int columnLength { get { return 1 + (m_tileIds.Count - 1) / m_rowLength; } }

        [SerializeField]
        private int m_rowLength = 1;
        [SerializeField]
        private List<uint> m_tileIds; //NOTE: name remains for compatibility but now it contains tileData instead of only the id

        public TileSelection() : this(null, 0) { } //NOTE: fix some errors with serialization
        public TileSelection(List<uint> tileIds, int rowLength)
        {
            m_tileIds = tileIds != null ? tileIds : new List<uint>();
            m_rowLength = Mathf.Max(1, rowLength);
        }


        public TileSelection Clone()
        {
            List<uint> tileIds = new List<uint>(m_tileIds);
            int rowLength = m_rowLength;
            return new TileSelection(tileIds, rowLength);
        }

        public void FlipVertical()
        {
            List<uint> flipedTileIds = new List<uint>();
            int totalRows = 1 + (m_tileIds.Count - 1) / rowLength;
            for (int y = totalRows - 1; y >= 0; --y)
            {
                for (int x = 0; x < rowLength; ++x)
                {
                    int idx = y * rowLength + x;
                    flipedTileIds.Add(m_tileIds[idx]);
                }
            }
            m_tileIds = flipedTileIds;
        }
    }

    [Serializable]
    public class TileView
    {
        public TileView(string name, TileSelection tileSelection)
        {
            m_name = name;
            m_tileSelection = tileSelection;
        }

        public string name { get { return m_name; } }
        public TileSelection tileSelection { get { return m_tileSelection; } }

        [SerializeField]
        private string m_name;
        [SerializeField]
        private TileSelection m_tileSelection;
    }

    public class Tileset : ScriptableObject
    {
        public const int k_TileId_Empty = 0x0000FFFF; //NOTE: same value as k_TileDataMask_TileId
        //NOTE: if the tileData is empty 0xFFFFFFFF, the GetBrushIdFromTileData will return -1. Sometimes it's better "if(brushId > 0)" to check if there is a valid brush
        public const int k_BrushId_Default = 0; // brush id 0 is used for default brush (used by tiles drawn without using a brush asset)
        public const uint k_TileData_Empty = 0xFFFFFFFF;
        // Tile Data Masks
        public const uint k_TileDataMask_TileId = 0x0000FFFF; // up to 256x256(65536 - 1) tiles (in a max. texture size of 8192x8192, min. tile size should be 32x32)
        public const uint k_TileDataMask_BrushId = 0x0FFF0000; // up to 4096 - 1 ( id 0 is used for undefined brush )
        public const uint k_TileDataMask_Flags = 0xF0000000; // Flags: (1bit)FlipX, (1bit)FlipY, (1bits)Rot90, (1 bit reserved)
        // Tile Data Flags
        public const uint k_TileFlag_FlipV = 0x80000000;
        public const uint k_TileFlag_FlipH = 0x40000000;
        public const uint k_TileFlag_Rot90 = 0x20000000;
        public const uint k_TileFlag_Updated = 0x10000000; // used by brushes to check when a tile should be updated or not

        public static int GetBrushIdFromTileData(uint tileData) { return tileData != k_TileData_Empty ? (int)((tileData & k_TileDataMask_BrushId) >> 16) : -1; }
        public static int GetTileIdFromTileData(uint tileData) { return (int)(tileData & k_TileDataMask_TileId); }
        public static uint GetTileFlagsFromTileData(uint tileData) { return (tileData & k_TileDataMask_Flags); }

        #region Public Events
        public delegate void OnTileSelectedDelegate(Tileset source, int prevTileId, int newTileId);
        public OnTileSelectedDelegate OnTileSelected;

        public delegate void OnTileSelectionChangedDelegate(Tileset source);
        public OnTileSelectionChangedDelegate OnTileSelectionChanged;

        public delegate void OnBrushSelectedDelegate(Tileset source, int prevBrushId, int newBrushId);
        public OnBrushSelectedDelegate OnBrushSelected;
        #endregion

        #region Public Properties
        public Texture2D AtlasTexture;
        public Vector2 TilePxSize = new Vector2(32, 32);
        public Vector2 SliceOffset;
        public Vector2 SlicePadding;
        public Color BackgroundColor = new Color32(205, 205, 205, 205);

        public Vector2 VisualTileSize = new Vector2(32, 32);
        public int VisualTilePadding = 1;
        public int TileRowLength = 8;

        /// <summary>
        /// Number of tiles in a row
        /// </summary>
        public int Width { get { return m_tilesetWidth > 0 ? m_tilesetWidth : Mathf.RoundToInt(AtlasTexture.width / TilePxSize.x); } set { m_tilesetWidth = Mathf.Clamp(value, 1, Tiles.Count); } }
        /// <summary>
        /// Number of tiles in a  column
        /// </summary>
        public int Height { get { return 1 + (Tiles.Count - 1) / Width; } }//{ get { return m_tilesetHeight > 0 ? m_tilesetHeight : Mathf.RoundToInt(AtlasTexture.height / TilePxSize.y); } }

        public bool GetGroupAutotiling(int groupA, int groupB)
        {
            return (m_brushGroupAutotilingMatrix[groupA] & (1u << groupB)) != 0;
        }
        public void SetGroupAutotiling(int groupA, int groupB, bool value)
        {
            if (value)
                m_brushGroupAutotilingMatrix[groupA] |= (1u << groupB);
            else
                m_brushGroupAutotilingMatrix[groupA] &= ~(1u << groupB);
        }
        public string[] BrushGroupNames { get { return m_brushGroupNames; } }

        [Serializable]
        public struct BrushContainer
        {
            public int Id; // should be > 0
            public TilesetBrush BrushAsset;
        }

        public Tile SelectedTile { get { return SelectedTileId != k_TileId_Empty ? m_tiles[SelectedTileId] : null; } }
        public List<BrushContainer> Brushes { get { return m_brushes; } }
        //public IList<Tile> Tiles { get { return m_tiles.AsReadOnly(); } } //NOTE: removed AsReadOnly for performance and for removing memory allocation
        public List<Tile> Tiles { get { return m_tiles; } }
        public float PixelsPerUnit { get { return m_pixelsPerUnit; } set { m_pixelsPerUnit = value; } }
        public void SetTiles(List<Tile> tiles) { m_tiles = tiles; }
        public Vector2 CalculateTileTexelSize() { return AtlasTexture != null ? Vector2.Scale(AtlasTexture.texelSize, TilePxSize) : Vector2.zero; }
        public List<TileView> TileViews { get { return m_tileViews; } }
        public int BrushTypeMask { get { return m_brushTypeMask; } set { m_brushTypeMask = value; } }

        public int SelectedTileId
        {
            get
            {
                if (m_selectedTileId >= Tiles.Count || m_selectedTileId < 0)
                {
                    m_selectedTileId = k_TileId_Empty;
                }
                return m_selectedTileId;
            }

            set
            {
                int prevTileId = m_selectedTileId;
                m_selectedTileId = value;
                //if (m_selectedTileId != k_TileId_Empty) // commented to fix select empty tile from tilemap
                {
                    m_tileSelection = null;
                    m_selectedBrushId = k_BrushId_Default;
                }
                //Debug.Log("SelectedTileId: " + SelectedTileId);
                if (OnTileSelected != null)
                {
                    OnTileSelected(this, prevTileId, m_selectedTileId);
                }
            }
        }

        public int SelectedBrushId
        {
            get { return m_selectedBrushId; }
            set
            {
                int prevBrushId = m_selectedBrushId;
                m_selectedBrushId = Mathf.Clamp(value, -1, m_tiles.Count - 1);
                m_selectedBrushId = (int)(m_selectedBrushId & k_TileDataMask_TileId); // convert -1 in k_TileId_Empty            

                //if (m_selectedBrushId != k_BrushId_Empty) // commented to fix select empty tile from tilemap
                {
                    m_selectedTileId = k_TileId_Empty;
                    m_tileSelection = null;
                }
                if (OnBrushSelected != null)
                {
                    OnBrushSelected(this, prevBrushId, m_selectedBrushId);
                }
            }
        }

        public TileSelection TileSelection
        {
            get
            {
                m_tileSelection = (m_tileSelection != null && m_tileSelection.selectionData != null && m_tileSelection.selectionData.Count > 0) ? m_tileSelection : null; //NOTE: sometimes m_tileSelection.tileIds has no tiles, even with the "set" check
                return m_tileSelection;
            }
            set
            {
                TileSelection prevValue = m_tileSelection;
                m_tileSelection = (value != null && value.selectionData != null && value.selectionData.Count > 0) ? value : null;
                if (m_tileSelection != null)
                {
                    m_selectedTileId = k_TileId_Empty;
                    m_selectedBrushId = k_BrushId_Default;
                }
                if (prevValue != m_tileSelection && OnTileSelectionChanged != null)
                {
                    OnTileSelectionChanged(this);
                }
            }
        }

        #endregion

        #region Private Fields
        [SerializeField]
        private List<TileView> m_tileViews = new List<TileView>();
        [SerializeField]
        private int m_tilesetWidth = 0;
        [SerializeField]
        private int m_tilesetHeight = 0;
        [SerializeField]
        private List<BrushContainer> m_brushes = new List<BrushContainer>();
        [SerializeField]
        private List<Tile> m_tiles = new List<Tile>();
        [SerializeField, Tooltip("Used only to set the initial cell size when a new tilemap is created")]
        private float m_pixelsPerUnit = 100f;
        [SerializeField]
        private string[] m_brushGroupNames = Enumerable.Range(0, 32).Select(x => x == 0 ? "Default" : "").ToArray();
        [SerializeField]
        private uint[] m_brushGroupAutotilingMatrix = Enumerable.Range(0, 31).Select(x => 1u << x).ToArray();
        [SerializeField]
        private string[] m_brushTypeMaskOptions;
        [SerializeField]
        private int m_brushTypeMask = -1;

        private int m_selectedTileId = k_TileId_Empty;
        private int m_selectedBrushId = 0;
        private TileSelection m_tileSelection = null;
        #endregion

        #region Public Methods

        public Tile GetTile(int tileId)
        {
            if (tileId >= 0 && tileId < m_tiles.Count)
            {
                return m_tiles[tileId];
            }
            return null;
        }

        private BrushContainer FindBrushContainerByBrushId(int brushId)
        {
            for (int i = 0; i < m_brushes.Count; ++i) if (m_brushes[i].Id == brushId) return m_brushes[i];
            return default(BrushContainer);
        }

        Dictionary<int, TilesetBrush> m_brushCache = new Dictionary<int, TilesetBrush>();
        public TilesetBrush FindBrush(int brushId)
        {
            if (brushId <= 0) return null;

            TilesetBrush tileBrush = null;
            if (!m_brushCache.TryGetValue(brushId, out tileBrush))
            {
                tileBrush = FindBrushContainerByBrushId(brushId).BrushAsset;
                m_brushCache[brushId] = tileBrush;
                //Debug.Log(" Cache miss! " + tileBrush.name);
            }
            return tileBrush;
        }

        public int FindBrushId(string name)
        {
            for (int i = 0; i < m_brushes.Count; ++i)
            {
                if (m_brushes[i].BrushAsset.name == name) return m_brushes[i].Id;
            }
            return -1;
        }

        public string[] UpdateBrushTypeArray()
        {
            List<string> outList = new List<string>();
            for (int i = 0; i < Brushes.Count; ++i)
            {
                TilesetBrush brush = Brushes[i].BrushAsset;
                if (brush)
                {
                    string type = brush.GetType().Name;
                    if (!outList.Contains(type)) outList.Add(type);
                }
            }
            m_brushTypeMaskOptions = outList.ToArray();
            return m_brushTypeMaskOptions;
        }

        public string[] GetBrushTypeArray()
        {
            if (m_brushTypeMaskOptions == null || m_brushTypeMaskOptions.Length == 0)
                UpdateBrushTypeArray();
            return m_brushTypeMaskOptions;
        }
        #endregion
    }
}