using UnityEngine;
using System.Collections;

public class HealthDrop : Drop
{
    public float amount;

    public override void OnPickUp(Player player)
    {
        if (player.state == DamageableState.Alive)
        {
            player.GainHealth(amount);
            base.OnPickUp(player);
        }
    }
}