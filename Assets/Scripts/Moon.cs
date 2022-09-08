using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour
{
    public Sprite[] sprites;
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        var day = System.DateTime.Now.Day;
        _spriteRenderer.flipX = day < 17;

        if (day < 6)
        {
            _spriteRenderer.sprite = sprites[0];
        }
        else if (day < 9 || day > 27)
        {
            _spriteRenderer.sprite = sprites[1];
        }
        else if (day < 12 || day > 24)
        {
            _spriteRenderer.sprite = sprites[2];
        }
        else if (day < 15 || day > 21)
        {
            _spriteRenderer.sprite = sprites[3];
        }
        else
        {
            _spriteRenderer.sprite = sprites[4];
        }
    }
}
