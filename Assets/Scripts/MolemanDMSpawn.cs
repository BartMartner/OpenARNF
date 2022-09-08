using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolemanDMSpawn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var slot = SaveGameManager.activeSlot;
        if (slot == null || !DeathmatchManager.instance || slot.deathmatchSettings.molemanSpawnRate <= 0 || slot.deathmatchSettings.maxMolemen <= 0)
        {
            Destroy(gameObject);
        }
    }
}
