using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChildDamagable : MonoBehaviour, IDamageable
{
    public Damageable parent;
    public bool targetable { get { return parent.targetable; } }
    public Vector3 position { get { return transform.position; } }

    public DamageableState state
    {
        get { return parent.state; }
        set { parent.state = value; }
    }

    public void Awake()
    {
        if(!parent)
        {
            parent = GetComponentInParent<Damageable>();
        }
    }

    public bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        return parent.Hurt(damage, source, damageType, ignoreAegis);
    }

    public void ApplyStatusEffect(StatusEffect statusEffect, Team team)
    {
        parent.ApplyStatusEffect(statusEffect, team);
    }

    public void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team)
    {
        parent.ApplyStatusEffects(statusEffects, team);
    }
}
