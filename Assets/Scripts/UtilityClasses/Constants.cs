using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using CreativeSpore.SuperTilemapEditor;

public static class Constants
{
#if DEBUG
    public static List<MajorItem> excludeItems = new List<MajorItem> { };
#else
    public static List<MajorItem> excludeItems = new List<MajorItem> { };
#endif

    public static string[] notRooms = new string[]
    { "SplashScreenWait", "Backstory", "StartScreen", "NewGame", "BossRush", "MainScene", "Core",
      "EndScreen01", "EndScreen02", "EndScreen03", "SpookyEnding", "Congratulations", "DeathmatchCore", "DeathmatchStressTest"};
    public static readonly LayerMask defaultMask = 1;
    public const int maxSeed = 1838265624; //309,218,023 than max int
    public const float startingMaxJumpHeight = 6.66f;
    public const float startingMinJumpHeight = 0.45f;
    public const float startingJumpTime = 0.68f;
    public const float defaultTempBuffRank = 0.05f;
    public const float startingWaterJumpMod = 0.6f;
    public const int healthTankAmount = 2;
    public const int energyModuleAmount = 2;
    public const float speedModuleAmount = 0.75f;
    public const int startingHealth = 8;
    public const int startingEnergy = 8;
    public const float startingAttackDelay = 0.4f;
    public const float startingMaxSpeed = 10f;
    public const float startingDamage = 3.5f;
    public const float startingProjectileSpeed = 16f;
    public const float damageModuleMultiplier = 0.15f;
    public const float damageModuleBonus = 0.5f;
    public const float transitionFadeTime = 0.16f;
    public const DamageType standardDoorImmunities = DamageType.Velocity;
    public readonly static Color damageFlashColor = new Color(1, 0.1f, 0.1f, 1);
    public readonly static Color32 blasterGreen = new Color32(114,248,168,255);
    public readonly static Color32 unvistedGreen = new Color32(114/2, 248/2, 168/2, 255);
    public readonly static Color32 electroYellow = new Color32(255, 255, 90, 255);
    public readonly static Color32 explosiveRed = new Color32(255, 70, 50, 255);
    public readonly static Color32 fireOrange = new Color32(255, 185, 0, 255);
    public readonly static Quaternion flippedFacing = Quaternion.Euler(0,180,0);
    public const float roomWidth = 24;
    public const float roomHeight = 14;
    public readonly static Vector2 roomSize = new Vector2(roomWidth, roomHeight);
    public static string roomDataPath
    {
        get
        {
            return Application.dataPath + "/Resources/RoomData/";
        }
    }
    public static readonly DamageType allDamageTypes = DamageType.Cold | DamageType.Electric | DamageType.Explosive | DamageType.Fire | DamageType.Generic | DamageType.Hazard | DamageType.Mechanical | DamageType.Velocity;

    public static HashSet<EnvironmentalEffect> traversalEnvEffects = new HashSet<EnvironmentalEffect>
    {
        EnvironmentalEffect.Darkness,
        EnvironmentalEffect.Confusion,
        EnvironmentalEffect.Heat,
        EnvironmentalEffect.Underwater,
    };
    public static HashSet<BossName> surfaceBosses = new HashSet<BossName> { BossName.Abomination, BossName.BeakLord, BossName.MouthMeat, BossName.Sluggard };
    public static HashSet<BossName> forestSlumsBosses = new HashSet<BossName> { BossName.Blightbark, BossName.OozeHart, BossName.MouthMeat };
    public static HashSet<BossName> caveBosses = new HashSet<BossName> { BossName.Cephalodiptera, BossName.MouthMeatSenior, BossName.WallCreep };
    public static HashSet<BossName> coolantSewersBosses = new HashSet<BossName> { BossName.Leviathan, BossName.Stalkus, BossName.WallCreep };
    public static HashSet<BossName> factoryBosses = new HashSet<BossName> { BossName.FleshAdder, BossName.MetalPatriarch, BossName.MouthMeatSenior };
    public static HashSet<BossName> crystalMinesBosses = new HashSet<BossName> { BossName.SkinDeviler, BossName.CorruptedMiner, BossName.FleshAdder };
    public static HashSet<BossName> buriedCityBosses = new HashSet<BossName> { BossName.MolemanShaman, BossName.Polyphemus, BossName.WhiteWyrm };
    public static HashSet<GameMode> postBeastModes = new HashSet<GameMode> { GameMode.BossRush, GameMode.Exterminator };

