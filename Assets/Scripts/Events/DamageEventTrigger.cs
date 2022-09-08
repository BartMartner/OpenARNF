using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEventTrigger : BaseEvent, IDamageable
{
    public bool targetable { get { return true; } }
    public Vector3 position { get { return transform.position; } }

    public DamageableState state
    {
        get
        {
            return DamageableState.Alive;
        }

        set
        {
            Debug.LogWarning("The damageable state of a DamageTrigger cannot be set.");
        }
    }

    public bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        if (!_eventCycleActive)
        {
            StartEvent();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ApplyStatusEffect(StatusEffect statusEffect, Team team) { }
    public void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team) { }
}
