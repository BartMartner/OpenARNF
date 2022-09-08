using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilOrphy : LilShrine
{
    public override IEnumerator Start()
    {
        _holyNumber = 9;
        effects.Add(TempShieldOrbs());
        effects.Add(SpawnBots());
        yield return base.Start();
    }

    protected IEnumerable SpawnBots()
    {         
        for (int i = 0; i < 9; i++)
        {
            yield return new WaitForSeconds(1f / 12f);
            player.SpawnNanobot(transform.position);
        }
        yield return new WaitForSeconds(1f / 12f);
    }

    protected IEnumerable TempShieldOrbs()
    {
        yield return null;
        var tempOrbs = player.gameObject.AddComponent<TemporaryShieldOrbs>();
        tempOrbs.Equip(player, 30 * player.blessingTimeMod);
    }
}
