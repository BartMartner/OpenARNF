using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorReskin : MonoBehaviour
{
    public Sprite[] sourceSprites;
    public Sprite[] altSprites;
    public string sourcePath;
    public string altPath;
    private Dictionary<string, Sprite> _alternateSprites;
    private SpriteRenderer _spriteRenderer;

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _alternateSprites = new Dictionary<string, Sprite>();
        var alts = !string.IsNullOrEmpty(altPath) ? Resources.LoadAll<Sprite>(altPath) : altSprites;
        var source = !string.IsNullOrEmpty(sourcePath) ? Resources.LoadAll<Sprite>(sourcePath) : sourceSprites;

        var length = alts.Length < source.Length ? alts.Length : source.Length;
        for (int i = 0; i < length; i++)
        {
            _alternateSprites.Add(source[i].name, alts[i]);
        }

        altSprites = null;
        sourceSprites = null;
    }

    public void LateUpdate()
    {
        Sprite sprite;        
        if (_spriteRenderer.sprite && _alternateSprites.TryGetValue(_spriteRenderer.sprite.name, out sprite))
        {
            _spriteRenderer.sprite = sprite;
        }
    }
}
