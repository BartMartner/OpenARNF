using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "Flamethower", menuName = "Player Energy Weapons/Flamethrower", order = 1)]
public class Flamethrower : PlayerEnergyWeapon, IFlameOrigin
{
    public FlamethrowerFlame flamePrefab;
    public AudioClip flameStart;
    public AudioClip flameLoop;
    public bool emitting;
    public bool hitWalls;
    public float flameRange = 6f;
    public float energyPerSecond = 2;
    public float baseDamage = 15f;

    [NonSerialized]
    private Transform _origin;
    public Transform origin { get { return _origin; } }

    [NonSerialized]
    private Vector3 _target;
    public Vector3 target { get { return _target; } }

    [NonSerialized]
    private List<FlamethrowerFlame> _flames;

    private float _flameTimer;
    private float _energyTimer;
    private float _energyTime;
    private float _flameInterval;
    private float _flameSpeed = 2f;
    private const float _flameMovingLifespan = 11 / 24f;
    private const float _flameTotalLifespan = 19 / 24f;
    private int _lastSortingOrder;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _flames = new List<FlamethrowerFlame>();
        _allowTurning = true;

        var flames = new GameObject();
        _origin = flames.transform;
        _origin.parent = _player.transform;
        _origin.localPosition = Vector3.zero;
        _origin.name = "Flames";
    }

    public override void Update()
    {
        if (emitting)
        {
            if(_player.energy <= 0 || (!_player.controller.GetButton(_player.attackString)))
            {
                Stop();
            }

            var mod = Mathf.Clamp((_player.baseProjectileSpeed - Constants.startingProjectileSpeed), 0, 10);
            var range = Mathf.Clamp(flameRange + _player.shotSpeedUps * 0.75f + mod, flameRange-2,20);

            _flameSpeed = range / _flameMovingLifespan;
            _flameInterval = _flameMovingLifespan / range;
            _energyTime = 1 / energyPerSecond;

            var aimingInfo = _player.GetAimingInfo();
            _origin.position = (aimingInfo.nearWall && hitWalls) ? aimingInfo.origin : aimingInfo.origin + aimingInfo.direction * 0.5f;
            _target = _origin.position + aimingInfo.direction * range;
            _flameTimer -= Time.deltaTime;
            if (_flameTimer <= 0)
            {
                SpawnFlame(aimingInfo.direction);
                _flameTimer = _flameInterval;
            }

            _energyTimer += Time.deltaTime;
            if(_energyTimer > _energyTime)
            {
                _energyTimer = 0;
                _player.energy -= 1;
            }
        }
        else
        {
            _flameTimer = 0f;
            _lastSortingOrder = 0;
        }
    }

    public void SpawnFlame(Vector3 direction)
    {
        _lastSortingOrder++;
        
        var damage = baseDamage * _player.damageMultiplier;
        var flame = GetFlame();

        _player.DamageLatchers(damage, DamageType.Fire);
        flame.Spawn(_lastSortingOrder, _flameSpeed, _flameMovingLifespan, _flameTotalLifespan, _player.team, true);
    }

    public FlamethrowerFlame GetFlame()
    {
        var flame = _flames.FirstOrDefault(f => f.gameObject.activeInHierarchy == false);

        if (!flame)
        {
            flame = Instantiate(flamePrefab);
            flame.owner = this;
            flame.player = _player;
            flame.team = _player.team;
            flame.transform.parent = _origin;
            flame.hitWalls = hitWalls;
            flame.GetComponent<DamageCreatureTrigger>().damage = baseDamage * _player.damageMultiplier;
            _flames.Add(flame);
        }

        return flame;
    }

    public override void OnAttackDown()
    {
        if (_player.state != DamageableState.Alive) return;

        if (_player.energy > 0)
        {
            emitting = true;
            _player.PlayOneShot(flameStart); //so that it won't get cut off
            _player.loopingAudio.clip = flameLoop;
            _player.loopingAudio.PlayScheduled(flameStart.length - 0.5f);
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
        if (_player && _player.loopingAudio)
        {
            _player.loopingAudio.Stop();
        }
        emitting = false;
        _player.attacking = false;
    }

    public void OnDestroy()
    {
        Stop();
        foreach (var flame in _flames)
        {
            if (flame) Destroy(flame.gameObject, _flameTotalLifespan);
        }
    }
}
