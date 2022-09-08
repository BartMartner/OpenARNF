using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxinOrb : TrailFollower
{
    public ProjectileStats projectileStats;

    private float _dripTimer;

    public override void Update()
    {
        base.Update();
        if (_dripTimer > 0)
        {
            _dripTimer -= Time.deltaTime;
        }
        else
        {
            projectileStats.team = player.team;
            projectileStats.damage = 1f * player.damageMultiplier;
            projectileStats.creepStats.damage = 1f * player.damageMultiplier;
            ProjectileManager.instance.Shoot(projectileStats, transform.position + Vector3.down * 0.5f, Vector3.down);
            _dripTimer = 0.88f;
        }
    }
}
