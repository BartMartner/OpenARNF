using UnityEngine;
using System.Collections;

public class SpawnChance : RoomBasedRandom
{
    [Range(0.005f, 1f)]
    public float spawnChance = 0.5f;

    public override void Randomize()
    {
        if (_random.Value() > spawnChance) { Destroy(gameObject); }
    }
}
