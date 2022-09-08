using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTilesInBounds : MonoBehaviour
{
    public GibType gibType;
    public int gibsPerTile;
    public bool cameraShake;
    private BoxCollider2D _boxCollider2D;

    public STETilemap tilemap;

    public void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        if (!tilemap) { tilemap = GameObject.FindGameObjectWithTag("MainTilemap").GetComponent<STETilemap>(); }
    }

    public void DestroyTilesImmediate()
    {
        if (!tilemap)
        {
            Debug.LogError("No tilemap assigned to DestroyTileInBounds on " + gameObject.name);
            return;
        }

        var bounds = _boxCollider2D.bounds;

        var min = bounds.min + new Vector3(0.5f, 0.5f);
        var width = bounds.extents.x * 2;
        var height = bounds.extents.y * 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = min + new Vector3(x, y);
                tilemap.Erase(tilemap.transform.InverseTransformPoint(pos));
            }
        }

        tilemap.UpdateMesh();
        Destroy(gameObject);
    }

    public void DestroyTiles()
    {
        StartCoroutine(WaitDestroyTiles());
    }

    private IEnumerator WaitDestroyTiles()
    {
        yield return new WaitForEndOfFrame();

        if (!tilemap)
        {
            Debug.LogError("No tilemap assigned to DestroyTileInBounds on " + gameObject.name);
            yield break;
        }

        var bounds = _boxCollider2D.bounds;

        var min = bounds.min + new Vector3(0.5f,0.5f);
        var width = bounds.extents.x * 2;
        var height = bounds.extents.y * 2;
        var blocksDestroy = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = min + new Vector3(x, y);
                tilemap.Erase(tilemap.transform.InverseTransformPoint(pos));
                blocksDestroy++;

                if(gibsPerTile > 0 && GibManager.instance)
                {
                    GibManager.instance.SpawnGibs(gibType, pos, gibsPerTile);
                }
            }
        }

        if(cameraShake)
        {
            MainCamera.instance.Shake(0.5f);
        }

        tilemap.UpdateMesh();
        
        var slot = SaveGameManager.activeSlot;
        var game = SaveGameManager.activeGame;
        if (slot != null && slot.totalBlocksDestroyed < long.MaxValue && (game == null || game.allowAchievements))
        {
            slot.totalBlocksDestroyed += blocksDestroy;
        }

        Destroy(gameObject);
    }
}
