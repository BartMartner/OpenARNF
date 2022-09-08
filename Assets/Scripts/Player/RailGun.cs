using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RailGun", menuName = "Player Energy Weapons/Rail Gun", order = 4)]
public class RailGun : PlayerEnergyWeapon, IHasOnDestroy
{
    public AudioClip beamSound;
    public bool emitting;
    public float minDamage;
    public float maxDamage;
    public float maxEnergy = 5f;

    private LaserStats _laserStats;
    private ILaser _laser;

    private IEnumerator _stopAfterTime;

    public Action onDestroy { get; set; }
    public Transform transform { get { return _player.transform; } }

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _allowTurning = true;
        _laserStats = new LaserStats();
        _laserStats.laserType = LaserType.RailGun;
        _laserStats.damageType = DamageType.Generic;
        _laserStats.ignoreDoors = false;
        _laserStats.layerMask = LayerMask.GetMask();
        _laserStats.range = 36;
        _laserStats.sortingLayerName = "Foreground1";
        _laserStats.sortingOrder = 20;
        _laserStats.stopTime = 0.33f;
        _laserStats.team = _player.team;
        _laserStats.stopType = LaserStopType.Fade;
    }

    public override void OnAttackDown()
    {
        if (!emitting)
        {
            emitting = true;
            _player.attacking = true;

            var energyUse = Mathf.Clamp(_player.energy, minEnergy, maxEnergy);
            var power = energyUse / maxEnergy;

            _player.PlayOneShot(beamSound, power * 1.5f); //so that it won't get cut off

            _player.energy -= energyUse;

            var aimingInfo = _player.GetAimingInfo();
            var origin = aimingInfo.origin + aimingInfo.direction * 0.5f;

            _laserStats.width = power * 1.5f;
            _laserStats.damage = Mathf.Lerp(minDamage, maxDamage, power) * _player.damageMultiplier;
            var rotation = Quaternion.FromToRotation(Vector3.right, aimingInfo.direction);
            _laser = LaserManager.instance.AttachAndFireLaser(_laserStats, origin - _player.transform.position, rotation, 0, this);                       
            _laser.gameObject.SetActive(true);

            _player.DamageLatchers(_laserStats.damage, DamageType.Generic);

            _stopAfterTime = StopAfterTime();
            _player.StartCoroutine(_stopAfterTime);
        }
    }

    private IEnumerator StopAfterTime()
    {
        yield return new WaitForSeconds(0.1f);
        if (_laser != null) { _laser.Stop(); }
        yield return new WaitForSeconds(0.43f);
        emitting = false;
        _player.attacking = false;
        _stopAfterTime = null;
    }

    public override void OnDeselect()
    {
        if(_stopAfterTime != null)
        {
            _player.StopCoroutine(_stopAfterTime);
        }
        Stop();
    }

    public override void Stop()
    {
        if (_player.loopingAudio)
        {
            _player.loopingAudio.Stop();
        }

        if (_laser != null)
        {
            _laser.Stop();
            _laser = null;
        }

        emitting = false;
        _player.attacking = false;
    }

    public void OnDestroy()
    {
        if (onDestroy != null) onDestroy();
        Stop();
    }
}
