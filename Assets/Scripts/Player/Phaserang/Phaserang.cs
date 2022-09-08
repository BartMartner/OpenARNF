using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Phaserang", menuName = "Player Energy Weapons/Phaserang", order = 4)]
public class Phaserang : PlayerEnergyWeapon
{
    public PhaserangBoomerang boomerangPrefab;    
    public AudioClip shootSound;
    public AudioClip returnSound;
    public float baseDamagePerSecond = 30;
    private List<PhaserangBoomerang> _boomerangs = new List<PhaserangBoomerang>();
    private float _velocityFactor = 0;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        NewBoomerang();
    }

    private PhaserangBoomerang NewBoomerang()
    {
        var boomerang = Instantiate(boomerangPrefab);
        boomerang.transform.parent = _player.transform.parent;
        boomerang.gameObject.SetActive(false);
        boomerang.onReturn += () =>
        {
            if (_player)
            {
                _player.PlayOneShot(returnSound);
                _player.GainEnergy(minEnergy);
            }
        };
        _boomerangs.Add(boomerang);
        return boomerang;
    }

    private IEnumerator Attack()
    {
        _player.attacking = true;
        yield return new WaitForSeconds(0.1f);
        _player.attacking = false;
    }

    public override void OnAttackDown()
    {
        if (_player.energy > minEnergy)
        {
            if (_player.state != DamageableState.Alive) return;
            if (_velocityFactor < 1) { _velocityFactor += Time.deltaTime * 0.75f; }
            if (!_player.flashing)
            {
                _player.StartFlash(1, 0.2f, Constants.blasterGreen, Mathf.Lerp(0, 1, Mathf.Clamp01(_velocityFactor - 0.5f)), false);
            }
        }
        else
        {
            if (!_player.flashing)
            {
                _player.StartFlash(1, 0.2f, Constants.damageFlashColor, 0.25f, false);
            }
        }
    }

    public override void OnAttackUp()
    {
        if (_player.energy > minEnergy)
        {
            var boomerang = GetBoomerang();
            if (_player.spinJumping) _player.spinJumping = false;
            _velocityFactor = Mathf.Clamp(_velocityFactor, 0.75f, 1);
            boomerang.damagePerSecond = baseDamagePerSecond * _player.damageMultiplier;
            boomerang.Shoot(_player.team, _player.transform, _player.GetAimingInfo(), _velocityFactor);
            _player.energy -= minEnergy;
            _player.StartCoroutine(Attack());
            _player.PlayOneShot(shootSound);
            _velocityFactor = 0.55f;
        }
    }

    public PhaserangBoomerang GetBoomerang()
    {
        foreach (var b in _boomerangs)
        {
            if (!b.alive) { return b; }
        }

        return NewBoomerang();
    }

    public override void OnDeselect()
    {
        base.OnDeselect();
        _velocityFactor = 0.55f;
    }

    public override bool Usable()
    {
        return _player && (_player.energy > minEnergy || _boomerangs.Any(b => b.alive));
    }
}