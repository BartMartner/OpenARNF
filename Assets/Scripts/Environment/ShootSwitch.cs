using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShootSwitch : MonoBehaviour, IDamageable, ISetByDamageType
{
    public bool targetable { get { return true; } }
    public Vector3 position { get { return transform.position; } }

    public UnityEvent onShoot;
    public SpriteRenderer switchRenderer;

    [EnumFlags]
    public DamageType immunities;
    public Sprite usedSprite;
    private Sprite _originalSprite;
    private AudioSource _audioSource;
    public AudioClip switchSound;
    public bool singleUse;
    public ShootSwitch linkedSwitch;
    public float aegisTime = 0.5f;

    private bool _aegis;
    private Flasher _flasher;
    public void Start()
    {
        _originalSprite = switchRenderer.sprite;
        _audioSource = GetComponent<AudioSource>();
        _flasher = switchRenderer.GetComponent<Flasher>();
    }

    private DamageableState _state = DamageableState.Alive;
    public DamageableState state
    {
        get
        {
            return _state;
        }

        set
        {
            _state = value;
        }
    }

    public bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        if (_aegis || _state != DamageableState.Alive || immunities != 0 && immunities.HasFlag(damageType))
        {
            return false;
        }

        if (onShoot != null)
        {
            onShoot.Invoke();
        }

        if(_audioSource)
        {
            _audioSource.PlayOneShot(switchSound);
        }

        if(singleUse)
        {
            _state = DamageableState.Dead;
            switchRenderer.sprite = usedSprite;

            if (linkedSwitch)
            {
                linkedSwitch.state = _state;
                linkedSwitch.switchRenderer.sprite = switchRenderer.sprite;
            }
        }
        else
        {
            StartCoroutine(UseFlash());
        }

        return true;
    }

    public void Update()
    {
        if(state == DamageableState.Alive)
        {
            if(!_flasher.flashing) _flasher.StartFlash(1, 0.5f, switchRenderer.color, 0.33f, true);
        }
        else if(_flasher.flashing)
        {
            _flasher.StopFlash();
        }
    }

    public IEnumerator UseFlash()
    {
        _aegis = true;
        switchRenderer.sprite = usedSprite;

        if (linkedSwitch)
        {
            linkedSwitch.state = _state;
            linkedSwitch.switchRenderer.sprite = switchRenderer.sprite;
        }

        yield return new WaitForSeconds(aegisTime);

        switchRenderer.sprite = _originalSprite;

        if (linkedSwitch)
        {
            linkedSwitch.state = _state;
            linkedSwitch.switchRenderer.sprite = switchRenderer.sprite;
        }

        _aegis = false;
    }

    public void SetByDamageType(DamageType damageType)
    {
        if (damageType == DamageType.Generic)
        {
            immunities = 0;
        }
        else
        {
            immunities = ~damageType;
        }

        Color32 color;
        if (Constants.damageTypeColors.TryGetValue(damageType, out color))
        {
            switchRenderer.color = color;
        }
    }

    public void ApplyStatusEffect(StatusEffect statusEffect, Team team) { }
    public void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team) { }
}
