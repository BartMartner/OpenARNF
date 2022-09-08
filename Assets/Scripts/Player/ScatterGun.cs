using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScatterGun", menuName = "Player Energy Weapons/ScatterGun", order = 0)]
public class ScatterGun : PlayerEnergyWeapon
{
    public AudioClip shootSound;
    public float baseDamage;
    public float minSpeed;
    public float maxSpeed;
    public float minGravity;
    public float maxGravity;
    public float energyCost;
    public float attackDelay;
    public float fireArc;
    public float arcShots;

    public override void OnAttackDown()
    {
        if (_player.energy >= energyCost)
        {
            _player.energy -= energyCost;
            if (shootSound)
            {
                _player.PlayOneShot(shootSound);
            }

            var modifiedAttackDelay = attackDelay * (_player.attackDelay / Constants.startingAttackDelay);
            _player.StartCoroutine(_player.DummyAttack(modifiedAttackDelay));

            var aimingInfo = _player.GetAimingInfo();
            var actualArcShots = _player.arcShots > 0 ? arcShots * _player.arcShots : arcShots;
            var actualFireArc = _player.arcShots > 0 ? fireArc * _player.arcShots * 0.75f : fireArc;

            for (int i = 0; i < actualArcShots; i++)
            {
                var stats = new ProjectileStats();
                stats.team = _player.team;
                stats.type = ProjectileType.Bullet;
                stats.speed = Random.Range(minSpeed, maxSpeed) + _player.shotSpeedModifier;
                stats.damage = baseDamage * _player.damageMultiplier;
                stats.gravity = Random.Range(minGravity, maxGravity);
                stats.lockRotation = true;
                stats.ignoreAegis = true;
                stats.canOpenDoors = true;
                stats.homing = _player.projectileStats.homing;
                stats.homingRadius = _player.projectileStats.homingRadius;
                stats.size = _player.projectileStats.size > 1 ? 2 : 1;

                float angleMod = (((float)i / (actualArcShots - 1f)) * 2f) - 1f;
                Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * actualFireArc / 2, Vector3.forward) * aimingInfo.direction).normalized;
                ProjectileManager.instance.Shoot(stats, aimingInfo.origin, shotDirection);                
            }
        }
    }
}
