using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CelestialCharge", menuName = "Player Activated Items/Celestial Charge", order = 1)]
public class CelestialCharge : PlayerActivatedItem
{
    public PaletteCycle palletteCycle;
    public AudioClip activateSound;
    public AudioClip deactivateSound;
    private bool _active;
    private float _timer;
    private PaletteCycling _paletteCyclingInstance;
    private float _interval;
    private GameObject _damageBounds;
    private DamageCreatureTrigger _damage;
    private BoxCollider2D _trigger;
    private SpriteTrail _playerTrail;

    public override void Initialize(Player player)
    {
        base.Initialize(player);        
        _paletteCyclingInstance = _player.gameObject.AddComponent<PaletteCycling>();
        _paletteCyclingInstance.enabled = false;
        _paletteCyclingInstance.paletteCycle = palletteCycle;
        _paletteCyclingInstance.defaultPalette = _player.palette;
        _paletteCyclingInstance.cycleFrequency = 0.05f;
        _interval = 1f / energyCost;

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
    }

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (_active)
        {
            Deactivate();
        }
        else if (Usable())
        {
            Activate();
        }
    }

    public override void Update()
    {
        base.Update();

        if (_active)
        {
            _trigger.size = _player.boxCollider2D.size * 1.1f;
            _trigger.offset = _player.boxCollider2D.offset;

            _timer += Time.deltaTime;

            if (_timer > _interval)
            {
                _player.energy -= 1;
                _timer -= _interval;
            }

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

            if (!Usable())
            {
                Deactivate();
            }
        }
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        _active = false;
        _player.invincible = false;
        _player.ResetLight();
        _paletteCyclingInstance.SetPalettes(_player.palette);
        Destroy(_paletteCyclingInstance);
        Destroy(_damageBounds);
        Destroy(_playerTrail.gameObject);
        MusicController.instance.SetPitch(1);
        return base.Unequip(position, parent, setJustSpawned);
    }

    public void Activate()
    {
        _paletteCyclingInstance.defaultPalette = _player.palette;
        _player.invincible = true;
        _player.light.sprite = _player.light400;
        _active = true;
        _paletteCyclingInstance.enabled = true;
        _damageBounds.SetActive(true);
        _damage.damage = _player.projectileStats.damage * 2;
        _playerTrail.Start();
        if(activateSound)
        {
            _player.PlayOneShot(activateSound);
        }
        MusicController.instance.SetPitch(1.5f);
    }

    public void Deactivate()
    {
        _paletteCyclingInstance.defaultPalette = _player.palette;
        _player.invincible = false;
        _active = false;
        _paletteCyclingInstance.enabled = false;
        _damageBounds.SetActive(false);
        _playerTrail.Stop();
        _player.ResetLight();
        if (activateSound)
        {
            _player.PlayOneShot(deactivateSound);
        }
        MusicController.instance.SetPitch(1);
    }

    public override bool Usable()
    {
        return _player.energy >= 1;
    }
}
