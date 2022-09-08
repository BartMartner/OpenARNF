using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtificeBeam", menuName = "Player Energy Weapons/Artifice Beam", order = 4)]
public class ArtificeBeam : PlayerEnergyWeapon
{
    public AudioClip laserStart;
    public AudioClip laserLoop;
    public bool emitting;
    public float scrapPerSecond = 1;
    public Texture2D[] palettes;
    public Color[] lightColors;

    private List<LineLaser> _lasers = new List<LineLaser>();
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
            if (!_player.controller.GetButton(_player.attackString) || _player.grayScrap <= 0)
            {
                Stop();
                return;
            }

            foreach (var laser in _lasers)
            {
                if (!laser.gameObject.activeInHierarchy) { laser.gameObject.SetActive(true); }
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
                _player.DamageLatchers(laser.damagePerSecond * Time.deltaTime, DamageType.Generic);
                i++;
            }
        }

        if (_lasers.Any((l) => l.gameObject.activeInHierarchy))
        {
            _energyTime = 1f / scrapPerSecond;
            _energyTimer += Time.deltaTime;
            if (_energyTimer > _energyTime)
            {
                _energyTimer -= _energyTime;
                if (_player.grayScrap >= 1)
                {
                    _player.grayScrap -= 1;
                }
            }
        }
    }

    public override void OnAttackDown()
    {
        if (_player.state != DamageableState.Alive) return;

        if (_player.grayScrap > 0 && !emitting)
        {
            emitting = true;
            if (laserStart && laserLoop)
            {
                _player.PlayOneShot(laserStart); //so that it won't get cut off
                _player.loopingAudio.clip = laserLoop;
                _player.loopingAudio.PlayScheduled(laserStart.length - 0.5f);
            }
            _player.attacking = true;
            SetUpLasers();
        }
    }

    public void SetUpLasers()
    {
        foreach (var laser in _lasers) { laser.ImmediateStop(); }
        _lasers.Clear();

        var scrapTypes = new CurrencyType[] { CurrencyType.Red, CurrencyType.Green, CurrencyType.Blue };

        for (int i = 0; i < 3; i++)
        {
            var scrap = scrapTypes[i];
            var scrapCount = _player.GetScrap(scrap);

            LaserStats stats = new LaserStats();
            stats.laserType = LaserType.BasicBeam;
            stats.layerMask = LayerMask.GetMask("Default");
            stats.ignoreDoors = false;
            stats.damage = _player.projectileStats.damage * 4.5f + (scrapCount * 2.5f);
            stats.damageType = DamageType.Generic;
            stats.team = _player.team;
            stats.stopTime = 0.15f;
            stats.stopType = LaserStopType.Shrink;
            stats.sortingLayerName = "AboveTiles";
            stats.sortingOrder = i * 2;
            stats.width = Mathf.Lerp(0.25f, 1, scrapCount/8f);

            var ilaser = LaserManager.instance.GetLaser(stats.laserType);
            var laser = ilaser as LineLaser;
            if (!laser) continue;

            laser.gameObject.SetActive(false);
            laser.transform.parent = _player.transform;
            laser.transform.localPosition = Vector3.zero;
            laser.scrollSpeed = 3f;
            laser.sineMag = Mathf.Lerp(0.25f, 1.5f, scrapCount/4f);
            laser.sineOffset = i * 0.33f;
            laser.palette = palettes[i];
            laser.lightColor = lightColors[i];
            laser.AssignStats(stats);
            _lasers.Add(laser);
        }
    }

    public override void OnAttackUp() { Stop(); }

    public override void OnDeselect() { Stop(); }

    public override void Stop()
    {
        if (_player && _player.loopingAudio) { _player.loopingAudio.Stop(); }
        foreach (var laser in _lasers) { laser.Stop(); }
        _lasers.Clear();

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

    private void OnDestroy()
    {
        foreach (var laser in _lasers)
        {
            if (laser) { laser.ImmediateStop(); }
        }
        _lasers.Clear();
    }

    public override bool Usable()
    {
        return _player && _player.grayScrap >= minEnergy;
    }
}
