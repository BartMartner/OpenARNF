using UnityEngine;
using System.Collections;

public class SetColorBasedOnCorruption : SetBasedOnCorruption
{
    public string target = "_MainColor";
    public Color[] colors;
    public bool setSpriteRendererColor = true;
    private Renderer[] _renderers;

    public override void SetCorruption()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in _renderers)
        {
            var minIndex = Mathf.FloorToInt(Mathf.Lerp(0, colors.Length - 1, _adjustedCorruption));
            var maxIndex = Mathf.CeilToInt(Mathf.Lerp(0, colors.Length - 1, _adjustedCorruption));
            var min = colors[minIndex];
            var max = colors[maxIndex];
            var color = Color.Lerp(min, max, _adjustedCorruption);
            if (setSpriteRendererColor)
            {
                var sr = r as SpriteRenderer;
                if(sr)
                {
                    sr.color = color;
                    return;
                }
            }
            
            r.material.SetColor(target, color);            
        }
    }
}
