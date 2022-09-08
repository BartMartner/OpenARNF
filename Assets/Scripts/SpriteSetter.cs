using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSetter : MonoBehaviour
{
    public int spriteIndex;
    public Sprite[] sprites;
    private SpriteRenderer _spriteRenderer;

    private void LateUpdate()
    {
        if (!_spriteRenderer) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
        else if(sprites != null && sprites.Length > 0)
        {
            spriteIndex = Mathf.Abs(spriteIndex) % sprites.Length;
            _spriteRenderer.sprite = sprites[spriteIndex];
        }
    }
}
