using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OozeHartForm1 : MonoBehaviour
{
    public Transform[] poopPoints;
    public CreepStats creepStats;

    public void Poop()
    {
        StartCoroutine(PoopRoutine());
    }

    public IEnumerator PoopRoutine()
    {
        foreach (var point in poopPoints)
        {
            FXManager.instance.TrySpawnCreep(point.position, Vector3.down, 1, creepStats);
            yield return new WaitForSeconds(1f/12f);
        }
    }

    private void OnDestroy()
    {
        FXManager.instance.KillAllCreep();
    }
}
