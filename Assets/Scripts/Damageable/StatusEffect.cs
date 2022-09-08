using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "New Status Effect", order = 1)]
public class StatusEffect : ScriptableObject, IHasTeam
{
    public StatusEffectsType type;
    public Color color;
    public float colorAmount = 0.1f;
    public FXType fXType;
    public float fXInterval = 0.1f;
    public float lifeSpan;
    public float amount;

    private float _damageBuildUp;
    private float _fxTimer;
    private Damageable _damageable;
    private IReactsToStatusEffect[] _reacts;
    private bool _added;
    private float _lastHealth;

    [NonSerialized]
    private Team _team;
    public Team team
    {
        get { return _team; }
        set { _team = value; }
    }

    public StatusEffect() { _team = team; }

    public static StatusEffect CopyOf(StatusEffect original)
    {
        var copy = ScriptableObject.CreateInstance<StatusEffect>();
        copy.team = original.team;
        copy.type = original.type;
        copy.color = original.color;
        copy.colorAmount = original.colorAmount;
        copy.fXType = original.fXType;
        copy.fXInterval = original.fXInterval;
        copy.lifeSpan = original.lifeSpan;
        copy.amount = original.amount;
        return copy;
    }

    public void Update()
    {
        lifeSpan -= Time.deltaTime;

        if (lifeSpan > 0 && _damageable)
        {
            _fxTimer += Time.deltaTime;
            var acutalInterval = fXInterval;
            if (_damageable.collider2D) //more fx for larger creatures
            {
                var area = (_damageable.collider2D.bounds.extents.x * _damageable.collider2D.bounds.extents.y) * 2;
                if (area > (3.5f * 3.5f))
                {
                    acutalInterval *= 0.5f;
                }
                else if (area > (2.5f * 2.5f))
                {
                    acutalInterval *= 0.75f;
                }
            }

            if (_fxTimer > acutalInterval)
            {
                _fxTimer -= acutalInterval;

                Vector2 position = _damageable.GetRandomFXPosition();
                FXManager.instance.SpawnFX(fXType, position);
            }

            if(type == StatusEffectsType.Burn || 
                type == StatusEffectsType.Acid ||
                type == StatusEffectsType.Poison)
            {
                _damageBuildUp += amount * Time.deltaTime;
                if (_damageBuildUp > 1)
                {
                    _damageable.HandleDamage(1);
                    _damageBuildUp -= 1;

                    if (DeathmatchManager.instance && _damageable.health <= 0 && _damageable.state == DamageableState.Alive)
                    {
                        var hasTeam = _damageable.gameObject.GetComponent<IHasTeam>();
                        if (hasTeam != null)
                        {
                            //if the player dies from a status effect award points
                            DeathmatchManager.instance.AwardPoints(team, hasTeam.team);
                        }
                    }
                }
            }

            if(type == StatusEffectsType.Acid)
            {
                if (_damageable.health > _lastHealth || _damageable.health <= 1) { lifeSpan = 0; }
            }

            _lastHealth = _damageable.health;
        }
    }

    public void Add(Damageable damageable)
    {
        if(_added)
        {
            throw new Exception("Trying to add a status effect that has already been added!");
        }

        _added = true;
        _damageable = damageable;
        _lastHealth = _damageable.health;
        _reacts = _damageable.gameObject.GetInterfacesInChildren<IReactsToStatusEffect>(true).ToArray();

        _damageable.SetDefaultFlashColor(color, colorAmount);

        for (int i = 0; i < _reacts.Length; i++)
        {
            if (_reacts[i] != null) { _reacts[i].OnAddEffect(this); }
        }
    }

    public void Remove()
    {
        if (!_added)
        {
            throw new Exception("Trying to remove a status effect that has never been added!");
        }

        _damageable.SetDefaultFlashColor(Color.white, 0);

        if(_reacts != null)
        {
            for (int i = 0; i < _reacts.Length; i++)
            {
                if (_reacts[i] != null) { _reacts[i].OnRemoveEffect(this); }
            }
        }

        Destroy(this);
    }

    public void Stack(StatusEffect statusEffect)
    {
        if (statusEffect.colorAmount > colorAmount) { colorAmount = statusEffect.colorAmount; }
        if (statusEffect.amount > amount) { amount = statusEffect.amount; }
        if (statusEffect.lifeSpan > lifeSpan) { lifeSpan = statusEffect.lifeSpan; }
        if (statusEffect.fXInterval < fXInterval) { fXInterval = statusEffect.fXInterval; }

        if (_reacts != null)
        {
            for (int i = 0; i < _reacts.Length; i++)
            {
                if (_reacts[i] != null) { _reacts[i].OnStackEffect(this); }
            }
        }
    }
}
