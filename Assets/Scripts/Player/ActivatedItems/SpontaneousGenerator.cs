using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpontaneousGenerator", menuName = "Player Activated Items/SpontaneousGenerator", order = 1)]
public class SpontaneousGenerator : PlayerActivatedItem
{
    public override void ButtonDown()
    {
        if (Usable())
        {
            _player.energy -= energyCost;
            PickUpManager.instance.PickupToNanobots(_player);
        }
    }

    public override bool Usable()
    {
        var result = base.Usable();
        return result && PickUpManager.instance && PickUpManager.instance.pickUpsPresent;
    }
}
