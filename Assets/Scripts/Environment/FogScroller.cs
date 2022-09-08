using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogScroller : MonoBehaviour
{
    public float scrollSpeed = 1;
    SpriteRenderer[] _renderers;

    public void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in _renderers)
        {
            r.material.SetFloat("_OffsetX", Random.Range(0f, 1f));
        }
    }

    public void Update()
    {
        foreach (var r in _renderers)
        {
            var o = r.material.GetFloat("_OffsetX");
            var speed = scrollSpeed * 2 * r.color.a * Time.deltaTime;
            r.material.SetFloat("_OffsetX", (o + speed) % 1);
        }
    }
}
