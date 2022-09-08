using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinOrb : TrailFollower
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public override IEnumerator Start()
    {
        yield return base.Start();
        player.onBolt += Shoot;
    }

    public override void Update()
    {
        base.Update();
        if (player.spiderForm)
        {
            _animator.SetFloat("Aiming", 0);
        }
        else
        {
            _animator.SetFloat("Aiming", player.aiming);
        }
    }

    public void Shoot(ProjectileStats stats, AimingInfo aimingInfo, int arcShots, float fireArc)
    {
        aimingInfo.origin = transform.position;
        var statsCopy = new ProjectileStats(stats);
        statsCopy.canOpenDoors = false;

        _animator.SetTrigger("Shoot");

        if (arcShots > 0)
        {
            ProjectileManager.instance.ArcShoot(statsCopy, aimingInfo.origin, aimingInfo.direction, arcShots, fireArc);
        }
        else
        {
            ProjectileManager.instance.Shoot(statsCopy, aimingInfo);
        }
    }

    public override void OnDestroy()
    {
        player.onBolt -= Shoot;
        base.OnDestroy();
    }
}
