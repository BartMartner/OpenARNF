using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAcidCloudOnDeath : MonoBehaviour
{
    public float size = 4;
    public Team team;
    public float lifespan = 5;
    public StatusEffect statusEffect;
    
    private Damageable _damageable;
    public void Awake()
    {
        _damageable = GetComponent<Damageable>();
        if (_damageable)
        {
            _damageable.onEndDeath.AddListener(OnDeath);
        }
    }

    public void OnDeath()
    {
        ProjectileManager.instance.SpawnDamageCloud(statusEffect, transform.position, size, team, lifespan);
    }
}
