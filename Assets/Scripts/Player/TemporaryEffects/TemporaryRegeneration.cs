using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryRegeneration : TemporaryPlayerEffect
{
    private float _rate;
    public override void Equip(Player player, float duration)
    {
        throw new System.Exception("TemporaryRegeneration should be equipped using Equip(Player player , float duration, float rate)");
    }

    public void Equip(Player player, float duration, float rate)
    {
        base.Equip(player, duration);
        _rate = rate;
        _player.bonusRegenerationRate += _rate;
    }

    public override void Unequip()
    {
        _player.bonusRegenerationRate -= _rate;
        base.Unequip();
    }
}
