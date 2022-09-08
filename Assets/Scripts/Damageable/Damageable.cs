using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Events;
using System.Linq;

public class Damageable : MonoBehaviour, IDamageable, ILiquidSensitive
{
    public DamageableState state { get; set; }

    [Header("Health and Damage")]
    public float health = 1;
    public float maxHealth;
    public bool ignoreRepeatedDamageSource;
    public bool notTargetable;
    public bool targetable { get { return !notTargetable; } }
    private GameObject _lastDamageSource;

    protected bool _aegisActive;
    public bool aegisActive
    {
        get { return _aegisActive; }
    }
    protected bool _softAegisActive;

    public float aegisTime { get; protected set; }
    public float deathTime = 0f;
    protected float _deathCounter;

    [EnumFlags]
    public DamageType immunities;
    [EnumFlags]
    public DamageType vulnerabilities;
    [EnumFlags]
    public DamageType resistances;

    public StatusEffectsType[] statusImmunities;
    public bool allowStatusTint = true;
    public Dictionary<StatusEffectsType, StatusEffect> statusEffects = new Dictionary<StatusEffectsType, StatusEffect>();
    private HashSet<StatusEffectsType> _statusEffectsToRemove = new HashSet<StatusEffectsType>();
    public bool invincible;

    [Header("Death")]
    public FXType deathFX;
    public Collider2D deathFXBoundsOverride;
    public int deathFXAmount = 1;
    public bool destroyOnDeath = true;
    public UnityEvent onStartDeath;
    public UnityEvent onEndDeath;
    public float onHurtLimit = 0.25f;
    public UnityEvent onHurt;
    public Damageable linkedDamageable;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip immuneSound;
    public AudioClip deathSound;
    public AudioClip deathEndSound;
    private float _onHurtTimer;

    [Header("Visuals")]
    public Animator animator;
    public bool flashOnHurt = true;
    protected SpriteRenderer[] _renderers;
    public SpriteRenderer mainRenderer { get { return (_renderers != null && _renderers.Length > 0) ? _renderers[0] : null; } }

    protected bool _flashing;
    public bool flashing { get { return _flashing; } }
    private Color _defaultFlashColor = Color.white;
    private float _defaultFlashAmount;

    protected Collider2D _collider2D;
    new public Collider2D collider2D
    {
        get { return _collider2D; }
    }

    public Transform altPosition;
    public virtual Vector3 position
    {
        get { return altPosition ? altPosition.position : transform.position; }
    }

    public bool inLiquid { get; set; }
    public bool electrifiesWater { get { return false; } }

