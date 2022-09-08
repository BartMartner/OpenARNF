using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LaserBeam", menuName = "Player Energy Weapons/Laser Beam", order = 4)]
public class LaserBeam : PlayerEnergyWeapon
{
    public AudioClip laserStart;
    public bool emitting;
    public float baseDamage = 1.5f;
    public float energyPerSecond = 2;
    
    private List<Laser> _lasers = new List<Laser>();
    private float _energyTimer;
    private float _energyTime;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _allowTurning = true;
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

            foreach (var laser in _lasers)
            {
                if (!laser.gameObject.activeInHierarchy)
                {
                    laser.gameObject.SetActive(true);
                }
            }

            if (_player.energy <= 0)
            {
                Stop();
            }

            var aimingInfo = _player.GetAimingInfo();
            var origin = aimingInfo.origin + aimingInfo.direction * 0.5f;

            var i = 0;
            foreach (var laser in _lasers)
            {
                float angleMod = (((float)i / (_player.arcShots - 1f)) * 2f) - 1f;
                Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * _player.fireArc / 2, Vector3.forward) * aimingInfo.direction).normalized;
                laser.transform.position = origin;
                laser.transform.rotation = Quaternion.FromToRotation(Vector3.right, shotDirection);
                var damage = baseDamage * _player.damageMultiplier;
                laser.damage = damage;
                _player.DamageLatchers(damage, DamageType.Generic);
                i++;
            }
        }

        if (_lasers.Any((l)=> l.gameObject.activeInHierarchy))
        {
            _energyTime = 1f / energyPerSecond;
            _energyTimer += Time.deltaTime;
            if (_energyTimer > _energyTime)
            {
                _energyTimer = 0;
                _player.energy -= 1;
            }
        }
    }

    public override void OnAttackDown()
    {
        if (_player.state != DamageableState.Alive) return;

        if (_player.energy > 0)
        {
            emitting = true;
            _player.PlayOneShot(laserStart); //so that it won't get cut off
            _player.attacking = true;
        }

        SetUpLasers();
    }

    public void SetUpLasers()
    {
        StopAndClearLasers();

        var shots = _player.arcShots > 1 ? _player.arcShots : 1;

        var bigLaser = _player.projectileStats.size > 2;
        LaserType type;
        DamageType damageType;
        List<StatusEffect> statusEffects;

        if (_player.projectileStats.damageType.HasFlag(DamageType.Fire))
        {
            type = bigLaser ? LaserType.BigHeat : LaserType.Heat;
            damageType = DamageType.Fire;
            statusEffects = new List<StatusEffect>() { ResourcePrefabManager.instance.LoadStatusEffect("StatusEffects/NormalBurn")};
        }
        else
        {
            type = bigLaser ? LaserType.BigEnergy : LaserType.Energy;
            damageType = DamageType.Generic;
            statusEffects = null;
        }

        Laser lastLaser = null;
        for (int i = 0; i < shots; i++)
        {
            var ilaser = LaserManager.instance.GetLaser(type);
            var laser = ilaser as Laser;
            if (!laser) continue;

            laser.gameObject.SetActive(false);
            laser.transform.parent = _player.transform;
            laser.transform.localPosition = Vector3.zero;
            laser.team = _player.team;
            laser.damageType = damageType;
            laser.statusEffects = statusEffects;
            laser.stopTime = 0.25f;
            laser.damageOnStop = false;
            laser.ignoreDoors = false;
            laser.layerMask = LayerMask.GetMask("Default");

            laser.lazerStart.sortingLayerID = SortingLayer.NameToID("AboveTiles");
            if (lastLaser)
            {
                laser.lazerStart.sortingOrder = lastLaser.lazerStart.sortingOrder + 3;
            }
            else
            {
                laser.lazerStart.sortingOrder = 1;
            }
            _lasers.Add(laser);
            lastLaser = laser;
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
        if (_player && _player.loopingAudio) { _player.loopingAudio.Stop(); }
        StopAndClearLasers();

        emitting = false;
        _player.attacking = false;
    }

    public override void ImmediateStop()
    {
        if (_player && _player.loopingAudio) { _player.loopingAudio.Stop(); }
        foreach (var laser in _lasers) { laser.ImmediateStop(); }
        _lasers.Clear();

        emitting = false;
        _player.attacking = false;
    }

    private void StopAndClearLasers()
    {
        foreach (var laser in _lasers) { if (laser) { laser.Stop(); } }
        _lasers.Clear();
    }

    private void OnDestroy()
    {
        foreach (var laser in _lasers) { if (laser) { laser.ImmediateStop(); } }
        _lasers.Clear();
    }
}
