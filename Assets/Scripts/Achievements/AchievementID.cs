using System.Collections.Generic;

public enum AchievementID
{
    None = 0,
    BuzzOrb = 1, //Discover the Buried City
    BigBolt = 2, //Defeat Abomination
    RailGun = 3, //Discover The Factory
    TripleShot = 4, //Defeat WallCreep
    PowerJump = 5, //Defeat Beaklord
    LightningGun = 6, //Defeat Mouth Meat Senior
    BrightShell = 7, //Discover The Caves
    LaserBeam = 8, //Defeat Metal Patriarch
    HunterKiller = 9, //Defeat Flesh Adder
    RoyalOrb = 10,
    HomingBolt = 11,
    MegaDamage = 12, //Defeat the MegaBeast 1 time
    MegaHealth = 13, //Defeat the MegaBeast 2 times
    MegaEnergy = 14, //Defeat the MegaBeast 3 times
    MegaAttack = 15, //Defeat the MegaBeast 4 times
    MegaSpeed = 16, //Defeat the MegaBeast 5 times
    BeastGuts = 17, //Defeat the MegaBeast 3 times
    HoverBoots = 18, //Defeat Cephalodiptera
    AutoTurret = 19, //Defeat Sluggard
    RegenerationHelm = 20,
    AspectShell = 21,
    ViridianShell = 22, //Defeat White Wyrm
    RevenantStation = 23, //Die Once
    OrbBot = 24, //Collect 4 Orbs (Chance to start with a Follower)
    GunBot = 25, //Collect 4 Energy Weapons (Chance to start with an Energy Weapon)
    ThoroughBot = 26, //100% (Chance to start with an Minor Item)
    FastBot = 27, //Speed Run (Chance to start with an Activated Item)
    FightBot = 28, //100% Speed Run (Chance to start with a Passive Item)
    TriOrb = 29, //Defeat Mouth Meat
    TheFleshening = 30, //Defeat the MegaBeast 1 time
    TheFlesheningII = 31, //Defeat the MegaBeast 2 time
    MasterMap = 32, //Uncover the entire map
    ArtificeHelm = 33, //Possess 50 scrap
    GlitchModule = 34, //Discover Glitch World
    GlitchMap = 35, //Collect The Blue Key
    GlitchShell = 36, //Collect The Red Key
    TheGlitchedKey = 37, //Collect all the keys
    ModuleTransmogrifier = 38, //Collect The Green Key
    CognitiveStabilizer = 39, //Defeat Tutorial Smith
    TheThief = 40, // Collect The Black Key
    TheQuickening = 41, //Complete the game with a completion rate less than 15%
    MegaShotSpeed = 42, //Defeat the MegaBeast 6 times
    TheTraitor = 43, //Spend 3 of each scrap
    EnergyVorb = 44, //Kill 100 Enemies
    ScrapVorb = 45, //Kill 1000 Enemies
    HealthVorb = 46, // Kill 10000 Enemies
    AllyBot = 47, //Empty 3 Shops - Shops always stock 3 items
    LilTyr = 48, //Donate 27 Gray Scrap
    LilZurvan = 49,  //Donate 125 Gray Scrap
    LilPhaestus = 50, //Donate 216 Gray Scrap
    LilWadjet = 51, //Donate 343 Gray Scrap
    LilBuluc = 52, //Donate 512 Gray Scrap
    LilOrphy = 53, //Donate 729 Gray Scrap
    BuzzsawShell = 54, //Have max speed of 17 or higher
    PowerShield = 55, //Have max health of 20 or higher
    Phaserang = 56, //Have shot speed of 22 or higher
    PhantasmalOrbs = 57, //Have 18 or more nanobots
    ForestSlums = 58, //Defeat all surface bosses
    NecroluminantSpray = 59, //Defeat Ooze Hart
    HiveHelm = 60, //Defeat Blightbark
    PersonalTeleporter = 61, //Discover all teleporters in a normal run
    TyrsHorns = 62, //Kill 20 Champions
    CoolantSewers = 63, //Defeat all cave bosses
    DiveShell = 64, //Discover the Coolant Sewers
    UpDog = 65, //Defeat Leviathan
    GlitchScrap = 66, //"Open the Door"
    Lasorb = 67, //Defeat Stalkus
    ScrapCache = 68, //Possess 99 Scrap
    ArtificeBeam = 69, //Possess 3 of each archaic scrap
    SwellBolt = 70, //Fire a shot with size 4 or more
    CrystalMines = 71, //Defeat all Factory bosses
    HazardShell = 72, //Defeat Mine Corrupter
    CrystalShell = 73, //Discover Crystal Caves
    WallJump = 74, //Sequence Break
    OrphielsAltar = 75, //Possess 81 Nanobots
    PhaseShell = 76, //Defeat Skin Deviler
    Kaboomerang = 77, //Have Explosive Bolts that Deal 8 Damage
    HellfireCannon = 78, //Have Fire Bolts and < 0.2 attack delay
    TrollHelm = 79, //Complete Boss Rush
    ToxinOrb = 80, //Complete Exterminator
    EmpireHelm = 81, //Complete Mega Map

