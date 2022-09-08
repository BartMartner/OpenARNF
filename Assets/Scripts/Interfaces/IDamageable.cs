using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    GameObject gameObject { get; }
    DamageableState state { get; set; }
    bool enabled { get; set; }
    bool targetable { get; }
    Vector3 position { get; }
    bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false);
    void ApplyStatusEffect(StatusEffect statusEffect, Team team);
    void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team);
}
