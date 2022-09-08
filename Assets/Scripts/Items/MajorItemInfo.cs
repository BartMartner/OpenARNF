using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemInfo : ScriptableObject
{
    public string fullName;
    public string description;
    public string itemPageDescription;
    public Sprite icon;
    public GameObject advancedDescription;
    public AchievementID requiredAchievement;

    public bool gunSmithPool = false;
    public bool orbSmithPool = false;
    public bool artificerPool = false;
    public bool theTraitorPool = true;
    public bool nontraversalPool = true;
    public bool deathmatch = false;
    public bool bossRushPool = true;
    public bool startingItemPool = true;
    public ShrineType[] shrinePools;

    [Header("Shop and Cost")]
    public int grayScrapCost;
    public int redScrapCost;
    public int greenScrapCost;
    public int blueScrapCost;
}

[CreateAssetMenu(fileName = "MajorItemInfo", menuName = "Major Items/Create Major Item Info", order = 1)]
public class MajorItemInfo : ItemInfo
{
    [Header("Major Item Info")]
    public MajorItem type;
    public bool isTraversalItem;
    public bool isActivatedItem;
    public MajorItem[] rendersUseless;
    /// <summary>
    /// The Major Item will not show up in these environments
    /// </summary>
    public EnvironmentType[] restrictedEnvironments = new EnvironmentType[0];
    public bool finalItem;

    [Header ("Weapon Properties")]
    public bool isEnergyWeapon;
    public bool noSprite;
    public bool applyDamageTypeToProjectile;
    public DamageType damageType = 0;
    public ProjectileType projectileType = ProjectileType.Generic;
    public int arcShots;
    public float fireArc;
    public bool penetrativeShot;
    public float homing;
    public float homingRadius;
    public float homingArc;
    public bool ignoreTerrain;
    public float speedMod = 1;
    public bool bounce;
    public float sizePerSecond;
    public float damageGainPerSecond;
    public ProjectileChildEffect projectileChildEffect;
    public StatusEffect[] projectileStatusEffects;

    [Header("Stat Bonuses")]
    public float baseDamageBonus;
    public float baseHealthBonus;
    public float baseEnergyBonus;
    public float baseSpeedBonus;
    public float baseShotSpeedBonus;
    public float baseShotSizeBonus;
    public float baseAttackMultiplier;
    public float healthMultiplier;
    public float energyMultiplier;
    public float damageMultiplier;
    public float shotSizeMultiplier;

    [Header("Hovering")]
    public bool allowHovering;
    public float hoverTime;
    public float hoverMaxVelocity;

    [Header("Other")]
    public float regenerationRate;
    public float itemEnergyRegenRate;
    public float pickUpRangeBonus;
    public EnvironmentalEffect environmentalResistance = 0;
    public bool follower;
    public int bonusNanobots;
    public float blessingTimeMod;
    
    [Header("Sprite Properties")]
    //should indicate priority on the following
    //public float spritePriority;
    public bool paletteOverride;
    public bool headSprite;
    public bool armSprite;
    public bool armDecal;
    public bool legSprite;
    public bool legDecal;
    public bool shoulderPadSprite;
    public bool torsoDecal;
    public bool torsoSprite;
    public float decalOrder;
}
