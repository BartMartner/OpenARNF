using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnPrefab : RoomBasedRandom
{
    [Range(0f, 1f)]
    public float spawnNothingChance;
    public GameObject[] prefabs;
    public bool matchRotation;

    [HideInInspector]
    public List<float> weights;

    public override void Randomize()
    {
        GetOneChild();
        Destroy(gameObject);
    }

    public GameObject GetOneChild()
    {
        if (_random == null)
        {
            Debug.LogWarning("_random was null in " + name);
            _random = new XorShift(0);
        }

        if (weights.Count != prefabs.Length)
        {
            Debug.LogWarning(name + "'s count doesn't match its weight list count! Correcting!");
            weights = new List<float>(prefabs.Length);
            weights.ForEach(w => w = 1);
        }

        if (spawnNothingChance > 0 && _random.Value() < spawnNothingChance)
        {
            return null;
        }

        float randPercent = _random.Value();
        float sumProbabilityFactor = Mathf.Max(weights.Sum(), float.Epsilon);
        for (int i = 0; i < weights.Count; i++)
        {
            float probability = weights[i] / sumProbabilityFactor;
            if (randPercent <= probability)
            {
                return Instantiate(prefabs[i], transform.position, (matchRotation ? transform.rotation : Quaternion.identity), transform.parent);
            }
            randPercent -= probability;
        }

        return prefabs.Length > 0 ? prefabs[prefabs.Length - 1] : null;
    }
}

