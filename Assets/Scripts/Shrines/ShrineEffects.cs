using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrineEffects
{
    //public float weight = 1f;
    public Func<Player, Outcome> outcome;
    public Func<Player, string> immediateEffect;
    public List<MajorItem> possibleItemRewards;
    public Action<Player> teleportationEffect; //for teleporting and such

    public static ShrineEffects fullEnergy = new ShrineEffects
    {
        outcome = (p) => { return Outcome.Positive; },
        immediateEffect = (p) =>
        {
            p.GainEnergy(p.maxEnergy);
            return "I grant you full energy, Fight Smith " + SaveGameManager.fightNumber + "!";
        },
    };

    public static ShrineEffects fullHealth = new ShrineEffects
    {
        outcome = (p) => { return Outcome.Positive; },
        immediateEffect = (p) =>
        {
            p.health = p.maxHealth;
            return "I grant you full health, Fight Smith " + SaveGameManager.fightNumber + "!";
        },
    };

    public static ShrineEffects glitchWorld = new ShrineEffects
    {
        outcome = (p) => { return Outcome.Negative; },

        immediateEffect = (p) =>
        {
            return "Followers of the forbidden are banished to a forbidden place!";
        },

        teleportationEffect = (p) =>
        {
            if (AchievementManager.instance)
            {
                AchievementManager.instance.WaitTryEarnAchievement(1f, AchievementID.GlitchScrap);
            }

            if (LayoutManager.instance)
            {
                LayoutManager.instance.GlitchToEnvironmentStart(EnvironmentType.Glitch);
            }
        }
    };

    public static ShrineEffects ModifyStat(int amount, MinorItemType type)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return amount > 0 ? Outcome.Positive : Outcome.Negative; },
            immediateEffect = (p) =>
            {
                p.ModifyMinorItemCollection(type, amount, true);
                var amountString = AmountString(amount);
                return (string.IsNullOrEmpty(amountString) ? "" : amountString + " ") + (amount > 0 ? StatBlessingText(type) : StatCurseText(type));
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects ModifyRandomStat(int amount)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return amount > 0 ? Outcome.Positive : Outcome.Negative; },
            immediateEffect = (player) =>
            {
                var type = Constants.allStatModules[Random.Range(0, Constants.allStatModules.Length)];
                player.ModifyMinorItemCollection(type, amount, true);
                var amountString = AmountString(amount);
                return (string.IsNullOrEmpty(amountString) ? "" : amountString + " ") + (amount > 0 ? StatBlessingText(type) : StatCurseText(type));
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects AddTempMod(PlayerStatType statType, float rank)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p)=> { return rank > 0 ? Outcome.Positive : Outcome.Negative; },
            immediateEffect = (p) =>
            {
                p.AddTempStatMod(statType, rank);
                return "A temporary " + (rank > 0 ? "boon to " : "curse upon ") + StatNames(statType);
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects AddTempRegen(float rate, float duration)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return Outcome.Positive; },
            immediateEffect = (p) =>
            {
                var statMod = p.gameObject.AddComponent<TemporaryRegeneration>();
                statMod.Equip(p, duration * p.blessingTimeMod, rate);
                return "May your health return to you quickly";
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects AddTempEnergyRegen(float rate, float duration)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return Outcome.Positive; },
            immediateEffect = (p) =>
            {
                var statMod = p.gameObject.AddComponent<TemporaryEnergyRegen>();
                statMod.Equip(p, duration * p.blessingTimeMod, rate);
                return "May your energy return to you quickly";
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects AddTempSheild(float duration)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return Outcome.Positive; },
            immediateEffect = (p) =>
            {
                var statMod = p.gameObject.AddComponent<TemporaryShield>();
                statMod.Equip(p, duration * p.blessingTimeMod);
                return "Go forth impenetrable.";
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects AddTempSheildOrbs(float duration)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return Outcome.Positive; },
            immediateEffect = (p) =>
            {
                var statMod = p.gameObject.AddComponent<TemporaryShieldOrbs>();
                statMod.Equip(p, duration * p.blessingTimeMod);
                return "My children will protect you for a while.";
            }
        };
        return shrineEffect;
    }


    public static ShrineEffects AddTempCelestialCharge(float duration)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return Outcome.Positive; },
            immediateEffect = (p) =>
            {
                var statMod = p.gameObject.AddComponent<TemporaryCelestialCharge>();
                statMod.Equip(p, duration * p.blessingTimeMod);
                return "Go forth unstoppable.";
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects SpawnNanobots(int amount)
    {
        var shrineEffect = new ShrineEffects()
        {
            outcome = (p) => { return Outcome.Positive; },
            immediateEffect = (p) =>
            {
                for (int i = 0; i < amount; i++)
                {
                    p.SpawnNanobot(p.position);
                }
                return "My children will protect you.";
            }
        };
        return shrineEffect;
    }

    public static ShrineEffects increaseAllStats = new ShrineEffects
    {
        outcome = (p) => { return Outcome.Positive; },
        immediateEffect = (player) =>
        {
            var colors = new List<Color32>();
            foreach (var stat in Constants.allStatModules)
            {
                player.ModifyMinorItemCollection(stat, 1, false, false);
                colors.Add(Constants.moduleColors[stat]);
            }

            player.StartMultiColorFlash(0.2f, colors.ToArray(), 0.7f, true);

            if (player.onCollectItem != null) { player.onCollectItem(); }
            return "I bless you with strength in all ways Fight Smith " + SaveGameManager.fightNumber + ".";
        },
    };

    #region Neutral Effects
    public static ShrineEffects shuffleStats = new ShrineEffects()
    {
        outcome = (p) => { return Outcome.Neutral; },
        immediateEffect = (player) => { player.ShuffleStats(); return "I shall remake you!"; }
    };

    public static ShrineEffects redistributeArchaicScrap = new ShrineEffects()
    {
        outcome = (p) => { return Outcome.Neutral; },
        immediateEffect = (player) =>
        {
            var total = player.totalSpecialScrap;
            if (total > 0)
            {
                var values = Constants.UnevenSplit(total, 3, new MicrosoftRandom()).ToList();
                player.redScrap = values[Random.Range(0, values.Count)];
                values.Remove(player.redScrap);
                player.blueScrap = values[Random.Range(0, values.Count)];
                values.Remove(player.blueScrap);
                player.greenScrap = values[0];
                return "Perhaps this array of scrap will be more useful to you.";
            }
            else
            {
                return "There is nothing I can do for you.";
            }
        }
    };

    public static ShrineEffects statsToScap = new ShrineEffects()
    {
        outcome = (p) => { return Outcome.Neutral; },
        immediateEffect = (player) =>
        {
            var total = 0;
            var scrapType = new CurrencyType[] { CurrencyType.Blue, CurrencyType.Green, CurrencyType.Red };

            if(player.damageUps > 0 && Random.value > 0.5f)
            {
                player.damageUps--;
                player.GainScrap(scrapType[Random.Range(0, scrapType.Length)], 1);
                total++;
            }

            if (player.attackUps > 0 && Random.value > 0.5f)
            {
                player.attackUps--;
                player.GainScrap(scrapType[Random.Range(0, scrapType.Length)], 1);
                total++;
            }

            if (player.energyUps > 0 && Random.value > 0.5f)
            {
                player.energyUps--;
                player.GainScrap(scrapType[Random.Range(0, scrapType.Length)], 1);
                total++;
            }

            if (player.healthUps > 0 && Random.value > 0.5f)
            {
                player.healthUps--;
                player.GainScrap(scrapType[Random.Range(0, scrapType.Length)], 1);
                total++;
            }

            if (player.speedUps > 0 && Random.value > 0.5f)
            {
                player.speedUps--;
                player.GainScrap(scrapType[Random.Range(0, scrapType.Length)], 1);
                total++;
            }

            if (player.shotSpeedUps > 0 && Random.value > 0.5f)
            {
                player.shotSpeedUps--;
                player.GainScrap(scrapType[Random.Range(0, scrapType.Length)], 1);
                total++;
            }

            if (total > 0)
            {
                player.CalculateAllStats();
                return "All creation requires destruction.";
            }
            else
            {
                return "There is nothing I can do for you.";
            }
        }
    };

    public static ShrineEffects rerollNonTraversalArtifact = new ShrineEffects()
    {
        outcome = (p) => { return Outcome.Neutral; },
        immediateEffect = (player) =>
        {
            if (player.RerollNontraversalArtifact()) { return "A few alterations to your equipment."; }
            else { return "There is nothing I can do for you"; }
        }
    };
    #endregion

    #region Negative Effects
    public static ShrineEffects healthToOne = new ShrineEffects ///Reduce health to 1
    {
        outcome = (p) => { return Outcome.Negative; },
        immediateEffect = (player) =>
        {
            if (player.artificeMode)
            {
                player.grayScrap = 1;
            }
            else
            {
                player.health = 1;
            }
            return "I strip you of health Fight Smith " + SaveGameManager.fightNumber + "!";
        },
    };

    public static ShrineEffects energyToOne = new ShrineEffects ///Reduce energy to 1
    {
        outcome = (p) => { return Outcome.Negative; },
        immediateEffect = (player) =>
        {
            player.energy = 0;
            return "I strip you of energy Fight Smith " + SaveGameManager.fightNumber + "!";
        },
    };
    #endregion

    public static ShrineEffects nothing = new ShrineEffects ///Reduce energy to 1
    {
        outcome = (p) => { return Outcome.Negative; },
        immediateEffect = (player) =>
        {            
            return "You get nothing Fight Smith " + SaveGameManager.fightNumber + ". Good day!";
        },
    };

    #region Text Helpers
    public static string StatCurseText(MinorItemType minorItemType)
    {
        switch (minorItemType)
        {
            case MinorItemType.AttackModule:
                return "I curse your violent hand with lanquid sloth , Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.DamageModule:
                return "I curse your violent hand with pathetic decrepitude, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.EnergyModule:
                return "I curse your energy, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.HealthTank:
                return "I curse your health, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.ShotSpeedModule:
                return "I curse the bolts from your violent hand with sluggish apathy, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.SpeedModule:
                return "I curse you with lethargy, Fight Smith " + SaveGameManager.fightNumber + ".";
            default:
                return "error";
        }
    }

    public static string StatBlessingText(MinorItemType minorItemType)
    {
        switch (minorItemType)
        {
            case MinorItemType.AttackModule:
                return "I invigorate your violent hand with speed , Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.DamageModule:
                return "I bless your violent hand with deadly strength, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.EnergyModule:
                return "I enhance your holy power, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.HealthTank:
                return "I grant longevity to you, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.ShotSpeedModule:
                return "I embolden your killing bolts with speed, Fight Smith " + SaveGameManager.fightNumber + ".";
            case MinorItemType.SpeedModule:
                return "I grant you swiftness, Fight Smith " + SaveGameManager.fightNumber + ".";
            default:
                return "error";
        }
    }

    public static string StatNames(PlayerStatType statType)
    {
        switch(statType)
        {
            case PlayerStatType.Attack:
                return "your rate of fire.";
            case PlayerStatType.Damage:
                return "the might of your weaponry.";
            case PlayerStatType.ShotSize:
                return "the girth of your bolt.";
            case PlayerStatType.ShotSpeed:
                return "the speed of your bolt.";
            case PlayerStatType.Speed:
                return "your haste.";
            case PlayerStatType.Energy:
                return "your energy.";
            case PlayerStatType.Health:
                return "your health.";
            default:
                return "abilities";
        }
    }

    public static string AmountString(int amount)
    {
        switch (Math.Abs(amount))
        {
            case 0:
            case 1:
                return string.Empty;
            case 2:
                return "Twofold";
            case 3:
                return "Threefold";
            case 4:
                return "Fourfold";
            case 5:
                return "Fivefold";
            case 6:
                return "Sixfold";
            default:
                return "Many times";
        }
    }
#endregion
}
