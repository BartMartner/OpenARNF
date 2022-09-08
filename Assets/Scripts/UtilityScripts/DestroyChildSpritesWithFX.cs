using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyChildSpritesWithFX : MonoBehaviour
{
    private SpriteRenderer[] _sprites;
    public FXType fxType;
    public float time;

    public void Awake()
    {
        _sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public void DestroySprites()
    {
        StartCoroutine(DestroySpritesSequence());
    }

    private IEnumerator DestroySpritesSequence()
    {        
        var delay = new WaitForSeconds(time / _sprites.Length);
        foreach (var sprite in _sprites)
        {
            FXManager.instance.SpawnFX(fxType, sprite.transform.position);
            Destroy(sprite);
            yield return delay;
        }
    }
}
