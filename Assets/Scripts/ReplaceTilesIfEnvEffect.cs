using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReplaceTilesIfEnvEffect : MonoBehaviour, IAbstractDependantObject
{
    public STETilemap tilemap;
    public RandomBrush toReplace;
    public RandomBrush replaceWith;
    public EnvironmentalEffect envEffect;

    private void Awake()
    {
        if (!tilemap)
        {
            tilemap = GetComponent<STETilemap>();
        }
    }

    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(roomAbstract.environmentalEffect.HasFlag(envEffect))
        {
            ReplaceTiles();
        }
    }

    public void ReplaceTiles()
    {
        for (int x = tilemap.MinGridX; x <= tilemap.MaxGridX; x++)
        {
            for (int y = tilemap.MinGridY; y <= tilemap.MaxGridY; y++)
            {
                var tileData = tilemap.GetTileData(x, y);
                if (tileData == Tileset.k_TileData_Empty) continue;
                for (int i = 0; i < toReplace.RandomTileList.Count; i++)
                {
                    var t = toReplace.RandomTileList[i];
                    int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                    if (t.tileData == tileId)
                    {
                        uint replacement = replaceWith.RandomTileList[i].tileData;
                        if ((tileData & Tileset.k_TileFlag_FlipH) != 0) { replacement |= Tileset.k_TileFlag_FlipH; }
                        if ((tileData & Tileset.k_TileFlag_FlipV) != 0) { replacement |= Tileset.k_TileFlag_FlipV; }
                        if ((tileData & Tileset.k_TileFlag_Rot90) != 0) { replacement |= Tileset.k_TileFlag_Rot90; }
                        tilemap.SetTileData(new Vector2(x, y), replacement);
                    }
                }
            }
        }

        tilemap.UpdateMesh();
    }
}
