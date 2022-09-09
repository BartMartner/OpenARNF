using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreativeSpore.SuperTilemapEditor
{

    [RequireComponent(typeof(STETilemap))]
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    public class BrushBehaviour : MonoBehaviour
    {
#region Singleton
        static BrushBehaviour s_instance;
        public static bool Exists { get { return s_instance; } }
        public static BrushBehaviour Instance
        {
            get
            {
                if (s_instance == null)
                {
                    BrushBehaviour[] brushes = FindObjectsOfType<BrushBehaviour>();
                    if (brushes.Length == 0)
                    {
                        GameObject obj = new GameObject("Brush");
                        s_instance = obj.AddComponent<BrushBehaviour>();
                        obj.hideFlags = HideFlags.HideInHierarchy;
                    }
                    else
                    {
                        s_instance = brushes[0];
                        // Destroy the rest of brushes if any for any possible bug
                        if (brushes.Length > 1)
                        {
                            Debug.LogWarning("More than one brush found. Removing rest of brushes...");
                            for (int i = 1; i < brushes.Length; ++i)
                            {
                                DestroyImmediate(brushes[i]);
                            }
                        }
                    }
                }
                return s_instance;
            }
        }
#endregion

        public enum eBrushPaintMode
        {
            Pencil,
            Line,
            Rect,
            FilledRect,
            Ellipse,
            FilledEllipse,
        }

        public STETilemap BrushTilemap { get { return m_brushTilemap; } }
        public Vector2 Offset;
        public bool IsUndoEnabled = true;
        public eBrushPaintMode PaintMode { get { return m_paintMode; } set { m_paintMode = value; } }
        public bool IsDragging { get { return m_isDragging; } }

        [SerializeField]
        STETilemap m_brushTilemap;

        private eBrushPaintMode m_paintMode = eBrushPaintMode.Pencil;

#region MonoBehaviour Methods
        void Start()
        {
            if(s_instance != this)
            {
                DestroyImmediate(gameObject);
            }
        }

        void OnDestroy()
        {
            if (m_brushTilemap != null)
            {
                m_brushTilemap.ClearMap();
            }
        }
#endregion
        public static void SetVisible(bool isVisible)
        {
            if (s_instance && s_instance.BrushTilemap)
            {
                s_instance.BrushTilemap.IsVisible = isVisible;
                for (int i = 0; i < s_instance.transform.childCount; ++i)
                {
                    s_instance.transform.GetChild(i).gameObject.SetActive(isVisible);
                }
            }
        }

        public static Tileset GetBrushTileset()
        {
            if (s_instance && s_instance.BrushTilemap)
            {
                return s_instance.BrushTilemap.Tileset;
            }
            return null;
        }

        public static bool IsBrushTilemap(STETilemap tilemap)
        {
            if (s_instance)
            {
                return s_instance.BrushTilemap == tilemap;
            }
            return false;
        }

        public static TileSelection CreateTileSelection()
        {
            if (s_instance && s_instance.BrushTilemap && (s_instance.BrushTilemap.GridWidth * s_instance.BrushTilemap.GridHeight > 1) )
            {
                List<uint> selectionData = new List<uint>(s_instance.BrushTilemap.GridWidth * s_instance.BrushTilemap.GridHeight);
                for(int gy = 0; gy < s_instance.BrushTilemap.GridHeight; ++gy)
                {
                    for(int gx = 0; gx < s_instance.BrushTilemap.GridWidth; ++gx)
                    {
                        selectionData.Add(s_instance.BrushTilemap.GetTileData(gx, s_instance.BrushTilemap.GridHeight - gy - 1));
                    }
                }
                return new TileSelection(selectionData, s_instance.BrushTilemap.GridWidth);
            }
            return null;
        }

        public static void SFlipV(){ if(s_instance) s_instance.FlipV(); }
        public static void SFlipH(){ if(s_instance) s_instance.FlipH(); }
        public static void SRot90(){ if(s_instance) s_instance.Rot90(); }
        public static void SRot90Back() { if (s_instance) s_instance.Rot90Back(); }

#region Drawing Methods

        public void FlipH(bool changeFlags = true)
        {
            m_brushTilemap.FlipH(changeFlags);
            m_brushTilemap.UpdateMeshImmediate();
        }

        public void FlipV(bool changeFlags = true)
        {
            m_brushTilemap.FlipV(changeFlags);
            m_brushTilemap.UpdateMeshImmediate();
        }

        public void Rot90(bool changeFlags = true)
        {

            int gridX = BrushUtil.GetGridX(-Offset, m_brushTilemap.CellSize);
            int gridY = BrushUtil.GetGridY(-Offset, m_brushTilemap.CellSize);

            Offset = -new Vector2(gridY * m_brushTilemap.CellSize.x, (m_brushTilemap.GridWidth - gridX - 1) * m_brushTilemap.CellSize.y);

            m_brushTilemap.Rot90(changeFlags);
            m_brushTilemap.UpdateMeshImmediate();
        }

        public void Rot90Back(bool changeFlags = true)
        {

            //NOTE: This is a fast way to rotate back 90º by rotating forward 3 times
            for (int i = 0; i < 3; ++i)
            {
                int gridX = BrushUtil.GetGridX(-Offset, m_brushTilemap.CellSize);
                int gridY = BrushUtil.GetGridY(-Offset, m_brushTilemap.CellSize);
                Offset = -new Vector2(gridY * m_brushTilemap.CellSize.x, (m_brushTilemap.GridWidth - gridX - 1) * m_brushTilemap.CellSize.y);
                m_brushTilemap.Rot90(changeFlags);
            }

            m_brushTilemap.UpdateMeshImmediate();
        }

        private Vector2 m_pressedPosition;
        private uint[,] m_brushPattern;
        private bool m_isDragging;

        public uint[,] GetBrushPattern()
        {
            uint[,] brushPattern = new uint[BrushTilemap.GridWidth, BrushTilemap.GridHeight];
            for (int y = BrushTilemap.MinGridY; y <= BrushTilemap.MaxGridY; ++y)
                for (int x = BrushTilemap.MinGridX; x <= BrushTilemap.MaxGridX; ++x)
                {
                    brushPattern[x - BrushTilemap.MinGridX, y - BrushTilemap.MinGridY] = BrushTilemap.GetTileData(x, y);
                }
            return brushPattern;
        }

        public void DoPaintPressed(STETilemap tilemap, Vector2 localPos, EventModifiers modifiers = default(EventModifiers))
        {
            //Debug.Log("DoPaintPressed (" + TilemapUtils.GetGridX(tilemap, localPos) + "," + TilemapUtils.GetGridY(tilemap, localPos) + ")");            
            if (m_paintMode == eBrushPaintMode.Pencil) Paint(tilemap, localPos);
            else
            {
                m_pressedPosition = localPos;
                m_isDragging = true;
                Offset = Vector2.zero;
                m_brushPattern = GetBrushPattern();
                bool isSingleEmptyTile = BrushTilemap.GridWidth == 1 && BrushTilemap.GridHeight == 1 && BrushTilemap.GetTileData(0, 0) == Tileset.k_TileData_Empty;
                if (isSingleEmptyTile)
                    Paint(tilemap, localPos);
            }
        }

        public void Paint(STETilemap tilemap, Vector2 localPos, bool skipEmptyTiles = false)
        {
            int minGridX = m_brushTilemap.MinGridX;
            int minGridY = m_brushTilemap.MinGridY;
            int maxGridX = m_brushTilemap.MaxGridX;
            int maxGridY = m_brushTilemap.MaxGridY;

            if (IsUndoEnabled)
            {
#if UNITY_EDITOR
                Undo.RecordObject(tilemap, STETilemap.k_UndoOpName + tilemap.name);
                Undo.RecordObjects(tilemap.GetComponentsInChildren<TilemapChunk>(), STETilemap.k_UndoOpName + tilemap.name);
#endif
            }
            tilemap.IsUndoEnabled = IsUndoEnabled;
            int dstGy = BrushUtil.GetGridY(localPos, tilemap.CellSize);
            bool doPaintEmpty = m_brushTilemap.GridWidth == 1 && m_brushTilemap.GridHeight == 1 // don't copy empty tiles
                        || m_brushPattern != null && m_brushPattern.GetLength(0) == 1 && m_brushPattern.GetLength(1) == 1;// unless the brush size is one
            doPaintEmpty &= !skipEmptyTiles;
            for (int gridY = minGridY; gridY <= maxGridY; ++gridY, ++dstGy)
            {
                int dstGx = BrushUtil.GetGridX(localPos, tilemap.CellSize);
                for (int gridX = minGridX; gridX <= maxGridX; ++gridX, ++dstGx)
                {
                    uint tileData = m_brushTilemap.GetTileData(gridX, gridY);
                    if (
                        doPaintEmpty ||
                        tileData != Tileset.k_TileData_Empty
                        )
                    {
                        tilemap.SetTileData(dstGx, dstGy, tileData);
                    }
                }
            }
            tilemap.UpdateMeshImmediate();
            tilemap.IsUndoEnabled = false;
        }

        public void Erase(STETilemap tilemap, Vector2 localPos)
        {
            int minGridX = m_brushTilemap.MinGridX;
            int minGridY = m_brushTilemap.MinGridY;
            int maxGridX = m_brushTilemap.MaxGridX;
            int maxGridY = m_brushTilemap.MaxGridY;

            if (IsUndoEnabled)
            {
//#if UNITY_EDITOR
//                Undo.RecordObject(tilemap, STETilemap.k_UndoOpName + tilemap.name);
//                Undo.RecordObjects(tilemap.GetComponentsInChildren<TilemapChunk>(), STETilemap.k_UndoOpName + tilemap.name);
//#endif
            }
            tilemap.IsUndoEnabled = IsUndoEnabled;
            int dstGy = BrushUtil.GetGridY(localPos, tilemap.CellSize);
            for (int gridY = minGridY; gridY <= maxGridY; ++gridY, ++dstGy)
            {
                int dstGx = BrushUtil.GetGridX(localPos, tilemap.CellSize);
                for (int gridX = minGridX; gridX <= maxGridX; ++gridX, ++dstGx)
                {
                    tilemap.SetTileData(dstGx, dstGy, Tileset.k_TileData_Empty);
                }
            }
            tilemap.UpdateMeshImmediate();
            tilemap.IsUndoEnabled = false;
        }

#endregion
    }
}
