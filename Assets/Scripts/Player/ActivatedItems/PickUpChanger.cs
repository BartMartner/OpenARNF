using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickUpChanger", menuName = "Player Activated Items/PickUpChanger", order = 1)]
public class PickUpChanger : PlayerActivatedItem
{
    public DropType forceType;

    public override void ButtonDown()
    {
        if (Usable())
        {
            _player.energy -= energyCost;
            PickUpManager.instance.ChangeAllPickUp(forceType);
        }
    }

    public override bool Usable()
    {
        var result = base.Usable();
        return result && PickUpManager.instance && PickUpManager.instance.pickUpsPresent;
    }
}
