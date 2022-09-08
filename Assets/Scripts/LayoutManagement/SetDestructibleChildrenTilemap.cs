using UnityEngine;
using System.Collections;
using CreativeSpore.SuperTilemapEditor;

public class SetDestructibleChildrenTilemap : MonoBehaviour
{
    public STETilemap tilemap;

    public void Awake()
    {
        var destructibles = GetComponentsInChildren<DestructibleTileBounds>();
        foreach (var tile in destructibles)
        {
            tile.tilemap = tilemap;
        }
    }
}
