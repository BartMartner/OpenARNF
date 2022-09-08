using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewProjectileEnergyWeapon", menuName = "Player Energy Weapons/Projectile Energy Weapon", order = 0)]
public class ProjectileEnergyWeapon : PlayerEnergyWeapon
{
    public ProjectileStats projectileStats;
    public AudioClip shootSound;
    public float baseDamage;
    public bool useDamageMultiplier = true;
    public bool useShotSpeedMultiplier = true;
    public bool useRateOfFire = true;
    public bool useHoming = true;
    public bool useSize = true;
    public bool useScaling = true;
    public float energyCost;
    public float attackDelay = 0.3f;

    public override void OnAttackDown()
    {
        if (_player.state != DamageableState.Alive) return;

        var realEnergyCost = energyCost;
        if(_player.rotaryShot) { realEnergyCost /= 2; }

        if (_player.energy >= realEnergyCost)
        {
            if (_player.healthBooster) { _player.CalculateAttackDelay(false); }
            _player.CalculateShotSpeed(false);
            _player.CalculateShotSize(false);
            _player.CalculateDamage(false);

            _player.energy -= realEnergyCost;
            if (shootSound) { _player.PlayOneShot(shootSound); }
            
            var stats = new ProjectileStats(projectileStats);

            stats.team = _player.projectileStats.team;
            stats.damage = baseDamage;
            
            if(useShotSpeedMultiplier)
            {
                stats.speed += _player.shotSpeedModifier;
            }

            if (useDamageMultiplier)
            {
                stats.damage *= _player.damageMultiplier;   
            }            

            if(useSize)
            {
                stats.size = Mathf.Clamp(_player.projectileStats.size, 0, 2);
            }

            if(useScaling)
            {
                stats.sizePerSecond += _player.projectileStats.sizePerSecond;
            }

            if(useHoming)
            {
                stats.homing = _player.projectileStats.homing;
                stats.homingRadius = _player.projectileStats.homingRadius;
            }

            if (useRateOfFire)
            {
                var modifiedAttackDelay = attackDelay * (_player.attackDelay / Constants.startingAttackDelay);
                _player.StartCoroutine(_player.Attack(stats, modifiedAttackDelay));
            }
            else
            {
                _player.StartCoroutine(_player.Attack(stats, attackDelay));
            }
        }
    }
}
