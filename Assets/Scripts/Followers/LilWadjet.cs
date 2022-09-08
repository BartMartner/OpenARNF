using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilWadjet : LilShrine
{
    public override IEnumerator Start()
    {
        _holyNumber = 7;
        effects.Add(TempSpeed());
        effects.Add(TempShield());
        yield return base.Start();
    }

    protected IEnumerable TempSpeed()
    {
        yield return null;
        player.AddTempStatMod(PlayerStatType.Speed, Constants.defaultTempBuffRank);
        FXManager.instance.SpawnFX(FXType.AnimeSplode, player.position);
    }

    protected IEnumerable TempShield()
    {
        yield return null;
        player.AddTempShield(30 * player.blessingTimeMod);
    }
}
