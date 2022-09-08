using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Row4Brush))]
public class Row4BrushEditor //: TilesetBrushEditor
{
    //[MenuItem("Assets/Create/SuperTilemapEditor/Brush/Row4Brush", priority = 50)]
    //public static Row4Brush CreateAsset()
    //{
    //    return EditorUtils.CreateAssetInSelectedDirectory<Row4Brush>();
    //}

    //Row4Brush m_brush;

    //BrushTileGridControl m_brushTileGridControl = new BrushTileGridControl();
    //public override void OnEnable()
    //{
    //    base.OnEnable();
    //    m_brush = (Row4Brush)target;
    //}

    //void OnDisable()
    //{
    //    m_brushTileGridControl.Tileset = null; // avoid receiving OnTileSelection
    //}

    //static int[] s_tileIdxMap = new int[]
    //{
    //    0, 1, 2, 3,
    //};

    //static int[] s_symbolIdxMap = new int[]
    //{
    //    4, 4,
    //    4, 4,
    //};

    //public override void OnInspectorGUI()
    //{
    //    base.OnInspectorGUI();
    //    if (!m_brush.Tileset) return;

    //    serializedObject.Update();
    //    EditorGUILayout.Space();

    //    m_brushTileGridControl.Tileset = m_brush.Tileset;
    //    m_brushTileGridControl.Display(target, m_brush.TileIds, s_tileIdxMap, 2, 2, m_brush.Tileset.VisualTileSize, s_symbolIdxMap);

    //    Repaint();
    //    serializedObject.ApplyModifiedProperties();
    //    if (GUI.changed)
    //    {
    //        EditorUtility.SetDirty(target);
    //    }
    //}
}
