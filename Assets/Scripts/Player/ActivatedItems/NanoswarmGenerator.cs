using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NanoswarmGenerator", menuName = "Player Activated Items/Nanoswarm Generator", order = 1)]
public class NanoswarmGenerator : PlayerActivatedItem
{
    public AudioClip spawnSound;

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (Usable())
        {
            _player.energy -= energyCost;
            _player.SpawnNanobot(_player.transform.position);

            if (spawnSound)
            {
                _player.PlayOneShot(spawnSound);
            }
        }
    }
}
