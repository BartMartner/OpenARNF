using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SetSpriteBasedOnCorruption : SetBasedOnCorruption
{
    public Sprite[] sprites;

    private SpriteRenderer _spriteRenderer;

    public override void SetCorruption()
    {
        if (sprites.Length <= 0)
        {
            Debug.LogWarning("SetSpriteBasedOnRoomsVisited attached to " + gameObject.name + " has an empty sprites array");
            return;
        }

        var index = Mathf.RoundToInt(Mathf.Lerp(0, sprites.Length - 1, _adjustedCorruption));

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = sprites[index];
    }
}
