using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilBuluc : LilShrine
{
    public override IEnumerator Start()
    {
        _holyNumber = 8;
        effects.Add(StatMod());
        effects.Add(RadialBolts());
        yield return base.Start();
    }

    protected IEnumerable StatMod()
    {
        yield return null;        
        player.AddTempStatMod(PlayerStatType.Attack, Constants.defaultTempBuffRank);
        FXManager.instance.SpawnFX(FXType.AnimeSplode, player.position);
    }

    public IEnumerable RadialBolts()
    {
        var stats = new ProjectileStats(player.projectileStats);
        stats.canOpenDoors = false;

        var shotsFired = 0;
        var burstCount = 24;
        var burstArc = 360f;
        var burstTime = 2;

        while (shotsFired < burstCount)
        {
            shotsFired++;

            float angleMod = (((float)shotsFired / (burstCount - 1f)) * 2f) - 1f;
            Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * burstArc / 2, Vector3.forward) * Vector2.up).normalized;
            player.PlayOneShot(player.attackSound, 0.75f);
            ProjectileManager.instance.Shoot(stats, transform.position, shotDirection);

            yield return new WaitForSeconds(burstTime / burstCount);
        }
    }
}
