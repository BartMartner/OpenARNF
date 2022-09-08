using UnityEngine;
using System.Collections;

public class EnergyDrop : Drop
{
    public float amount;

    public override void OnPickUp(Player player)
    {
        player.GainEnergy(amount);
        base.OnPickUp(player);
    }
}