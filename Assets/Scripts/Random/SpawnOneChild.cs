using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpawnOneChild : RoomBasedRandom 
{
    [Range(0f,1f)]
    public float spawnNothingChance;

    [HideInInspector]
    public List<float> weights;

    public override void Awake()
    {
        if (enabled)
        {
            //without this, transforms with attached MonsterSpawnPoints will roll before this and it may pick empty spawn point transforms.
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(false);
            }
        }

        base.Awake();
    }

    public override void Randomize()
    {
        var salvage = GetOneChild();
        if (salvage != null)
        {
            salvage.SetParent(transform.parent);
            salvage.gameObject.SetActive(true);
        }

        Destroy(gameObject);
    }

    public Transform GetOneChild()
    {
        if (_random == null)
        {
            Debug.LogWarning("_random was null in " + name);
            _random = new XorShift(0);
        }

        if(weights.Count != transform.childCount)
        {
            Debug.LogWarning(name + "'s count doesn't match its weight list count! Correcting!");
            weights = new List<float>(transform.childCount);
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
                return transform.GetChild(i);
            }
            randPercent -= probability;
        }

        return transform.childCount > 0 ? transform.GetChild(transform.childCount - 1) : null;
    }
}
