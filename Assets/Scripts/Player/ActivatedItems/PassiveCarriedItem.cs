using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveCarriedItem", menuName = "Player Activated Items/PassiveCarriedItem", order = 1)]
public class PassiveCarriedItem : PlayerActivatedItem
{
    public DropType forceDropType;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _player.forceDropType = forceDropType;
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        _player.forceDropType = DropType.None;
        return base.Unequip(position, parent, setJustSpawned);
    }

    public override bool Usable()
    {
        return false;
    }
}