    //GameModes
    BossRush = 201, //Defeat 14 Different Bosses
    Exterminator = 202, //Kill 30k enemies
    MegaMap = 203, //Discover Every Area
}

public partial class AchievementManager
{
    public static Dictionary<AchievementID, AchievementInfo> achievements = new Dictionary<AchievementID, AchievementInfo>
    {
        {AchievementID.AspectShell,
            new AchievementInfo
            {
                achievementID = AchievementID.AspectShell,
                name = "Aspect Shell",
                associatedItem = MajorItem.AspectShell,
                associatedBoss = BossName.MegaBeastCore,
                collectMeans = "Defeat the Megabeast Core",
            }
        },

        {AchievementID.AutoTurret,
            new AchievementInfo
            {
                achievementID = AchievementID.AutoTurret,
                name = "Auto Turret",
                associatedItem = MajorItem.AutoTurret,
                associatedBoss = BossName.Sluggard,
                collectMeans = "Defeat the Sluggard",
            }
        },

        {AchievementID.BrightShell,
            new AchievementInfo
            {
                achievementID = AchievementID.BrightShell,
                name = "Bright Shell",
                associatedItem = MajorItem.BrightShell,
                collectMeans = "Discover The Caves",
            }
        },

        {AchievementID.BuzzOrb,
            new AchievementInfo
            {
                achievementID = AchievementID.BuzzOrb,
                name = "Buzz Orb",
                associatedItem = MajorItem.BuzzOrb,
                collectMeans = "Discover The Buried City",
            }
        },

        {AchievementID.RailGun,
            new AchievementInfo
            {
                achievementID = AchievementID.RailGun,
                name = "Rail Gun",
                associatedItem = MajorItem.RailGun,
                collectMeans = "Discover The Factory",
            }
        },

        {AchievementID.BigBolt,
            new AchievementInfo
            {
                achievementID = AchievementID.BigBolt,
                name = "Big Bolt",
                associatedItem = MajorItem.BigBolt,
                associatedBoss = BossName.Abomination,
                collectMeans = "Defeat Abomination",
            }
        },

        {AchievementID.PowerJump,
            new AchievementInfo
            {
                achievementID = AchievementID.PowerJump,
                name = "Power Jump",
                associatedItem = MajorItem.PowerJump,
                associatedBoss = BossName.BeakLord,
                collectMeans = "Defeat Beak Lord",
            }
        },

        {AchievementID.HomingBolt,
            new AchievementInfo
            {
                achievementID = AchievementID.HomingBolt,
                name = "Homing Bolt",
                associatedItem = MajorItem.HomingBolt,
                associatedBoss = BossName.MolemanShaman,
                collectMeans = "Defeat Moleman Shaman",
            }
        },

        {AchievementID.HunterKiller,
            new AchievementInfo
            {
                achievementID = AchievementID.HunterKiller,
                name = "Hunter Killer",
                associatedItem = MajorItem.HunterKiller,
                associatedBoss = BossName.FleshAdder,
                collectMeans = "Defeat Flesh Adder",
            }
        },

        {AchievementID.HoverBoots,
            new AchievementInfo
            {
                achievementID = AchievementID.HoverBoots,
                name = "Hover Boots",
                associatedItem = MajorItem.HoverBoots,
                associatedBoss = BossName.Cephalodiptera,
                collectMeans = "Defeat Cephalodiptera",
            }
        },

        {AchievementID.LaserBeam,
            new AchievementInfo
            {
                achievementID = AchievementID.LaserBeam,
                name = "Laser Beam",
                associatedItem = MajorItem.LaserBeam,
                associatedBoss = BossName.MetalPatriarch,
                collectMeans = "Defeat Metal Patriarch",
            }
        },

        {AchievementID.LightningGun,
            new AchievementInfo
            {
                achievementID = AchievementID.LightningGun,
                name = "Lightning Gun",
                associatedItem = MajorItem.LightningGun,
                associatedBoss = BossName.MouthMeatSenior,
                collectMeans = "Defeat Mouth Meat Senior",
            }
        },

        {AchievementID.RegenerationHelm,
            new AchievementInfo
            {
                achievementID = AchievementID.RegenerationHelm,
                name = "Regeneration Helm",
                associatedItem = MajorItem.RegenerationHelm,
                collectMeans = "Discover The Beast's Guts",
            }
        },

        {AchievementID.RoyalOrb,
            new AchievementInfo
            {
                achievementID = AchievementID.RoyalOrb,
                name = "Royal Orb",
                associatedItem = MajorItem.RoyalOrb,
                associatedBoss = BossName.Polyphemus,
                collectMeans = "Defeat Polyphemus",
            }
        },

        {AchievementID.TriOrb,
            new AchievementInfo
            {
                achievementID = AchievementID.TriOrb,
                name = "Tri Orb",
                associatedItem = MajorItem.TriOrb,
                associatedBoss = BossName.MouthMeat,
                collectMeans = "Defeat Mouth Meat",
            }
        },

        {AchievementID.TripleShot,
            new AchievementInfo
            {
                achievementID = AchievementID.TripleShot,
                name = "Triple Shot",
                associatedItem = MajorItem.TripleShot,
                associatedBoss = BossName.WallCreep,
                collectMeans = "Defeat Wall Creep",
            }
        },

        {AchievementID.MegaDamage,
            new AchievementInfo
            {
                achievementID = AchievementID.MegaDamage,
                name = "Mega Damage",
                associatedItem = MajorItem.MegaDamage,
                collectMeans = "Defeat the Megabeast once",
            }
        },

        {AchievementID.MegaHealth,
            new AchievementInfo
            {
                achievementID = AchievementID.MegaHealth,
                name = "Mega Health",
                associatedItem = MajorItem.MegaHealth,
                collectMeans = "Defeat the Megabeast twice",
            }
        },

        {AchievementID.MegaEnergy,
            new AchievementInfo
            {
                achievementID = AchievementID.MegaEnergy,
                name = "Mega Energy",
                associatedItem = MajorItem.MegaEnergy,
                collectMeans = "Defeat the Megabeast thrice",
            }
        },

        {AchievementID.MegaAttack,
            new AchievementInfo
            {
                achievementID = AchievementID.MegaAttack,
                name = "Mega Attack",
                associatedItem = MajorItem.MegaAttack,
                collectMeans = "Defeat the Megabeast four times",
            }
        },

        {AchievementID.MegaSpeed,
            new AchievementInfo
            {
                achievementID = AchievementID.MegaSpeed,
                name = "Mega Speed",
                associatedItem = MajorItem.MegaSpeed,
                collectMeans = "Defeat the Megabeast five times",
            }
        },


        {AchievementID.ViridianShell,
            new AchievementInfo
            {
                achievementID = AchievementID.ViridianShell,
                name = "Viridian Shell",
                associatedItem = MajorItem.ViridianShell,
                associatedBoss = BossName.WhiteWyrm,
                collectMeans = "Defeat White Wyrm",
            }
        },

        {AchievementID.BeastGuts,
            new AchievementInfo
            {
                achievementID = AchievementID.BeastGuts,
                name = "The guts of the beast",
                associatedItem = MajorItem.None,
                description = "The guts of the beast have been exposed",
                collectMeans = "Defeat the Megabeast thrice",
            }
        },


        {AchievementID.RevenantStation,
            new AchievementInfo
            {
                achievementID = AchievementID.RevenantStation,
                name = "Revenant Station",
                associatedItem = MajorItem.None,
                collectMeans = "Die",
            }
        },

        {AchievementID.OrbBot,
            new AchievementInfo
            {
                achievementID = AchievementID.OrbBot,
                name = "Orb Bot",
                description = "A chance to start with a follower has been unlocked",
                associatedItem = MajorItem.None,
                collectMeans = "Collect 4 orbs in a single run",
            }
        },

        {AchievementID.GunBot,
            new AchievementInfo
            {
                achievementID = AchievementID.GunBot,
                name = "Gun Bot",
                description = "A chance to start with an energy weapon has been unlocked",
                associatedItem = MajorItem.None,
                collectMeans = "Collect 3 energy weapons in a single run",
            }
        },

        {AchievementID.ThoroughBot,
            new AchievementInfo
            {
                achievementID = AchievementID.ThoroughBot,
                name = "Thorough Bot",
                description = "A chance to start with a minor item has been unlocked",
                associatedItem = MajorItem.None,
                collectMeans = "Collect 100% of the items in a run",
            }
        },

        {AchievementID.FastBot,
            new AchievementInfo
            {
                achievementID = AchievementID.FastBot,
                name = "Fast Bot",
                description = "A chance to start with an activated item has been unlocked",
                associatedItem = MajorItem.None,
                collectMeans = "Defeat the Megabeast in under 40 minutes in a normal run",
            }
        },

        {AchievementID.FightBot,
            new AchievementInfo
            {
                achievementID = AchievementID.FightBot,
                name = "Fight Bot",
                description = "A chance to start with a passive item has been unlocked",
                associatedItem = MajorItem.None,
                collectMeans = "Defeat the Megabeast Core in under 60 minutes and collect 100% of the items in the same run",
            }
        },

        {AchievementID.TheFleshening,
            new AchievementInfo
            {
                achievementID = AchievementID.TheFleshening,
                name = "The Fleshening",
                description = "Flesh Grows Everywhere",
                associatedItem = MajorItem.None,
                collectMeans = "Defeat the Megabeast",
            }
        },

        {AchievementID.TheFlesheningII,
            new AchievementInfo
            {
                achievementID = AchievementID.TheFlesheningII,
                name = "The Fleshening II",
                description = "Even More Flesh!",
                associatedItem = MajorItem.None,
                collectMeans = "Defeat the Megabeast twice",
            }
        },

        {AchievementID.MasterMap,
            new AchievementInfo
            {
                achievementID = AchievementID.MasterMap,
                name = "Master Map",
                associatedItem = MajorItem.MasterMap,
                collectMeans = "Discover every map square in a run",
            }
        },

        {AchievementID.ArtificeHelm,
            new AchievementInfo
            {
                achievementID = AchievementID.ArtificeHelm,
                name = "Artifice Helm",
                associatedItem = MajorItem.ArtificeHelm,
                collectMeans = "Possess 50 scrap",
            }
        },

        {AchievementID.GlitchModule,
            new AchievementInfo
            {
                achievementID = AchievementID.GlitchModule,
                name = "Glitch Module",
                description = "It is forbidden",
                hidden = true,
                collectMeans = "Discover a forbidden place",
            }
        },

        {AchievementID.GlitchMap,
            new AchievementInfo
            {
                achievementID = AchievementID.GlitchMap,
                name = "Glitch Map",
                associatedItem = MajorItem.GlitchMap,
                hidden = true,
                collectMeans = "Collect The Blue Key",
            }
        },

        {AchievementID.GlitchShell,
            new AchievementInfo
            {
                achievementID = AchievementID.GlitchShell,
                name = "Glitch Shell",
                associatedItem = MajorItem.GlitchShell,
                hidden = true,
                collectMeans = "Collect The Red Key",
            }
        },

        {AchievementID.TheGlitchedKey,
            new AchievementInfo
            {
                achievementID = AchievementID.TheGlitchedKey,
                name = "The Glitched Key",
                associatedItem = MajorItem.TheGlitchedKey,
                hidden = true,
                collectMeans = "Collect all of the Keys",
            }
        },

         {AchievementID.ModuleTransmogrifier,
            new AchievementInfo
            {
                achievementID = AchievementID.ModuleTransmogrifier,
                name = "Module Transmogrifier",
                associatedItem = MajorItem.ModuleTransmogrifier,
                hidden = true,
                collectMeans = "Collect The Green Key",
            }
        },

        {AchievementID.CognitiveStabilizer,
            new AchievementInfo
            {
                achievementID = AchievementID.CognitiveStabilizer,
                name = "Cognitive Stabilizer",
                associatedItem = MajorItem.CognitiveStabilizer,
                hidden = true,
                collectMeans = "Defeat Tutorial Smith",
            }
        },

        {AchievementID.TheThief,
            new AchievementInfo
            {
                achievementID = AchievementID.TheThief,
                name = "The Thief",
                associatedItem = MajorItem.TheThief,
                hidden = true,
                collectMeans = "Collect The Black Key",
            }
        },

        {AchievementID.TheQuickening,
            new AchievementInfo
            {
                achievementID = AchievementID.TheQuickening,
                name = "The Quickening",
                description = "Higher Chance For Starting Items",
                collectMeans = "Complete a run with a collection rate less than 15%",
            }
        },

        {AchievementID.MegaShotSpeed,
            new AchievementInfo
            {
                achievementID = AchievementID.MegaShotSpeed,
                name = "Mega Shot Speed",
                associatedItem = MajorItem.MegaShotSpeed,
                collectMeans = "Defeat the Megabeast six times",
            }
        },

        {AchievementID.TheTraitor,
            new AchievementInfo
            {
                achievementID = AchievementID.TheTraitor,
                name = "The Traitor",
                description = "A Morally Flexible Moleman",
                collectMeans = "Trade in 3 of each type of Archaic Scrap to allies"
            }
        },

        {AchievementID.EnergyVorb,
            new AchievementInfo
            {
                achievementID = AchievementID.EnergyVorb,
                name = "Energy Vorb",
                associatedItem = MajorItem.EnergyVorb,
                collectMeans = "Kill 100 Enemies",
            }
        },

        {AchievementID.ScrapVorb,
            new AchievementInfo
            {
                achievementID = AchievementID.ScrapVorb,
                name = "Scrap Vorb",
                associatedItem = MajorItem.ScrapVorb,
                collectMeans = "Kill 1000 Enemies",
            }
        },

        {AchievementID.HealthVorb,
            new AchievementInfo
            {
                achievementID = AchievementID.HealthVorb,
                name = "Health Vorb",
                associatedItem = MajorItem.HealthVorb,
                collectMeans = "Kill 7500 Enemies",
            }
        },

        {AchievementID.AllyBot,
            new AchievementInfo
            {
                achievementID = AchievementID.AllyBot,
                name = "Ally Bot",
                description = "In future runs, allies always carry 3 items",
                collectMeans = "Empty Out 3 Allies",
            }
        },

        {AchievementID.LilTyr,
            new AchievementInfo
            {
                achievementID = AchievementID.LilTyr,
                name = "Lil' Tyr",
                associatedItem = MajorItem.LilTyr,
                collectMeans = "Donate 27 gray scrap to shrines",
            }
        },

        {AchievementID.LilZurvan,
            new AchievementInfo
            {
                achievementID = AchievementID.LilZurvan,
                name = "Lil' Zurvan",
                associatedItem = MajorItem.LilZurvan,
                collectMeans = "Donate 125 gray scrap to shrines",
            }
        },

        {AchievementID.LilPhaestus,
            new AchievementInfo
            {
                achievementID = AchievementID.LilPhaestus,
                name = "Lil' Phaestus",
                associatedItem = MajorItem.LilPhaestus,
                collectMeans = "Donate 216 gray scrap to shrines",
            }
        },

        {AchievementID.LilWadjet,
            new AchievementInfo
            {
                achievementID = AchievementID.LilWadjet,
                name = "Lil' Wadjet",
                associatedItem = MajorItem.LilWadjet,
                collectMeans = "Donate 343 gray scrap to shrines",
            }
        },

        {AchievementID.LilBuluc,
            new AchievementInfo
            {
                achievementID = AchievementID.LilBuluc,
                name = "Lil' Buluc",
                associatedItem = MajorItem.LilBuluc,
                collectMeans = "Donate 512 gray scrap to shrines",
            }
        },

        {AchievementID.LilOrphy,
            new AchievementInfo
            {
                achievementID = AchievementID.LilOrphy,
                name = "Lil' Orphy",
                associatedItem = MajorItem.LilOrphy,
                collectMeans = "Donate 729 gray scrap to shrines",
            }
        },

        {AchievementID.BuzzsawShell,
            new AchievementInfo
            {
                achievementID = AchievementID.BuzzsawShell,
                name = "Buzzsaw Shell",
                associatedItem = MajorItem.BuzzsawShell,
                collectMeans = "Have a max speed of 17 or higher",
            }
        },

        {AchievementID.PowerShield,
                new AchievementInfo
            {
                achievementID = AchievementID.PowerShield,
                name = "Power Shield",
                associatedItem = MajorItem.PowerShield,
                collectMeans = "Have a max health of 20 or higher",
            }
        },

        {AchievementID.Phaserang,
            new AchievementInfo
            {
                achievementID = AchievementID.Phaserang,
                name = "Phaserang",
                associatedItem = MajorItem.Phaserang,
                collectMeans = "Have a shot speed of 22 or higher",
            }
        },

        {AchievementID.PhantasmalOrbs,
            new AchievementInfo
            {
                achievementID = AchievementID.PhantasmalOrbs,
                name = "Phantasmal Orbs",
                associatedItem = MajorItem.PhantasmalOrbs,
                collectMeans = "Have 18 or more nanobots at once",
            }
        },

        {AchievementID.ForestSlums,
            new AchievementInfo
            {
                achievementID = AchievementID.ForestSlums,
                name = "The Forest Slums",
                associatedItem = MajorItem.None,
                description = "The Forest Slums have been discovered",
                collectMeans = "Defeat all Surface City bosses",
            }
        },

        {AchievementID.NecroluminantSpray,
            new AchievementInfo
            {
                achievementID = AchievementID.NecroluminantSpray,
                name = "Necroluminant Spray",
                associatedItem = MajorItem.NecroluminantSpray,
                associatedBoss = BossName.OozeHart,
                collectMeans = "Defeat Ooze Hart",
            }
        },

        {AchievementID.HiveHelm,
            new AchievementInfo
            {
                achievementID = AchievementID.HiveHelm,
                name = "Hive Helm",
                associatedItem = MajorItem.HiveHelm,
                associatedBoss = BossName.Blightbark,
                collectMeans = "Defeat Blightbark",
            }
        },

        {AchievementID.PersonalTeleporter,
            new AchievementInfo
            {
                achievementID = AchievementID.PersonalTeleporter,
                name = "Personal Teleporter",
                associatedItem = MajorItem.PersonalTeleporter,
                collectMeans = "Discover Every Teleporter in a Normal Run",
            }
        },

        {AchievementID.TyrsHorns,
            new AchievementInfo
            {
                achievementID = AchievementID.TyrsHorns,
                name = "Tyr's Horns",
                associatedItem = MajorItem.TyrsHorns,
                collectMeans = "Kill 20 Champions",
            }
        },

        {AchievementID.CoolantSewers,
            new AchievementInfo
            {
                achievementID = AchievementID.CoolantSewers,
                name = "The Coolant Sewers",
                associatedItem = MajorItem.None,
                description = "The Coolant Sewers have been discovered",
                collectMeans = "Defeat all Cave bosses",
            }
        },

        {AchievementID.DiveShell,
            new AchievementInfo
            {
                achievementID = AchievementID.DiveShell,
                name = "Dive Shell",
                associatedItem = MajorItem.DiveShell,
                collectMeans = "Discover the Coolant Sewers",
            }
        },

        {AchievementID.UpDog,
            new AchievementInfo
            {
                achievementID = AchievementID.UpDog,
                name = "Up Dog",
                associatedItem = MajorItem.UpDog,
                associatedBoss = BossName.Leviathan,
                collectMeans = "Defeat Leviathan",
            }
        },

        {AchievementID.Lasorb,
            new AchievementInfo
            {
                achievementID = AchievementID.Lasorb,
                name = "Lasorb",
                associatedItem = MajorItem.Lasorb,
                associatedBoss = BossName.Stalkus,
                collectMeans = "Defeat Stalkus",
            }
        },

        {AchievementID.GlitchScrap,
            new AchievementInfo
            {
                achievementID = AchievementID.GlitchScrap,
                name = "Glitch Scrap",
                hidden = true,
                associatedItem = MajorItem.None,
                description = "Some scrap is unstable. Choose wisely.",
                collectMeans = "Open the Door",
            }
        },

        {AchievementID.ScrapCache,
            new AchievementInfo
            {
                achievementID = AchievementID.ScrapCache,
                name = "Scrap Cache",
                associatedItem = MajorItem.ScrapCache,
                collectMeans = "Possess 99 Scrap",
            }
        },

        {AchievementID.ArtificeBeam,
            new AchievementInfo
            {
                achievementID = AchievementID.ArtificeBeam,
                name = "Artifice Beam",
                associatedItem = MajorItem.ArtificeBeam,
                collectMeans = "Possess 3 of Each Archaic Scrap",
            }
        },

        {AchievementID.SwellBolt,
            new AchievementInfo
            {
                achievementID = AchievementID.SwellBolt,
                name = "Swell Bolt",
                associatedItem = MajorItem.SwellBolt,
                collectMeans = "Fire a shot size of 4 or greater",
            }
        },

        {AchievementID.CrystalMines,
            new AchievementInfo
            {
                achievementID = AchievementID.CrystalMines,
                name = "The Crystal Mines",
                associatedItem = MajorItem.None,
                description = "The Crystal Mines have been discovered",
                collectMeans = "Defeat all Factory bosses",
            }
        },

        {AchievementID.HazardShell,
            new AchievementInfo
            {
                achievementID = AchievementID.HazardShell,
                name = "Hazard Shell",
                associatedItem = MajorItem.HazardShell,
                associatedBoss = BossName.CorruptedMiner,
                collectMeans = "Defeat Corrupted Miner",
            }
        },

        {AchievementID.CrystalShell,
            new AchievementInfo
            {
                achievementID = AchievementID.CrystalShell,
                name = "Crystal Shell",
                associatedItem = MajorItem.CrystalShell,
                collectMeans = "Discover the Crystal Mines",
            }
        },

        {AchievementID.WallJump,
            new AchievementInfo
            {
                achievementID = AchievementID.WallJump,
                name = "Wall Jump",
                associatedItem = MajorItem.None,
                description = "You shouldn't be here yet...",
                collectMeans = "Sequence Break",
                hidden = true,
            }
        },

        {AchievementID.OrphielsAltar,
            new AchievementInfo
            {
                achievementID = AchievementID.OrphielsAltar,
                name = "Orphiel's Altar",
                associatedItem = MajorItem.OrphielsAltar,
                collectMeans = "Possess 81 Nanobots",
            }
        },

        {AchievementID.PhaseShell,
            new AchievementInfo
            {
                achievementID = AchievementID.PhaseShell,
                name = "Phase Shell",
                associatedItem = MajorItem.PhaseShell,
                associatedBoss = BossName.SkinDeviler,
                collectMeans = "Defeat Skin Deviler",
            }
        },

        {AchievementID.Kaboomerang,
            new AchievementInfo
            {
                achievementID = AchievementID.Kaboomerang,
                name = "Kaboomerang",
                associatedItem = MajorItem.Kaboomerang,
                collectMeans = "Have Explosive Bolts that Deal 8 or more Damage",
            }
        },

        {AchievementID.HellfireCannon,
            new AchievementInfo
            {
                achievementID = AchievementID.HellfireCannon,
                name = "Hellfire Cannon",
                associatedItem = MajorItem.HellfireCannon,
                collectMeans = "Have Fire Bolts with an attack delay less than 0.2",
            }
        },

        {AchievementID.TrollHelm,
            new AchievementInfo
            {
                achievementID = AchievementID.TrollHelm,
                name = "Troll Helm",
                associatedItem = MajorItem.TrollHelm,
                collectMeans = "Complete Boss Rush Mission",
            }
        },

        {AchievementID.ToxinOrb,
            new AchievementInfo
            {
                achievementID = AchievementID.ToxinOrb,
                name = "Toxin Orb",
                associatedItem = MajorItem.ToxinOrb,
                collectMeans = "Complete Exterminator Mission",
            }
        },

        {AchievementID.EmpireHelm,
            new AchievementInfo
            {
                achievementID = AchievementID.EmpireHelm,
                name = "Empire Helm",
                associatedItem = MajorItem.EmpireHelm,
                collectMeans = "Complete Mega Map Mission",
            }
        },

        {AchievementID.BossRush,
            new AchievementInfo
            {
                achievementID = AchievementID.BossRush,
                name = "Boss Rush",
                description = "A New Challenge Has Been Unlocked!",
                collectMeans = "Defeat 12 Different Bosses",
            }
        },

        {AchievementID.Exterminator,
            new AchievementInfo
            {
                achievementID = AchievementID.Exterminator,
                name = "Exterminator",
                description = "A New Challenge Has Been Unlocked!",
                collectMeans = "Kill 15000 Enemies",
            }
        },

        {AchievementID.MegaMap,
            new AchievementInfo
            {
                achievementID = AchievementID.MegaMap,
                name = "Mega Map",
                description = "A New Challenge Has Been Unlocked!",
                collectMeans = "Discover Every Environment",
            }
        },
    };
}
