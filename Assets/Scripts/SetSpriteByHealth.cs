using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SetSpriteByHealth : MonoBehaviour
{
    public Sprite[] sprites;
    private Damageable _damageable;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _damageable = GetComponent<Damageable>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _damageable.onHurt.AddListener(OnHurt);
    }
    
    void OnHurt()
    {
        var progress = _damageable.health / _damageable.maxHealth;
        _spriteRenderer.sprite = sprites[Mathf.RoundToInt(Mathf.Lerp(0, sprites.Length-1, progress))];
    }

    private void OnDestroy()
    {
        if (_damageable)
        {
            _damageable.onHurt.RemoveListener(OnHurt);
        }
    }
}