    public static Dictionary<EnvironmentType, HashSet<BossName>> envBosses = new Dictionary<EnvironmentType, HashSet<BossName>>
    {
        { EnvironmentType.Surface, surfaceBosses},
        { EnvironmentType.ForestSlums, forestSlumsBosses},
        { EnvironmentType.Cave, caveBosses},
        { EnvironmentType.CoolantSewers, coolantSewersBosses},
        { EnvironmentType.Factory, factoryBosses},
        { EnvironmentType.CrystalMines, crystalMinesBosses},
        { EnvironmentType.BuriedCity, buriedCityBosses},
    };

    //Will change once color lazers are abandoned
    public static Dictionary<DamageType, Color32> damageTypeColors = new Dictionary<DamageType, Color32>()
    {
        {DamageType.Cold, new Color32 (166,255,255,255)},
        {DamageType.Velocity, new Color32(200,255,100,255)},
        {DamageType.Fire, fireOrange},
        {DamageType.Mechanical, new Color32(255, 0, 255, 255)},
        {DamageType.Explosive, explosiveRed},
        {DamageType.Electric, electroYellow},
        {DamageType.Hazard, new Color32 (255,230,128, 255) },
    };

    public static Dictionary<MinorItemType, Color32> moduleColors = new Dictionary<MinorItemType, Color32>()
    {
        {MinorItemType.AttackModule, new Color32 (96,176,225,255)},
        {MinorItemType.DamageModule, new Color32(155,55,0,255)},
        {MinorItemType.EnergyModule, new Color32(219, 220, 94, 255)},
        {MinorItemType.HealthTank, new Color32(114, 248, 169, 255)},
        {MinorItemType.ShotSpeedModule, new Color32 (240, 175, 0, 255)},
        {MinorItemType.SpeedModule, new Color32(138,235,0,255)},
    };

    public static Dictionary<PlayerStatType, Color32> statColors = new Dictionary<PlayerStatType, Color32>()
    {
        {PlayerStatType.Attack, new Color32 (96,176,225,255)},
        {PlayerStatType.Damage, new Color32(155,55,0,255)},
        {PlayerStatType.Energy, new Color32(219, 220, 94, 255)},
        {PlayerStatType.Health, new Color32(114, 248, 169, 255)},
        {PlayerStatType.ShotSpeed, new Color32 (240, 175, 0, 255)},
        {PlayerStatType.Speed, new Color32(138,235,0,255)},
        {PlayerStatType.ShotSize, blasterGreen},
    };

    public static readonly MinorItemType[] allStatModules = new MinorItemType[]
    {
        MinorItemType.AttackModule,
        MinorItemType.DamageModule,
        MinorItemType.EnergyModule,
        MinorItemType.HealthTank,
        MinorItemType.ShotSpeedModule,
        MinorItemType.SpeedModule,
    };

    public static ProjectileType GetPlayerProjectileType(ICollection<ProjectileType> projectileTypes)
    {
        ProjectileType pType = ProjectileType.BlasterBolt;

        if (projectileTypes.Contains(ProjectileType.FireBolt))
        {
            if (projectileTypes.Contains(ProjectileType.ElectroBolt))
            {
                if (projectileTypes.Contains(ProjectileType.ExplosiveBolt))
                {
                    pType = ProjectileType.ElectroplosiveFireBolt;
                }
                else
                {
                    pType = ProjectileType.ElectroFireBolt;
                }
            }
            else if (projectileTypes.Contains(ProjectileType.ExplosiveBolt))
            {
                pType = ProjectileType.ExplosiveFireBolt;
            }
            else
            {
                pType = ProjectileType.FireBolt;
            }
        }
        else if (projectileTypes.Contains(ProjectileType.ElectroBolt))
        {
            if (projectileTypes.Contains(ProjectileType.ExplosiveBolt))
            {
                pType = ProjectileType.ElectroplosiveBolt;
            }
            else
            {
                pType = ProjectileType.ElectroBolt;
            }
        }
        else if (projectileTypes.Contains(ProjectileType.ExplosiveBolt))
        {
            pType = ProjectileType.ExplosiveBolt;
        }
        else if (projectileTypes.Contains(ProjectileType.WebBolt))
        {
            pType = ProjectileType.WebBolt;
        }
        else if (projectileTypes.Contains(ProjectileType.ToxinBolt))
        {
            pType = ProjectileType.ToxinBolt;
        }
        else if (projectileTypes.Contains(ProjectileType.PhaseBolt))
        {
            pType = ProjectileType.PhaseBolt;
        }
        else if (projectileTypes.Contains(ProjectileType.BounceBolt))
        {
            pType = ProjectileType.BounceBolt;
        }
        else if (projectileTypes.Contains(ProjectileType.PenetrativeBolt))
        {
            pType = ProjectileType.PenetrativeBolt;
        }
        else
        {
            pType = ProjectileType.BlasterBolt;
        }

        return pType;
    }

