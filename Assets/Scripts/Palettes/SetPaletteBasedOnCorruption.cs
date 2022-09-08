using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPaletteBasedOnCorruption : SetBasedOnCorruption
{
    public Texture2D[] textures;

    private Renderer _renderer;

    public override void SetCorruption()
    {
        if (textures.Length <= 0)
        {
            Debug.LogWarning("SetSpriteBasedOnRoomsVisited attached to " + gameObject.name + " has an empty sprites array");
            return;
        }

        var index = Mathf.RoundToInt(Mathf.Lerp(0, textures.Length - 1, _adjustedCorruption));

        _renderer = GetComponent<Renderer>();
        if (_renderer.material.HasProperty("_Palette"))
        {
            _renderer.material.SetTexture("_Palette", textures[index]);
        }        
    }
}
