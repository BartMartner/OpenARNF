using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProjectileStats
{
    [Tooltip("Use realistic projectile motion where speed is horizontal velocity and gravity is vertical velocity")]
    public bool projectileMotion;
    public ProjectileType type = ProjectileType.Generic;
    public Team team;
    public float damage = 1;
    [EnumFlags] public DamageType damageType = DamageType.Generic;
    public Explosion explosion = Explosion.None;
    public float explosionDamage = 2f;
    public float speed = 8;
    [Tooltip("Will cause the speed applied to this projectile to vary +/- by this amount")]
    public float speedDeviation;
    public float lifeSpan = 10;
    public float gravity = 0;
    [Tooltip("Will cause the gravity applied to this projectile to vary +/- by this amount")]
    public float gravityDeviation;
    public float homing = 0;
    public float homingRadius = 0;
    public float homingArc = 90;
    public float size = 1;
    public float sizePerSecond = 0f;
    public float damageGainPerSecond = 0f;
    public AnimationCurve motionPattern;
    public bool lockRotation;
    public bool ignoreTerrain;
    public bool ignoreAegis;
    public bool shootable = true;
    public bool penetrative;
    public bool bounce;
    public bool fragment;
    public bool canOpenDoors;
    public bool ignoreDoorLayer;
    public float preShotInvisTime;
    public ProjectileFragmentStats fragmentStats;
    public GameObject spawnOnHit;
    public bool spawnAtContact;
    public AudioClip shootSound;
    public bool spawnCreep;
    public CreepStats creepStats;
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public List<ProjectileChildEffect> childEffects = new List<ProjectileChildEffect>();
    public Action<IDamageable> onHurt;

    [HideInInspector]
    public bool isFragment;

    public ProjectileStats() { }

    public ProjectileStats(ProjectileStats stats)
    {
        type = stats.type;
        team = stats.team;
        projectileMotion = stats.projectileMotion;
        damage = stats.damage;
        damageType = stats.damageType;
        explosion = stats.explosion;
        explosionDamage = stats.explosionDamage;
        speed = stats.speed;
        speedDeviation = stats.speedDeviation;
        lifeSpan = stats.lifeSpan;
        gravity = stats.gravity;
        gravityDeviation = stats.gravityDeviation;
        homing = stats.homing;
        homingRadius = stats.homingRadius;
        homingArc = stats.homingArc;
        size = stats.size;
        sizePerSecond = stats.sizePerSecond;
        damageGainPerSecond = stats.damageGainPerSecond;
        bounce = stats.bounce;
        motionPattern = stats.motionPattern;
        lockRotation = stats.lockRotation;
        ignoreTerrain = stats.ignoreTerrain;
        ignoreAegis = stats.ignoreAegis;
        ignoreDoorLayer = stats.ignoreDoorLayer;
        penetrative = stats.penetrative;
        fragment = stats.fragment;
        fragmentStats = new ProjectileFragmentStats(stats.fragmentStats);
        canOpenDoors = stats.canOpenDoors;
        spawnOnHit = stats.spawnOnHit;
        childEffects = new List<ProjectileChildEffect>(stats.childEffects);
        preShotInvisTime = stats.preShotInvisTime;
        shootable = stats.shootable;
        shootSound = stats.shootSound;
        spawnCreep = stats.spawnCreep;
        creepStats = stats.creepStats;
        statusEffects = new List<StatusEffect>(stats.statusEffects);
    }
}

[Serializable]
public class ProjectileFragmentStats
{
    public float sizeMod = 0.5f;
    public float damageMod = 0.5f;
    public int amount = 3;
    public float arc = 33;
    public float lifeSpan = 3f;
    public int recursion = 0;

    public ProjectileFragmentStats() { }

    public ProjectileFragmentStats(ProjectileFragmentStats stats)
    {
        if (stats != null)
        {
            sizeMod = stats.sizeMod;
            damageMod = stats.damageMod;
            amount = stats.amount;
            arc = stats.arc;
            lifeSpan = stats.lifeSpan;
            recursion = stats.recursion;
        }        
    }
}