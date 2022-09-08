using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LilTyr : LilShrine
{
    public override IEnumerator Start()
    {
        _holyNumber = 3;
        effects.Add(TempDamage());
        effects.Add(TempShotSize());
        yield return base.Start();
    }

    protected IEnumerable TempDamage()
    {
        yield return null;
        player.AddTempStatMod(PlayerStatType.Damage, Constants.defaultTempBuffRank);        
        FXManager.instance.SpawnFX(FXType.AnimeSplode, player.position);
    }

    protected IEnumerable TempShotSize()
    {
        yield return null;
        player.AddTempStatMod(PlayerStatType.ShotSize, Constants.defaultTempBuffRank);
        FXManager.instance.SpawnFX(FXType.AnimeSplode, player.position);
    }
}
