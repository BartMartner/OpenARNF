using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LaserStats
{
    public LaserType laserType;
    public Team team;
    public float damage;
    public float stopTime = 0.25f;
    public float width = 1;
    public DamageType damageType = DamageType.Generic;
    public bool ignoreDoors = true;
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public LayerMask layerMask;
    public string sortingLayerName = "AboveTiles";
    public int sortingOrder;
    public float range = 24;
    public LaserStopType stopType = LaserStopType.Shrink;
}
