using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomSprite : RoomBasedRandom
{
    public Sprite[] sprites;

    [HideInInspector]
    public List<float> weights;

    private SpriteRenderer _spriteRenderer;

    public override void Randomize()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_random == null)
        {
            Debug.LogWarning("_random was null in " + name);
            _random = new XorShift(0);
        }

        if (weights.Count != sprites.Length)
        {
            Debug.LogWarning(name + "'s sprite length doesn't match its weight list count! Correcting!");
            weights = new List<float>(sprites.Length);
            weights.ForEach(w => w = 1);
        }

        float randPercent = _random.Value();
        float sumProbabilityFactor = Mathf.Max(weights.Sum(), float.Epsilon);
        for (int i = 0; i < weights.Count; i++)
        {
            float probability = weights[i] / sumProbabilityFactor;
            if (randPercent <= probability)
            {
                _spriteRenderer.sprite = sprites[i];
                break;
            }
            randPercent -= probability;
        }
    }
}
