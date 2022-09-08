using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningGun", menuName = "Player Energy Weapons/Lightning Gun", order = 3)]
public class LightningGun : PlayerEnergyWeapon
{
    public LightningGunBolt lightningPrefab;
    public AudioClip lightningStart;
    public AudioClip lightningLoop;
    public bool emitting;
    public float baseDamagePerSecond = 1.5f;
    public float energyPerSecond = 2;
    public float range = 8f;
    public float fireArc = 90f;

    private LightningGunBolt _lightning;
    private float _energyTimer;
    private float _energyTime;
    private Vector3 _lastTarget;
    private Quaternion _randomAngle;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _lightning = Instantiate(lightningPrefab);
        _lightning.gameObject.SetActive(false);
        _lightning.transform.parent = _player.transform;
        _lightning.transform.localPosition = Vector3.zero;
        _lightning.team = Team.Player;

        _allowTurning = true;

        _randomAngle = Quaternion.AngleAxis(Random.Range(-30F, 30F), Vector3.forward);
    }

    public override void Update()
    {
        if (emitting)
        {
            if (!_player.controller.GetButton(_player.attackString))
            {
                Stop();
                return;
            }

            if (!_lightning.gameObject.activeInHierarchy)
            {
                _lightning.gameObject.SetActive(true);
            }

            if (_player.energy <= 0)
            {
                Stop();
            }

            _energyTime = 1 / energyPerSecond;

            var aimingInfo = _player.GetAimingInfo();
            var origin = aimingInfo.origin + aimingInfo.direction * 0.5f;

            var mod = Mathf.Clamp((_player.baseProjectileSpeed - Constants.startingProjectileSpeed), 0, 10);
            var modRange = Mathf.Clamp(range + _player.shotSpeedUps * 0.75f + mod, range-2, 20);

            var closestEnemy = EnemyManager.instance.GetClosestInArc(aimingInfo.origin, aimingInfo.direction, modRange, fireArc);

            _randomAngle = Quaternion.AngleAxis(Random.Range(-30F, 30F), Vector3.forward);
            Vector3 randomDirection = ( _randomAngle * aimingInfo.direction).normalized;

            var target = closestEnemy ? closestEnemy.position : origin + randomDirection * modRange;
            var lerpValue = closestEnemy ? 0.5f : 0.15f;

            var trueTarget = Vector3.Lerp(_lastTarget, target, lerpValue);
            _lightning.damagePerSecond = baseDamagePerSecond * _player.damageMultiplier;
            _lightning.DrawLightning(origin, trueTarget, closestEnemy);
            _player.DamageLatchers(_lightning.damagePerSecond * Time.deltaTime, DamageType.Generic);

            _energyTimer += Time.deltaTime;
            if (_energyTimer > _energyTime)
            {
                _energyTimer = 0;
                _player.energy -= 1;
            }

            _lastTarget = trueTarget;
        }
        else
        {
            _lastTarget = _player.transform.position;
        }
    }

    public override void OnAttackDown()
    {
        if (_player.state != DamageableState.Alive) return;

        if (_player.energy > 0)
        {
            emitting = true;
            _player.PlayOneShot(lightningStart); //so that it won't get cut off
            _player.loopingAudio.clip = lightningLoop;
            _player.loopingAudio.PlayScheduled(lightningStart.length - 0.5f);
            _player.attacking = true;
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

    public override void Stop()
    {
        _player.loopingAudio.Stop();
        _lightning.gameObject.SetActive(false);
        emitting = false;
        _player.attacking = false;
    }
}
