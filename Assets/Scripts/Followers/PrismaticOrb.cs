using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrismaticOrb : OrbitalFollower
{
    public AudioClip laserStartClip;
    public AudioClip laserClip;
    public LaserStats laserStats;

    private bool _lasering;

    public override IEnumerator Start()
    {
        return base.Start();
    }

    public override bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        if(!_lasering)
        {
            var projectile = source.GetComponent<Projectile>();
            if (projectile)
            {
                StartCoroutine(FireLaser(projectile));
            }
        }

        return false;
    }

    private IEnumerator FireLaser(Projectile projectile)
    {
        _lasering = true;

        var rotation = Quaternion.FromToRotation(Vector3.right, projectile.direction);
        var duration = 0.5f;
        var bigLaser = projectile.stats.size > 2;
        LaserType type;
        DamageType damageType;
        List<StatusEffect> statusEffects;
        
        if (projectile.stats.damageType.HasFlag(DamageType.Fire))
        {
            type = bigLaser ? LaserType.BigHeat : LaserType.Heat;
            damageType = DamageType.Fire;
            statusEffects = new List<StatusEffect>() { ResourcePrefabManager.instance.LoadStatusEffect("StatusEffects/NormalBurn") };
        }
        else
        {
            type = bigLaser ? LaserType.BigEnergy : LaserType.Energy;
            damageType = DamageType.Generic;
            statusEffects = null;
        }

        laserStats.ignoreDoors = true;
        laserStats.laserType = type;
        laserStats.damage = projectile.stats.damage * 2f;
        laserStats.damageType = damageType;
        laserStats.statusEffects = statusEffects;

        LaserManager.instance.AttachAndFireLaser(laserStats, Vector3.zero, rotation, duration, this);        
     
        _audioSource.PlayOneShot(laserStartClip);
        _audioSource.loop = true;
        _audioSource.clip = laserClip;
        _audioSource.PlayDelayed(laserStartClip.length * 0.5f);
        yield return new WaitForSeconds(duration);
        _audioSource.Stop();
        _audioSource.loop = false;
        
        yield return new WaitForSeconds(laserStats.stopTime);
        _lasering = false;
    }
}
