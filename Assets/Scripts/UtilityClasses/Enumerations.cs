using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// For [Flag] enums, 0 can count as none, nothing or null and be checked in code that way, 
/// but excluding it allows for [Flag] enums to be treated like Unity's Layer Masks
/// </summary>

public enum Team
{
    None = 0,
    Player = 1,
    Enemy = 2,
    DeathMatch0 = 3,
    DeathMatch1 = 4,
    DeathMatch2 = 5,
    DeathMatch3 = 6,
}

public enum DamageableState
{
    Alive = 0,
    Dying = 1,
    Dead = 2,
}

public enum ProjectileType
{
    Generic = 0,
    BlasterBolt = 1,
    TumorBullet = 2,
    Rocket = 3,
    ElectroBolt = 4,
    BioPlasma = 5,
    GreenBioPlasma = 6,
    BigSlimeColumnBullet = 7,
    FireBolt = 8,
    ElectroFireBolt = 9,
    PenetrativeBolt = 10,
    Buzzsaw = 11,
    ExplosiveBolt = 12,
    PhaseBolt = 13,
    EyePlasma = 14,
    Molotov = 15,
    YellowBioPlasma = 16,
    EnergyAxe = 17,
    WaveBomb = 18,
    Bullet = 19,
    ElectroplosiveBolt = 20,
    ExplosiveFireBolt = 21,
    ElectroplosiveFireBolt = 22,
    SmithBolt = 23,
    SlimeBullet = 24,
    NecroluminantSpray = 25,
    RedBioPlasma = 26,
    ToxinBolt = 27,
    WebBolt = 28,
    BounceBolt = 29,
    PulseGrenade = 30,
}

public enum LaserType
{    
    Slime = 0,
    Heat = 1,
    Bioplasma = 2,
    Energy = 3,
    BigEnergy = 4,
    BigHeat = 5,
    BlueBioPlasma = 6,
    BasicBeam = 7,
    TutSmith = 8,
    BigSlime = 9,
    BigBioPlasma = 10,
    RailGun = 11,
}

public enum LaserStopType
{
    Shrink = 0,
    Fade = 1,
}

public enum Explosion
{
    None,
    E16x16,
    E32x32,
    E48x48,
    E64x64,
    Elec16x16,
    Elec32x32,
    Elec48x48,
    Elec64x64,
}

public enum GibType
{
    Meat = 0,    
    BrownRock = 1,
    CaveMetal = 2,    
    PaleMeat = 3,
    PinkMeat = 4,
    ExplosiveDoorShield = 5,
    Concrete = 6,
    BuriedCityRock = 7,
    GreenMeat = 8,
    GlitchGib = 9,
    GreenTech = 10,
    BoneDoorShield = 11,
}

public enum FXType
{
    None = 0,
    BloodSplatSmall = 1,
    SmokePuffSmall = 2,
    FlameSmall = 3,
    ExplosionSmall = 4,
    ExplosionMedium =5,
    Teleportation = 6,
    AnimeSplode = 7,
    DashPush = 8,
    AcidBubbles = 9,
    PoisonBubbles = 10,
    Splash32 = 11,
    AcidSplash32 = 12,
    MuzzleFlash = 13,
}

public enum Possibility
{
    Never = 0,
    Sometimes = 1,
    Always = 2,
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
}

public enum AutomapSegmentState
{
    Hidden,
    Discovered,
    Visited,
}

[Flags]
public enum DamageType
{
    Generic = 1,
    Explosive = 2,
    Velocity = 4,
    Cold = 8,
    Electric = 16,
    Mechanical = 32,
    Fire = 64,
    Hazard = 128,
}


[Flags]
public enum EnvironmentalEffect
{
    None = 0,
    Heat = 1,
    Darkness = 2,
    Fog = 4,
    Strobe = 8,
    Pulse = 16,
    Confusion = 32,
    Underwater = 64,
}

public enum MinorItemType
{
    None = 0,
    HealthTank = 1,
    DamageModule = 2,
    EnergyModule = 3,
    RedScrap = 4,
    GreenScrap = 5,
    BlueScrap = 6,
    AttackModule = 7,
    SpeedModule = 8,
    ShotSpeedModule = 9,
    GlitchModule = 10,
    GlitchScrap = 11,
}

public enum PlayerStatType
{
    Attack,
    Damage,
    Health,
    Energy,
    ShotSize,
    ShotSpeed,
    Speed,
}

[Flags]
public enum Direction
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 4,
    Left = 8,
}

public enum EnvironmentType
{
    Surface = 0,
    Cave = 1,
    Factory = 2,
    BuriedCity = 3,
    BeastGuts = 4,
    Glitch = 5,
    ForestSlums = 6,
    GreyBox = 7,
    CoolantSewers = 8,
    CrystalMines = 9,
}

public enum BossName
{
    None = 0,
    MouthMeatSenior = 1,
    MetalPatriarch = 2,
    WallCreep = 3,
    FleshAdder = 4,
    Abomination = 5,
    BeakLord = 6,
    Polyphemus = 7,
    MolemanShaman = 8,
    Cephalodiptera = 9,
    MegaBeastCore = 10,
    Sluggard = 11,
    WhiteWyrm = 12,
    MouthMeat = 13,
    GlitchBoss = 14,
    OozeHart = 15,
    Blightbark = 16,
    Leviathan = 17,
    Stalkus = 18,
    SkinDeviler = 19,
    CorruptedMiner = 20,
    BeastRemnants = 21,
}

public enum RoomType
{
    None = 0,
    StartingRoom = 1,
    ItemRoom = 2,
    TransitionRoom = 3,
    SaveRoom = 4,
    BossRoom = 5,
    MegaBeast = 6,
    Shop = 7,
    Shrine = 8,
    Teleporter = 9,
    OtherSpecial = 10,
}

public enum CurrencyType
{
    Gray,
    Red,
    Green,
    Blue,
}

public enum ShopType
{
    None = 0,
    OrbSmith = 1,
    GunSmith = 2,
    Artificer = 3,
    TheTraitor = 4,
}

public enum ShrineType
{
    None = 0,
    Tyr = 1,
    Zurvan = 2,
    Hephaestus = 3,
    WadjetMikail = 4,
    BulucChabtan = 5,
    Orphiel = 6,
}

[Flags]
public enum GameMode
{
    Normal = 1,
    ClassicBossRush = 2,
    Exterminator = 4,
    Spooky = 8,
    MirrorWorld = 16,
    TrueCoOp = 32,
    MegaMap = 64,
    BossRush = 128,
}

public enum TransitionFadeType
{
    Normal,
    BeastGuts,
    Glitch,
    Teleporter,
}

public enum Outcome
{
    Positive,
    Neutral,
    Negative,
}

public enum DeathmatchMode
{
    FragLimit,
    TimeLimit,
    //LastRobotStanding,
}

public enum DeathmatchItemMode
{
    All,
    EnergyOnly,
    None,
}

public enum StatusEffectsType
{
    Burn,
    Acid,
    Poison,
    Slow,
    Fatigue,
}

public enum AIMode
{
    Patrol,
    Hunt,
}

public enum DropType
{
    None,
    SmallHealth,
    SmallEnergy,
    GrayScrap,
    DamageBuff,
    AttackBuff,
    SpeedBuff,
}