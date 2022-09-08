using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class DamageCreatureTrigger : MonoBehaviour, IHasTeam, ILiquidSensitive
{
    public float damage = 1;
    public bool perSecond;
    public DamageType damageType = DamageType.Generic;
    [Tooltip("If this is set true, damage is best thought as damage per frame.")]
    public bool ignoreAegis;
    public bool ignoreDoors;
    public bool ignoreSwitches;
    public UnityEvent onDamage;
    public Action<IDamageable> onDamageParam;
    public List<StatusEffect> statusEffects;

    [HideInInspector]
    private Collider2D _collider2D;
    new public Collider2D collider2D
    {
        get
        {
            if (!_collider2D) { _collider2D = GetComponent<Collider2D>(); }
            return _collider2D;
        }
    }

    private Team _team;
    /// <summary>
    /// Warning! This is currently only relevant to Deathmatch
    /// </summary>
    public Team team
    {
        get { return _team; }
        set { _team = value; }
    }

    public bool inLiquid { get; set; }
    public bool electrifiesWater { get { return damageType == DamageType.Electric; } }

    public void OnTriggerStay2D(Collider2D other)
    {
        OnHit(other);
    }

    public void OnHit(Collider2D other)
    {
        if (!enabled) return;

        var hasTeam = other.gameObject.GetComponent<IHasTeam>();
        if (hasTeam == null || hasTeam.team != _team)
        {
            var damagable = other.gameObject.GetComponent<IDamageable>();
            if (damagable == null || !damagable.enabled) { return; }
            if (ignoreSwitches && (other.gameObject.GetComponent<ShootSwitch>())) { return; }

            var layer = LayerMask.LayerToName(other.gameObject.layer);
            if (ignoreDoors && (damagable is Door || layer == "Door")) { return; }

            var rDamage = perSecond ? damage * Time.deltaTime : damage;
            if (_team == Team.Player && PlayerManager.instance) { rDamage *= PlayerManager.instance.coOpMod; }

            if (damagable.Hurt(rDamage, gameObject, damageType, ignoreAegis))
            {
                if (statusEffects != null && statusEffects.Count > 0)
                {
                    damagable.ApplyStatusEffects(statusEffects, team);
                }

                if (onDamage != null) { onDamage.Invoke(); }
                if (onDamageParam != null) { onDamageParam(damagable); }
            }
        }
    }

    public bool OnEnterLiquid(Water water)
    {
        return damageType == DamageType.Electric;
    }

    public void OnExitLiquid() { }
}
