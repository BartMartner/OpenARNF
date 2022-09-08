using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ColorPingPong : MonoBehaviour
{
    public float cycleTime;
    public Color color1;
    public Color color2;

    private float _timer;
    private float _halfCycle;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (cycleTime <= 0)
        {
            return;
        }

        _timer += Time.deltaTime;
        _halfCycle = cycleTime / 2;

        if (_timer > cycleTime)
        {
            _timer -= cycleTime;
        }

        if (_timer <= _halfCycle)
        {
            _spriteRenderer.color = Color.Lerp(color1, color2, _timer / _halfCycle);
        }
        else
        {
            _spriteRenderer.color = Color.Lerp(color2, color1, (_timer - _halfCycle) / _halfCycle);
        }
    }
}