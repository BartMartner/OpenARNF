using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteScroll : MonoBehaviour
{    
    public Vector2 speed;
    private SpriteRenderer _spriteRenderer;

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (!_spriteRenderer.material.HasProperty("_OffsetX") || !_spriteRenderer.material.HasProperty("_OffsetY"))
        {
            Debug.LogError("Destroying SpriteScroll script." + gameObject.name + " does not have a material with the _OffsetX and _OffsetY propeties.");
            Destroy(this);
        }
    }

    // Update is called once per frame
    public void Update ()
    {
        var x = _spriteRenderer.material.GetFloat("_OffsetX");
        var y = _spriteRenderer.material.GetFloat("_OffsetY");
        x += (speed.x * Time.deltaTime) % 1;
        y += (speed.y * Time.deltaTime) % 1;
        _spriteRenderer.material.SetFloat("_OffsetX", x);
        _spriteRenderer.material.SetFloat("_OffsetY", y);
    }
}
