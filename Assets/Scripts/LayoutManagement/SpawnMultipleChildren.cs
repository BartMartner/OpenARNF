using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMultipleChildren : RoomBasedRandom
{
    public int minChildrenToSpawn;
    public int maxChildrenToSpawn;

    public override void Awake()
    {
        //without this, transforms with attached MonsterSpawnPoints will roll before this and it may pick empty spawn point transforms.
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }

        base.Awake();
    }

    public override void Randomize()
    {
        if (_random == null)
        {
            Debug.LogWarning("_random was null in " + name);
            _random = new XorShift(0);
        }

        var times = Mathf.Clamp(_random.Range(minChildrenToSpawn, maxChildrenToSpawn+1), 0, transform.childCount);

        for (int i = 0; i < times; i++)
        {
            var salvage = transform.GetChild(_random.Range(0, transform.childCount));
            salvage.SetParent(transform.parent);
            salvage.gameObject.SetActive(true);
            //Debug.Log(salvage.name);
        }

        Destroy(gameObject);
    }
}
