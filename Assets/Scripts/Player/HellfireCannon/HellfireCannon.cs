using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HellfireCannon", menuName = "Player Energy Weapons/Hellfire Cannon", order = 3)]
public class HellfireCannon : PlayerEnergyWeapon
{
    public AudioClip shootSound;
    public float baseDamage;
    public float minSpeed;
    public float maxSpeed;
    public float energyCost;
    public GameObject spawnOnDeath;
    private float _fireArc = 25f;
    private bool _firing;
    private float _shotTimer = 0;
    private float _shotDelay;
    private float _minShotDelay = 0.125f;
    private float _maxShotDelay = 0.375f;
    private float _rofAccel = 0.0375f;
    private float _energyTimer;
    private float _energyTime;
    private float _energyPerSecond = 1;
    private float _minGravity = 12;
    private float _maxGravity = 14;

    public override void OnAttackDown()
    {
        if (_player.state != DamageableState.Alive) return;

        if (_player.energy > minEnergy)
        {
            _firing = true;
            _player.attacking = true;
            _shotDelay = _maxShotDelay;
        }
    }

    public override void OnAttackUp()
    {
        Stop();
    }

    public override void OnDeselect()
    {
        Stop();
    }

    public override void Update()
    {
        if (_firing)
        {
            if (_player.energy <= 0 || (!_player.controller.GetButton(_player.attackString)))
            {
                Stop();
            }

            _energyTime = 1 / _energyPerSecond;
            _energyTimer += Time.deltaTime;
            if (_energyTimer > _energyTime)
            {
                _energyTimer = 0;
                _player.energy -= 1;
            }

            if (_shotTimer > 0)
            {
                _shotTimer -= Time.deltaTime;
            }
            else
            {
                if (_shotDelay > _minShotDelay)
                {
                    _shotDelay -= _rofAccel;
                }
                else
                {
                    _shotDelay = _minShotDelay;
                }

                _shotTimer = _shotDelay;
                var max = _maxShotDelay - _minShotDelay;
                var t = _maxShotDelay - _shotDelay;
                var charge = t / max;
                _energyPerSecond = charge > 0.33f ? 1 : 2;
                Fire(charge);
            }
        }
        else
        {
            _shotTimer = 0;
            _shotDelay = _maxShotDelay;
            _energyPerSecond = 1;
        }
    }

    public void Fire(float charge)
    {
        if (shootSound) { _player.PlayOneShot(shootSound); }
        var aimingInfo = _player.GetAimingInfo();
        var minShots = Mathf.RoundToInt(Mathf.Lerp(1, 3, charge));
        var shots = _player.arcShots > 0 ? _player.arcShots + (minShots-1) : minShots;

        for (int i = 0; i < shots; i++)
        {
            var stats = new ProjectileStats();
            stats.projectileMotion = true;
            stats.team = _player.team;
            stats.type = ProjectileType.FireBolt;
            stats.speed = Mathf.Lerp(minSpeed, maxSpeed, charge) + _player.shotSpeedModifier + Random.Range(-2, 2);
            stats.damage = baseDamage * _player.damageMultiplier;
            stats.damageType = DamageType.Fire;
            stats.gravity = Random.Range(_minGravity, _maxGravity);
            stats.lockRotation = false;
            stats.ignoreAegis = true;
            stats.canOpenDoors = true;
            stats.homing = _player.projectileStats.homing;
            stats.homingRadius = _player.projectileStats.homingRadius;
            stats.size = _player.projectileStats.size > 1 ? 3 : 2;
            stats.spawnOnHit = spawnOnDeath;
            stats.spawnAtContact = true;

            float angleMod = Random.Range(-0.75f, 1f);
            Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * _fireArc / 2, Vector3.forward) * aimingInfo.direction).normalized;
            ProjectileManager.instance.Shoot(stats, aimingInfo.origin, shotDirection);
        }
    }

    public override void Stop()
    {
        _firing = false;
        _player.attacking = false;
    }

    public void OnDestroy()
    {
        Stop();
    }
}
