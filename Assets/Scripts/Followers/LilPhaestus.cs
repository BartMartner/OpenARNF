using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilPhaestus : LilShrine
{
    public override IEnumerator Start()
    {
        _holyNumber = 6;
        effects.Add(StatMod());
        effects.Add(TempCelestialCharge());
        yield return base.Start();
    }

    protected IEnumerable StatMod()
    {
        yield return new WaitForSeconds(1f);
        var tempBoostCount = player.GetComponentsInChildren<TemporaryEnergyRegen>().Length;
        var bonus = 1f / (tempBoostCount > 3 ? tempBoostCount - 2 : 1);
        var tempEnergyRegen = player.gameObject.AddComponent<TemporaryEnergyRegen>();
        tempEnergyRegen.Equip(player, 60 * player.blessingTimeMod, bonus);
        FXManager.instance.SpawnFX(FXType.AnimeSplode, player.position);
    }

    public IEnumerable TempCelestialCharge()
    {
        yield return null;
        var tempCelestialCharge = player.gameObject.AddComponent<TemporaryCelestialCharge>();
        tempCelestialCharge.Equip(player, 30 * player.blessingTimeMod);
    }
}