    public static DamageType DamageTypeFromItem(MajorItem majorItem)
    {
        if(ItemManager.items.ContainsKey(majorItem))
        {
            return ItemManager.items[majorItem].damageType;
        }
        else
        {
            return 0;
        }
    }

    public static Vector2 WorldToLayoutPosition(Vector3 worldPosition)
    {
        return new Vector2(Mathf.RoundToInt(worldPosition.x / roomWidth), Mathf.RoundToInt(worldPosition.y / roomHeight));
    }

    public static Vector3 LayoutToWorldPosition(Vector2 layoutPosition)
    {
        return new Vector3(layoutPosition.x * roomWidth, layoutPosition.y * roomHeight);
    }

    public static Vector3 LayoutToWorldPosition(Int2D layoutPosition)
    {
        return new Vector3((layoutPosition.x + 0.5f) * roomWidth, (layoutPosition.y + 0.5f) * roomHeight);
    }

    public static int[] EvenSplit(int numberToSplit, int waysToSplit, bool ascending = false)
    {
        if(numberToSplit < 0)
        {
            Debug.LogError("DivideEvenly does not support negative numbers!");
            return null;
        }

        var split = new int[waysToSplit];
        int remainder;
        int div = Math.DivRem(numberToSplit, waysToSplit, out remainder);
        for (int i = 0; i < waysToSplit; i++)
        {
            split[i] = i < remainder ? div + 1 : div;
        }

        if (ascending)
        {
            return split.Reverse().ToArray();
        }
        else
        {
            return split;
        }
    }

    public static int[] UnevenSplit(int numberToSplit, int waysToSplit, MicrosoftRandom random)
    {
        var split = new int[waysToSplit];
        for (int i = 0; i < split.Length; i++)
        {
            if (waysToSplit == 1)
            {
                split[i] = numberToSplit;
            }
            else
            {
                var evenSplit = numberToSplit / (float)waysToSplit;
                var number = random.value > 0.5 ? Mathf.CeilToInt(evenSplit) : Mathf.FloorToInt(evenSplit);
                number += random.value > 0.5f ? -1 : 1;
                number = Mathf.Clamp(number, 0, numberToSplit);
                split[i] = number;
                waysToSplit--;
                numberToSplit -= number;
            }
        }

        return split;
    }

    public static List<List<T>> GetAllCombos<T>(List<T> list)
    {
        List<List<T>> result = new List<List<T>>();
        // head
        result.Add(new List<T>());
        result.Last().Add(list[0]);
        if (list.Count == 1)
            return result;
        // tail
        List<List<T>> tailCombos = GetAllCombos(list.Skip(1).ToList());
        tailCombos.ForEach(combo =>
        {
            result.Add(new List<T>(combo));
            combo.Add(list[0]);
            result.Add(new List<T>(combo));
        });
        return result;
    }

    public static void SetCollisionForTeam(GameObject go, Team team, bool only = false)
    {
        switch (team)
        {
            case Team.Player:
                go.layer = only ? LayerMask.NameToLayer("EnemyOnly") : LayerMask.NameToLayer("Projectile");
                break;
            case Team.DeathMatch0:
            case Team.DeathMatch1:
            case Team.DeathMatch2:
            case Team.DeathMatch3:
                go.layer = only ? LayerMask.NameToLayer("CreatureOnly") : LayerMask.NameToLayer("Projectile");
                break;
            case Team.Enemy:
                go.layer = only ? LayerMask.NameToLayer("PlayerOnly") : LayerMask.NameToLayer("EnemyProjectile");
                break;
            case Team.None:
                go.layer = LayerMask.NameToLayer("Default");
                break;
        }
    }