    protected virtual void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        state = DamageableState.Alive;
        if (!audioSource) { audioSource = GetComponentInChildren<AudioSource>(); }
        if (!animator) { animator = GetComponentInChildren<Animator>(); }
        maxHealth = health;
    }

    protected virtual void Start()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        _lastDamageSource = null;

        if (_onHurtTimer > 0) { _onHurtTimer -= Time.unscaledDeltaTime; }

        switch (state)
        {
            case DamageableState.Dead:
                return;
            case DamageableState.Dying:
                _deathCounter += Time.deltaTime;
                if (_deathCounter > deathTime)
                {
                    state = DamageableState.Dead;
                    EndDeath();
                }
                break;
            case DamageableState.Alive:
                UpdateAlive();

                if(statusEffects.Count > 0)
                {                    
                    foreach (var effect in statusEffects.Values)
                    {
                        if (effect.lifeSpan > 0)
                        {
                            effect.Update();
                        }
                        else
                        {
                            effect.Remove();
                            _statusEffectsToRemove.Add(effect.type);
                            //Debug.Log("removing status effect");
                        }
                    }

                    if (_statusEffectsToRemove.Count > 0)
                    {
                        foreach (var effect in _statusEffectsToRemove)
                        {
                            statusEffects.Remove(effect);
                        }
                        _statusEffectsToRemove.Clear();
                    }
                }
                break;
        }
    }

    protected virtual void UpdateAlive()
    {
        if (health <= 0) { StartDeath(); }
    }

    public IEnumerator Aegis(float aegisTime)
    {
        _aegisActive = true;
        yield return new WaitForSeconds(aegisTime);
        _aegisActive = false;
    }

    private void SetFlashColor(Color color, float amount)
    {
        if (_renderers == null)
        {
            Debug.LogWarning(gameObject.name + " Damageable has null _renderers Array");
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            if (renderer != null)
            {
                renderer.material.SetColor("_FlashColor", color);
                renderer.material.SetFloat("_FlashAmount", amount);
            }
        }
    }

    public void SetDefaultFlashColor(Color color, float amount)
    {
        if (_renderers == null)
        {
            Debug.LogWarning(gameObject.name + " Damageable has null _renderers Array");
            return;
        }

        _defaultFlashColor = color;
        _defaultFlashAmount = amount;

        if (!flashing)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                if (renderer != null)
                {
                    renderer.material.SetColor("_FlashColor", color);
                    renderer.material.SetFloat("_FlashAmount", amount);
                }
            }
        }
    }

    public void StartFlash(int flashes, float time, Color color, float amount, bool fade)
    {
        if (fade)
        {
            StartCoroutine(FadeFlash(flashes, time, color, amount));
        }
        else
        {
            StartCoroutine(Flash(flashes, time, color, amount));
        }
    }

    public IEnumerator FadeColor(float time, Color color, float amount, bool to)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;

        var timer = 0f;        
        while (timer < time)
        {
            timer += Time.deltaTime;
            SetFlashColor(color, to ? Mathf.Lerp(0, amount, timer / time) : Mathf.Lerp(amount, 0, timer / time));
            yield return null;
        }
    }

    protected virtual IEnumerator FadeFlash(int flashes, float time, Color color, float amount)
    {
        if(_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;
        var flashCounter = 0;        

        while (flashCounter < flashes)
        {
            flashCounter++;
            var timer = 0f;
            while(timer < time)
            {
                timer += Time.deltaTime;
                SetFlashColor(color, Mathf.Lerp(amount, 0, timer / time));
                yield return null;
            }
        }

        StopFlash();
    }

    protected virtual IEnumerator Flash(int flashes, float time, Color color, float amount)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;
        var flashCounter = 0;

        while (flashCounter < flashes)
        {
            flashCounter++;
            SetFlashColor(color, amount);
            yield return new WaitForSeconds(time * 0.5f);
            SetFlashColor(_defaultFlashColor, _defaultFlashAmount);
            yield return new WaitForSeconds(time * 0.5f);
        }

        StopFlash();
    }

    public virtual void StartMultiColorFlash(float timePerFlash, Color32[] colors, float amount, bool fade)
    {
        StartCoroutine(MultiColorFlash(timePerFlash, colors, amount, fade));
    }

    protected virtual IEnumerator MultiColorFlash(float timePerFlash, Color32[] colors, float amount, bool fade)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;

        foreach (var color in colors)
        {
            if (fade)
            {
                yield return StartCoroutine(Flash(1, timePerFlash, color, amount));
            }
            else
            {
                yield return StartCoroutine(FadeFlash(1, timePerFlash, color, amount));
            }
        }

        StopFlash();
    }

    public void StopFlash()
    {
        foreach (var renderer in _renderers)
        {
            if (renderer != null)
            {
                renderer.material.SetColor("_FlashColor", _defaultFlashColor);
                renderer.material.SetFloat("_FlashAmount", _defaultFlashAmount);
            }
        }
        _flashing = false;
    }

    public virtual bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        //if aegis is active and ignoreAegis is false, don't hurt the damageable
        if (health <= 0 || state != DamageableState.Alive || invincible || (_aegisActive && !ignoreAegis)) { return false; }

        if (immunities != 0 && immunities.HasFlag(damageType))
        {
            OnImmune(damageType);
            return false;
        }

        if (ignoreRepeatedDamageSource && _lastDamageSource == source) { return false; }

        _lastDamageSource = source;

        if (linkedDamageable) { linkedDamageable.Hurt(damage, source, damageType, true); }

        if (vulnerabilities != 0)
        {
            foreach (var d in damageType.GetFlags())
            {
                if (vulnerabilities.HasFlag(d)) { damage *= 2; }
            }
        }

        if (resistances != 0)
        {
            foreach (var d in damageType.GetFlags())
            {
                if (resistances.HasFlag(d)) { damage *= 0.5f; }
            }
        }

        HandleDamage(damage);

        if (flashOnHurt && !_flashing) { StartCoroutine(Flash(1, 0.2f, Constants.damageFlashColor, 0.5f)); }

        if (_onHurtTimer <= 0)
        {
            if (onHurt != null) { onHurt.Invoke(); }
            _onHurtTimer = onHurtLimit;
            if (hurtSound && (health > 0 || !deathSound)) { PlayOneShot(hurtSound); }
        }

        if (!_aegisActive && !ignoreAegis && aegisTime > 0 )
        {
            StartCoroutine(Aegis(aegisTime));
        }

        return true;
    }

    public virtual void ApplyStatusEffect(StatusEffect statusEffect, Team team)
    {
        if (statusImmunities.Contains(statusEffect.type)) return;
        if (inLiquid && statusEffect.type == StatusEffectsType.Burn) return;

        if (!statusEffects.ContainsKey(statusEffect.type))
        {
            var copy = StatusEffect.CopyOf(statusEffect);
            copy.team = team;
            copy.Add(this);
            statusEffects.Add(statusEffect.type, copy);
        }
        else
        {
            statusEffects[statusEffect.type].Stack(statusEffect);
        }
    }

    public virtual void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team)
    {
        foreach (var effect in statusEffects) { ApplyStatusEffect(effect, team); }
    }

    public virtual void HandleDamage(float damageAmount) { health -= damageAmount; }

    public virtual void OnImmune(DamageType damageType)
    {
        if (damageType == DamageType.Generic) { StartCoroutine(FadeFlash(1, 0.4f, Color.white, 0.5f)); }
        if (immuneSound) { PlayOneShot(immuneSound); }
    }

    public virtual void StartDeath()
    {
        if (state != DamageableState.Alive) { return; }

        health = 0;
        state = DamageableState.Dying;
        _deathCounter = 0;

        if (deathSound) { AudioManager.instance.PlayOneShot(deathSound); }
        //if (deathSound) { AudioSource.PlayClipAtPoint(deathSound, MainCamera.instance.transform.position); }

        if (deathFX != FXType.None)
        {
            if (deathFXAmount > 1)
            {
                StartCoroutine(SpawnDeathFXOvertime());
            }
            else
            {
                FXManager.instance.SpawnFX(deathFX, position);
            }
        }

        if (onStartDeath != null) { onStartDeath.Invoke(); }
    }

    public IEnumerator SpawnDeathFXOvertime()
    {
        var delay = new WaitForSeconds(deathTime / (float)deathFXAmount);
        Bounds area;

        for (int a = 0; a < deathFXAmount; a++)
        {    
            if (deathFXBoundsOverride)
            {
                area = deathFXBoundsOverride.bounds;
            }
            else
            {
                area = _collider2D ? _collider2D.bounds : new Bounds(position, new Vector3(1, 1, 0));
            }
            
            FXManager.instance.SpawnFX(deathFX, Extensions.randomInsideBounds(area));
            yield return delay;
        }
    }

    public IEnumerator FadeOut(float time, bool destroy)
    {
        var timer = 0f;

        //Turn on fade
        foreach (var renderer in _renderers)
        {
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }

        while (timer < time)
        {
            timer += Time.deltaTime;
            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    renderer.color = Color.Lerp(Color.white, Color.clear, timer / time);
                }
            }
            yield return null;
        }

        if (destroy) { Destroy(gameObject); }
    }

    public virtual void EndDeath()
    {
        if (deathEndSound) { AudioManager.instance.PlayOneShot(deathEndSound); }
        if (onEndDeath != null) { onEndDeath.Invoke(); }
        if (destroyOnDeath) { Destroy(gameObject); }
    }

    public virtual void DestroySelf(float delay) { Destroy(gameObject, delay); }

    public void SetInvincible(bool value) { invincible = value; }
    public void SetTargetable(bool value) { notTargetable = !value; }

    public Vector3 GetRandomFXPosition()
    {
        var fxPosition = Vector2.zero;

        if(deathFXBoundsOverride != null)
        {
            fxPosition = Extensions.randomInsideBounds(deathFXBoundsOverride.bounds);
        }
        else if (collider2D)
        {
            fxPosition = Extensions.randomInsideBounds(collider2D.bounds);
        }
        else
        {
            fxPosition = (Vector2)position + Random.insideUnitCircle;
        }

        fxPosition.y += 0.5f;
        return fxPosition;
    }

    public virtual bool OnEnterLiquid(Water water)
    {
        foreach (var s in statusEffects)
        {
            if (s.Key == StatusEffectsType.Burn) { s.Value.lifeSpan = 0; }
        }

        return !statusImmunities.Contains(StatusEffectsType.Burn);
    }

    public virtual void OnExitLiquid() { }

    public void PlayOneShot(AudioClip audioClip)
    {
        PlayOneShot(audioClip, 1);
    }

    public void PlayOneShot(AudioClip audioClip, float volumeScale = 1)
    {
        if(!audioClip)
        {
            Debug.LogError(gameObject.name + " Damageable tried to play a null audioClip!");
            return;
        }

        if(audioSource)
        {
            audioSource.PlayOneShot(audioClip, volumeScale);
        }
        else
        {
            AudioManager.instance.PlayClipAtPoint(audioClip, position, volumeScale);
        }
    }
}
