using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryCelestialCharge : TemporaryPlayerEffect
{
    private AudioClip _activateSound;
    private AudioClip _deactivateSound;
    private PaletteCycling _paletteCyclingInstance;
    private GameObject _damageBounds;
    private DamageCreatureTrigger _damage;
    private BoxCollider2D _trigger;
    private SpriteTrail _playerTrail;

    public override void Equip(Player player, float duration)
    {
        base.Equip(player, duration);

        var existingCharge = player.GetComponent<TemporaryCelestialCharge>();
        if(existingCharge != this)
        {
            Destroy(this);
            return;
        }

        _activateSound = Resources.Load<AudioClip>("Sounds/CelestialChargeActivate");
        _deactivateSound = Resources.Load<AudioClip>("Sounds/CelestialChargeDeactivate");

        _paletteCyclingInstance = _player.gameObject.AddComponent<PaletteCycling>();
        _paletteCyclingInstance.enabled = false;
        _paletteCyclingInstance.paletteCycle = Resources.Load<PaletteCycle>("PaletteCycles/PaletteCycleRainbow");
        _paletteCyclingInstance.defaultPalette = _player.palette;
        _paletteCyclingInstance.cycleFrequency = 0.05f;

        _damageBounds = new GameObject("CelestialChargeDamage");
        _damageBounds.SetActive(false);
        _damageBounds.layer = LayerMask.NameToLayer("EnemyOnly");
        _damage = _damageBounds.AddComponent<DamageCreatureTrigger>();
        _trigger = _damageBounds.AddComponent<BoxCollider2D>();
        _trigger.isTrigger = true;
        _damageBounds.transform.SetParent(_player.transform);
        _damageBounds.transform.localPosition = Vector3.zero;

        _playerTrail = new GameObject().AddComponent<SpriteTrail>();
        _playerTrail.spawnDistance = 0.75f;
        _playerTrail.fadeTime = 0.5f;
        _playerTrail.parentRenderer = _player.GetComponent<SpriteRenderer>();
        _playerTrail.transform.SetParent(_player.transform);
        _playerTrail.transform.localPosition = Vector3.zero;
        _playerTrail.transform.localRotation = Quaternion.identity;
        _playerTrail.gameObject.SetActive(false);

        Activate();
    }

    public override void Unequip()
    {
        Deactivate();
        base.Unequip();
    }

    public void Activate()
    {
        _paletteCyclingInstance.defaultPalette = _player.palette;
        _player.invincible = true;
        _player.light.sprite = _player.light400;
        _paletteCyclingInstance.enabled = true;
        _damageBounds.SetActive(true);
        _damage.damage = _player.projectileStats.damage * 2;
        _playerTrail.Start();

        if (_activateSound)
        {
            _player.PlayOneShot(_activateSound);
        }

        MusicController.instance.SetPitch(1.5f);
    }

    public void Deactivate()
    {
        if (_activateSound)
        {
            _player.PlayOneShot(_deactivateSound);
        }

        _player.invincible = false;

        _damageBounds.SetActive(false);
        Destroy(_damageBounds);

        _playerTrail.Stop();
        _player.ResetLight();
        Destroy(_playerTrail.gameObject);

        _paletteCyclingInstance.defaultPalette = _player.palette;
        _paletteCyclingInstance.enabled = false;
        _paletteCyclingInstance.SetPalettes(_player.palette);
        Destroy(_paletteCyclingInstance);

        if (MusicController.instance)
        {
            MusicController.instance.SetPitch(1);
        }
    }

    protected override void Update()
    {
        _player.invincible = true;
        _trigger.size = _player.boxCollider2D.size * 1.1f;
        _trigger.offset = _player.boxCollider2D.offset;

        if (!_player.confused && MusicController.instance.pitch != 1.5f) { MusicController.instance.SetPitch(1.5f); }

        switch (_paletteCyclingInstance.currentIndex)
        {
            case 0:
                _player.light.color = new Color32(190, 254, 0, 255);
                break;
            case 1:
                _player.light.color = new Color32(107, 205, 106, 255);
                break;
            case 2:
                _player.light.color = new Color32(85, 198, 196, 255);
                break;
            case 3:
                _player.light.color = new Color32(145, 145, 218, 255);
                break;
            case 4:
                _player.light.color = new Color32(148, 94, 201, 255);
                break;
            case 5:
                _player.light.color = new Color32(168, 56, 56, 255);
                break;
            case 6:
                _player.light.color = new Color32(254, 127, 0, 255);
                break;
        }

        base.Update();
    }
}
