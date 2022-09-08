using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapDrop : Drop
{
    public float amount;

    public override IEnumerator Start()
    {
        PickUpManager.instance.scrapDrops.Add(this);
        return base.Start();
    }

    public override void OnDestroy()
    {
        if (PickUpManager.instance)
        {
            PickUpManager.instance.scrapDrops.Remove(this);
        }
        base.OnDestroy();
    }

    public override void OnPickUp(Player player)
    {
        player.GainScrap(CurrencyType.Gray, amount);
        base.OnPickUp(player);
    }
}