    public static void SetCollisionForTeam(Collider2D collider2D, Team team, bool only = false)
    {
        switch (team)
        {
            case Team.Player:
                collider2D.gameObject.layer = only ? LayerMask.NameToLayer("EnemyOnly") : LayerMask.NameToLayer("Projectile");
                break;
            case Team.DeathMatch0:
            case Team.DeathMatch1:
            case Team.DeathMatch2:
            case Team.DeathMatch3:
                collider2D.gameObject.layer = only ? LayerMask.NameToLayer("CreatureOnly") : LayerMask.NameToLayer("Projectile");
                break;
            case Team.Enemy:
                collider2D.gameObject.layer = only ? LayerMask.NameToLayer("PlayerOnly") : LayerMask.NameToLayer("EnemyProjectile");
                break;
            case Team.None:
                collider2D.gameObject.layer = LayerMask.NameToLayer("Default");
                break;
        }

        if (team >= Team.DeathMatch0 && team <= Team.DeathMatch3)
        {
            if (DeathmatchManager.instance)
            {
                DeathmatchManager.instance.SetCollisionIgnore(team, collider2D);
            }
            else
            {
                Debug.LogError("A Projectile is set to a deathmatch team and there's no DeathmatchManager");
            }
        }
    }

    public static string[] prefixes = new string[] 
    {
        "Slippery", "Sly", "Blind", "Hairy", "Lucky", "Garish", "Brave", "Fat", "Lonesome", "Wild", "Handsome", "Erstwhile",
        "Rabid", "Sir", "Angry", "Grandpa", "Lil'", "Shaking", "Ole", "Dirty", "Hot", "Big", "Strange", "Mighty", "Proud",
        "Scroungy", "Smilin'", "Whistlin'", "Quick Gun", "Large", "Father", "Very Wild", "Smelly", "Six Finger", "Plucky",
        "Crazy", "Scarless", "Impulsive", "Dank", "Oft-Late", "Always-Humming", "The", "Not-so Lonesome", "Father", "Howlin'",
        "Fearful", "Immaculate", "Greasy", "Big", "Very Large", "Gargantuan", "Small", "Smoll", "Stupid", "Very Intelligent", "Long",
        "Guilty", "Slick", "Regrettable", "Nervous", "Ample", "Distubingly Smooth", "Pale", "Thrifty", "Gritty", "Weird Uncle", "Sad",
        "Gangrenous", "Bratty", "Smelly", "Innocent", "Grumpy", "Hopeful", "Doctor", "Cranky", "Unsavory",
    };

    public static string[] names = new string[] 
    {
        "Ronald", "JoJo", "Mortimer", "Cromwell", "Gary", "Rebecca", "Grom", "Lucky", "Garol", "Seb", "Tony", "Bill", "Shannon",
        "Fritz", "Joe Stink", "Lamar", "Mr. Donovan", "Shelia", "Curtis", "Vert", "Dirty", "Harold", "Daniel", "Willy", "Mildred",
        "Frances", "Marge", "Doris", "Kevin", "Jimmy", "Richard", "Milo", "Earl", "Sally", "Bobo", "Krunk", "Grok", "Wilbert", "Charlotte",
        "Rob", "Craig", "Greg", "Nicki", "Deb", "Emil", "Donald", "Davey", "Who Builds", "Shrub", "John", "Tobias", "Mendelson", "Tiff",
        "Barney", "Wizard", "Limble", "Kimble", "Jimble", "Jam", "B", "Drizzle", "Gargle", "Jimothy", "Staniel", "Bart", "Monty", "Hans",
        "Nom Took", "Charlie", "Chad", "Jenny", "Lisa", "Kate", "Aldous", "Camilia", "Noam", "Tim",
    };

    public static string[] suffixes = new string[] 
    {
        "The Sneak", "The Cromulent", "III", "The Loser", "The Negotiator", "Pro Digger", ", A Handsom Lad", "No Teeth", "The Predator",
        "Who Howls", "Six-Pack", "The Crank", "The Four Clawed", ", A Gentleman", "Two Teeth", ", Feared By All", "The Diplomat", "The Brave",
        "The Favorite", "Barrel Sleeper", "The Louse", "Snapper", "The Arsonist", "Who Must Not Be Named", "Who Is Not To Be Trusted", "Misty",
        "Bigman", "The Cranky", "Who Is Always Tired", "The Barron", "Cobblesnot", "The Long", "The Extended", "The Obstinant", "The Nimble", "The Jammy", "Jr.",
        "The Best Moleman", "The Peacebringer", "The Cure Finder",
    };

    public static string GetMolemanName()
    {
        var name = string.Empty;

        var prefix = UnityEngine.Random.value > 0.5f;

        if (prefix)
        {
            name += prefixes[UnityEngine.Random.Range(0,prefixes.Length)] + " ";
        }

        name += names[UnityEngine.Random.Range(0, names.Length)];

        if (!prefix)
        {
            var suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
            if (suffix[0] == ',')
            {
                name += suffix;
            }
            else
            {
                name += " " + suffix;
            }
        }

        return name;
    }
}
