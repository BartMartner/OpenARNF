using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchHealthDrop : DeathmatchDrop
{
    public float amount;

    public override void OnPickUp(Player player)
    {
        if (player.state == DamageableState.Alive && player.health < player.maxHealth)
        {
            player.GainHealth(amount);
            base.OnPickUp(player);
        }
    }

    public override bool PickUpNeeded(Player player)
    {
        return player.health < player.maxHealth;
    }
}
