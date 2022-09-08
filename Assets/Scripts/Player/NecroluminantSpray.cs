using UnityEngine;

[CreateAssetMenu(fileName = "NecroluminantSpray", menuName = "Player Energy Weapons/NecroluminantSpray", order = 0)]
public class NecroluminantSpray : PlayerEnergyWeapon
{
    public ProjectileStats projectileStats;
    public float baseDamage;
    public float energyCost;
    public float attackDelay = 0.1f;
    public float scatterArc = 0;

    public override void OnSelect()
    {
        if(_player.light.sprite != _player.light400)
        {
            _player.light.sprite = _player.light200;
        }
        base.OnSelect();
    }

    public override void OnDeselect()
    {
        base.OnDeselect();
        _player.ResetLight();
    }

    public override void OnAttackDown()
    {
        if (_player.state != DamageableState.Alive) return;

        if (_player.energy >= energyCost)
        {
            if (_player.healthBooster) { _player.CalculateAttackDelay(false); }
            _player.CalculateShotSpeed(false);
            _player.CalculateShotSize(false);
            _player.CalculateDamage(false);

            _player.energy -= energyCost;

            var stats = new ProjectileStats(projectileStats);

            stats.team = _player.projectileStats.team;
            stats.damage = baseDamage;
            stats.speed += _player.shotSpeedModifier;
            stats.damage *= _player.damageMultiplier;
            stats.size = Mathf.Clamp(_player.projectileStats.size, 0, 2);
            stats.homing = _player.projectileStats.homing;
            stats.homingRadius = _player.projectileStats.homingRadius;
            stats.creepStats = new CreepStats(projectileStats.creepStats);
            stats.creepStats.damage = stats.creepStats.damage * _player.damageMultiplier;

            var modifiedAttackDelay = attackDelay * (_player.attackDelay / Constants.startingAttackDelay);
            _player.StartCoroutine(_player.DummyAttack(modifiedAttackDelay));

            var aimingInfo = _player.GetAimingInfo();
            var directionMod = (Quaternion.AngleAxis(Random.Range(scatterArc * -0.5f, scatterArc * 0.5f), Vector3.forward) * aimingInfo.direction).normalized;

            //if (_player.onAttack != null) { _player.onAttack(stats, aimingInfo, _player.arcShots, _player.fireArc); }

            if (_player.arcShots > 0)
            {
                for (int i = 0; i < _player.arcShots; i++)
                {
                    float angleMod = (((float)i / (_player.arcShots - 1f)) * 2f) - 1f;
                    Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * _player.fireArc / 2, Vector3.forward) * directionMod).normalized;
                    ProjectileManager.instance.Shoot(stats, aimingInfo.origin, shotDirection);
                }
            }
            else
            {
                ProjectileManager.instance.Shoot(stats, aimingInfo.origin, directionMod);
            }
        }
    }
}

