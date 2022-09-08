using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPickUp : Drop
{
    public PlayerStatType statType;    

    public override void OnPickUp(Player player)
    {
        player.AddTempStatMod(statType, Constants.defaultTempBuffRank);
        base.OnPickUp(player);
    }
}
