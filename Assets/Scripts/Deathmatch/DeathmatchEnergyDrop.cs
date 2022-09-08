using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchEnergyDrop : DeathmatchDrop
{
    public float amount;

    public override void OnPickUp(Player player)
    {
        if (player.state == DamageableState.Alive && player.energy < player.maxEnergy)
        {
            player.GainEnergy(amount);
            base.OnPickUp(player);
        }
    }

    public override bool PickUpNeeded(Player player)
    {
        return player.energy < player.maxEnergy;
    }
}
