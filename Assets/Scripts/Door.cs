using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Door : Damageable
{
    [Header("Door")]
    public int id;
    public SpriteRenderer external;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    public GameObject shield;
    public Damageable fireShield;
    public Damageable explosiveShield;
    public Damageable electricShield;
    public Damageable mechanicalShield;
    public AudioClip open;
    public AudioClip close;
    public AudioClip lockSound;
    public AudioClip unlockSound;
    public bool toConfusionRoom;
    public bool toHeatRoom;
    public bool toWaterRoom;
    public bool toBossRoom;
    public Collider2D collision;
    public bool isClosed { get { return _collider2D.enabled; } }
    public bool allowShieldTint = true;
    private Damageable _activeDamageTypeShield;
    private bool _locked;

    /// <summary>
    /// used for exterminator mode
    /// </summary>
    public bool hardLock;

    private SpriteRenderer _shieldRenderer;
    private ProjectileDeflector _lockedDeflector;
    private Color _originalShieldColor;
    private bool _saveOnOpen;
    private bool _open = false;

    public bool locked
    {
        get
        {
            return _locked;
        }

        set
        {
            if (!value && hardLock)
            {
                Debug.LogWarning("Door not unlocked as hardLock is true");
                return;
            }

            if (_locked != value)
            {    
                _locked = value;

                if (_locked)
                {
                    _collider2D.enabled = true;
                    collision.enabled = true;
                    _lockedDeflector.enabled = true;
                }
                else
                {
                    _lockedDeflector.enabled = false;
                }

                if (_shieldRenderer)
                {
                    StartCoroutine(LockAnimation());
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _lockedDeflector = shield.GetComponent<ProjectileDeflector>();
        _lockedDeflector.enabled = false;
        _shieldRenderer = shield.GetComponent<SpriteRenderer>();
        _originalShieldColor = shield.GetComponent<SpriteRenderer>().color;
    }

    public override bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        if (locked || _activeDamageTypeShield)
        {
            return false;
        }

        if (immunities == 0 || !immunities.HasFlag(damageType))
        {
            Open();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Open(bool immediate = false)
    {
        _collider2D.enabled = false;
        collision.enabled = false;
        animator.SetBool("Closed", false);
        animator.SetTrigger(immediate ? "OpenImmediate" : "Open");
        var activeGame = SaveGameManager.activeGame;
        if (_saveOnOpen && activeGame != null && !activeGame.doorsOpened.Contains(id))
        {
            activeGame.doorsOpened.Add(id);
            if (Automap.instance) { Automap.instance.Refresh(); }
        }

        if (!immediate)
        {
            if (!_open) { PlayOneShot(open); }
        }
        else if(_activeDamageTypeShield)
        {
            Destroy(_activeDamageTypeShield.gameObject);
        }

        _open = true;
    }

    public void Close()
    {
        _collider2D.enabled = true;
        collision.enabled = true;
        animator.SetBool("Closed", true);
        if (_open) { PlayOneShot(close); }
        _open = false;
    }

    public void SetImmunitiesAndColor(DamageType damageType)
    {
        Color32 color;

        immunities = Constants.standardDoorImmunities;

        switch (damageType)
        {
            case DamageType.Fire:
                _saveOnOpen = true;
                _activeDamageTypeShield = fireShield;
                _activeDamageTypeShield.gameObject.SetActive(true);
                break;
            case DamageType.Explosive:
                _saveOnOpen = true;
                _activeDamageTypeShield = explosiveShield;
                _activeDamageTypeShield.gameObject.SetActive(true);
                break;
            case DamageType.Electric:
                _saveOnOpen = true;
                _activeDamageTypeShield = electricShield;
                _activeDamageTypeShield.gameObject.SetActive(true);
                break;
            case DamageType.Mechanical:
                _saveOnOpen = true;
                _activeDamageTypeShield = mechanicalShield;
                _activeDamageTypeShield.gameObject.SetActive(true);
                break;
            default:
                Destroy(fireShield.gameObject);
                Destroy(explosiveShield.gameObject);
                Destroy(electricShield.gameObject);
                if (damageType != 0 && damageType != DamageType.Generic)
                {
                    _saveOnOpen = true;
                    immunities = ~damageType;
                    if (allowShieldTint && Constants.damageTypeColors.TryGetValue(damageType, out color))
                    {
                        _shieldRenderer.color = color;
                    }
                }
                break;
        }

        if (_activeDamageTypeShield != null)
        {
            _activeDamageTypeShield.onStartDeath.AddListener(() => { if (!locked) Open(false); });

            var shieldDeflector = _activeDamageTypeShield.GetComponent<ProjectileDeflector>();
            if (shieldDeflector)
            {
                shieldDeflector.deflectDamage = ~damageType;
            }
        }
    }

    /// <summary>
    /// used for exterminator mode
    /// </summary>
    public void HardLock(int expectedCapabilities)
    {
        hardLock = true;
        LockImmediate();
        if(expectedCapabilities > 0 && expectedCapabilities <= 7)
        {
            _shieldRenderer.material = Resources.Load<Material>("Materials/DoorShield0" + expectedCapabilities);
        }
    }

    public void LockImmediate()
    {
        _locked = true;
        _collider2D.enabled = true;
        collision.enabled = true;
        _lockedDeflector.enabled = true;
        _shieldRenderer.color = Color.white;
        _shieldRenderer.sprite = lockedSprite;
        animator.enabled = false;
        _open = false;
    }

    private IEnumerator LockAnimation()
    {
        var timer = 0f;
        animator.enabled = false;
        PlayOneShot(_locked ? lockSound : unlockSound);

        _shieldRenderer.material.SetColor("_FlashColor", Color.white);
        while (timer < 0.5f)
        {
            _shieldRenderer.material.SetFloat("_FlashAmount", timer/0.5f);
            timer += Time.deltaTime;
            yield return null;
        }

        _shieldRenderer.material.SetFloat("_FlashAmount", 1);
        _shieldRenderer.color = _locked ? Color.white : _originalShieldColor;
        _shieldRenderer.sprite = _locked ? lockedSprite : unlockedSprite;

        timer = 1;
        while (timer > 0)
        {
            _shieldRenderer.material.SetFloat("_FlashAmount", timer);
            timer -= Time.deltaTime;
            yield return null;
        }

        _shieldRenderer.material.SetFloat("_FlashAmount", 0);
        animator.enabled = !_locked;
    }

    public void SetupDoor()
    {
        var room = GetComponentInParent<Room>();
        if(room)
        {
            string spriteName = "Door";

            if (room.roomInfo.environmentType != EnvironmentType.Glitch)
            {
                if (toBossRoom)
                {
                    spriteName = "BossDoor";
                }
                else if (toWaterRoom)
                {
                    spriteName = "WaterDoor";
                }
                else if (toHeatRoom)
                {
                    spriteName = "HotDoor";
                }
                else if (toConfusionRoom)
                {
                    spriteName = "ConfusionDoor";
                }
            }

            switch(room.roomInfo.environmentType)
            {
                case EnvironmentType.Glitch:
                    spriteName = "Glitch" + spriteName;
                    external.material = Resources.Load<Material>("Materials/Glitch");

                    var cycle = external.GetComponent<PaletteCycling>();
                    if (!cycle)
                    {
                        cycle = external.gameObject.AddComponent<PaletteCycling>();
                    }
                    cycle.paletteCycle = Resources.Load<PaletteCycle>("PaletteCycles/GlitchCycleDoor");
                    cycle.cycleFrequency = 0.3846154f;
                    break;
                case EnvironmentType.BuriedCity:
                    spriteName = "BuriedCity" + spriteName;
                    break;
                case EnvironmentType.CoolantSewers:
                    spriteName = "Sewer" + spriteName;
                    break;
                case EnvironmentType.BeastGuts:
                    spriteName = "BeastGuts" + spriteName;
                    animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Doors/BeastGutsDoor");
                    var shieldRenderer = shield.GetComponent<SpriteRenderer>();
                    var gutsShield = Resources.LoadAll<Sprite>("Sprites/Doors/BeastGutsShield");
                    shieldRenderer.sprite = gutsShield[0];
                    lockedSprite = gutsShield[gutsShield.Length - 1];
                    unlockedSprite = gutsShield[0];
                    shieldRenderer.color = Color.white;
                    open = Resources.Load<AudioClip>("Sounds/Doors/BeastGutsDoorOpen");
                    close = Resources.Load<AudioClip>("Sounds/Doors/BeastGutsDoorClose");
                    allowShieldTint = false;
                    break;
            }

            if (toConfusionRoom)
            {
                var sprites = Resources.LoadAll<Sprite>("Sprites/Doors/" + spriteName);
                external.sprite = sprites[0];
                var anim = external.gameObject.AddComponent<SimpleAnimator>();
                anim.sprites = sprites;
                anim.fps = 6;
            }
            else
            {
                external.sprite = Resources.Load<Sprite>("Sprites/Doors/" + spriteName);
            }
        }
    }

    public override void ApplyStatusEffect(StatusEffect statusEffect, Team team) { return; }
    public override void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team) { return; }
}
