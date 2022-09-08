using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ShrineInfo : DialogueInfo
{
    public ShrineType type;
    public Animator animator;
    public Animator[] lightAnimators;
    public Transform[] activatedItemSpots;
    public string offerNothing = "How dare you offer me nothing!";
    public string positiveEffect = "Your offer pleases me!";
    public string neutralEffect = "This is what you offer me?";
    public string negativeEffect = "Your offer offends me!";

    [NonSerialized]
    public ShrineTriggerBounds triggerBounds;
    [NonSerialized]
    public int timesUsed;

    public MinorItemType associatedMinorItem
    {
        get
        {
            switch (type)
            {
                case ShrineType.BulucChabtan:
                    return MinorItemType.AttackModule;
                case ShrineType.Hephaestus:
                    return MinorItemType.EnergyModule;
                case ShrineType.Tyr:
                    return MinorItemType.DamageModule;
                case ShrineType.WadjetMikail:
                    return MinorItemType.SpeedModule;
                case ShrineType.Zurvan:
                    return MinorItemType.HealthTank;
                case ShrineType.Orphiel:
                default:
                    return MinorItemType.None;
            }
        }
    }

    public int holyNumber
    {
        get
        {
            switch (type)
            {
                case ShrineType.BulucChabtan:
                    return 8;
                case ShrineType.Hephaestus:
                    return 6;
                case ShrineType.Orphiel:
                    return 9;
                case ShrineType.Tyr:
                    return 3;
                case ShrineType.WadjetMikail:
                    return 7;
                case ShrineType.Zurvan:
                    return 5;
                default:
                    return 1;
            }
        }
    }

    public List<MajorItem> itemPool = new List<MajorItem>();

    private Dictionary<ItemPrice, ShrineEffects> _effects; //assign based on type?
    public Dictionary<ItemPrice, ShrineEffects> effects { get { return _effects; } }

    public void Initialize()
    {
        if (_effects != null) return;

        _effects = new Dictionary<ItemPrice, ShrineEffects>();
        _effects.Add(new ItemPrice(4, 4, 4, 4), ShrineEffects.glitchWorld);

        itemPool.Clear();
        foreach (var kvp in ItemManager.items)
        {
            if (kvp.Value.shrinePools != null && kvp.Value.shrinePools.Contains(type)) { itemPool.Add(kvp.Key); }
        }

        switch (type)
        {
            case ShrineType.Tyr: //Damage
                var dRank = 1;
                var szRank = 1;
                for (int i = 3; i <= 99; i+=3)
                {
                    if (i % 9 == 0)
                    {
                        //9,18,27,36,45,
                        //54,63,72,81,90,
                        //99
                        _effects.Add(new ItemPrice(i, 0, 0, 0), ShrineEffects.AddTempMod(PlayerStatType.ShotSize, szRank/11f));
                        szRank++;
                    }
                    else
                    {
                        //3,6,12,15,21,
                        //24,30,33,39,42,
                        //48,51,57,60,66,
                        //69,75,78,84,87,
                        //90,93,96
                        _effects.Add(new ItemPrice(i, 0, 0, 0), ShrineEffects.AddTempMod(PlayerStatType.Damage, dRank/23f));
                        dRank++;
                    }
                }
                _effects.Add(new ItemPrice(6, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.ShootOrb } });
                _effects.Add(new ItemPrice(6, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.WreckingShell, MajorItem.DamageBoostAura } });
                _effects.Add(new ItemPrice(6, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.FragmentShot, MajorItem.EnergyAxe } });
                _effects.Add(new ItemPrice(9, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.LilTyr } });
                _effects.Add(new ItemPrice(9, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.PenetrativeShot } });
                _effects.Add(new ItemPrice(9, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.AuraBolt } });
                _effects.Add(new ItemPrice(27, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.MegaDamage } });
                _effects.Add(new ItemPrice(27, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.BigBolt } });
                _effects.Add(new ItemPrice(27, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.TripleShot } });
                break;
            case ShrineType.Zurvan: //Health
                _effects.Add(new ItemPrice(5, 0, 0, 0), ShrineEffects.fullHealth);
                _effects.Add(new ItemPrice(10, 0, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.BossMap, MajorItem.CaveMap, MajorItem.BuriedCityMap, MajorItem.FactoryMap } });
                _effects.Add(new ItemPrice(15, 0, 0, 0), ShrineEffects.AddTempRegen(0.5f, 240));
                _effects.Add(new ItemPrice(20, 0, 0, 0), ShrineEffects.AddTempRegen(0.75f, 240));
                _effects.Add(new ItemPrice(25, 0, 0, 0), ShrineEffects.AddTempRegen(1f, 240));
                _effects.Add(new ItemPrice(30, 0, 0, 0), ShrineEffects.AddTempRegen(1.1f, 240));
                _effects.Add(new ItemPrice(35, 0, 0, 0), ShrineEffects.AddTempRegen(1.15f, 240));
                _effects.Add(new ItemPrice(40, 0, 0, 0), ShrineEffects.AddTempRegen(1.25f, 240));
                _effects.Add(new ItemPrice(45, 0, 0, 0), ShrineEffects.AddTempRegen(1.3f, 240));
                _effects.Add(new ItemPrice(50, 0, 0, 0), ShrineEffects.AddTempRegen(1.4f, 240));
                _effects.Add(new ItemPrice(55, 0, 0, 0), ShrineEffects.AddTempRegen(1.5f, 240));
                _effects.Add(new ItemPrice(60, 0, 0, 0), ShrineEffects.AddTempRegen(1.75f, 240));
                _effects.Add(new ItemPrice(65, 0, 0, 0), ShrineEffects.AddTempRegen(1.85f, 240));
                _effects.Add(new ItemPrice(70, 0, 0, 0), ShrineEffects.AddTempRegen(2f, 240));
                _effects.Add(new ItemPrice(75, 0, 0, 0), ShrineEffects.AddTempRegen(2.25f, 240));
                _effects.Add(new ItemPrice(80, 0, 0, 0), ShrineEffects.AddTempRegen(2.5f, 240));
                _effects.Add(new ItemPrice(85, 0, 0, 0), ShrineEffects.ModifyStat(1, MinorItemType.HealthTank));
                _effects.Add(new ItemPrice(90, 0, 0, 0), ShrineEffects.ModifyStat(1, MinorItemType.HealthTank));
                _effects.Add(new ItemPrice(95, 0, 0, 0), ShrineEffects.ModifyStat(1, MinorItemType.HealthTank));
                _effects.Add(new ItemPrice(10, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.Explorb } });
                _effects.Add(new ItemPrice(10, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.AttractorHelm } });
                _effects.Add(new ItemPrice(10, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.ArtifactMap, MajorItem.SearchBurst } });
                _effects.Add(new ItemPrice(15, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.HealthVorb, MajorItem.LilZurvan } });
                _effects.Add(new ItemPrice(15, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.RegenerationHelm } });
                _effects.Add(new ItemPrice(15, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.MegaHealth } });
                _effects.Add(new ItemPrice(20, 1, 1, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.ETHChip } });
                break;
            case ShrineType.Hephaestus: //Energy, Scrap, Shops
                _effects.Add(new ItemPrice(6, 0, 0, 0), ShrineEffects.fullEnergy);
                _effects.Add(new ItemPrice(12, 0, 0, 0), ShrineEffects.redistributeArchaicScrap);
                _effects.Add(new ItemPrice(18, 0, 0, 0), ShrineEffects.AddTempEnergyRegen(0.5f, 240));
                _effects.Add(new ItemPrice(24, 0, 0, 0), ShrineEffects.rerollNonTraversalArtifact);
                _effects.Add(new ItemPrice(30, 0, 0, 0), ShrineEffects.AddTempCelestialCharge(60));
                _effects.Add(new ItemPrice(36, 0, 0, 0), ShrineEffects.rerollNonTraversalArtifact);
                _effects.Add(new ItemPrice(42, 0, 0, 0), ShrineEffects.AddTempEnergyRegen(1f, 240));
                _effects.Add(new ItemPrice(48, 0, 0, 0), ShrineEffects.AddTempCelestialCharge(120));
                _effects.Add(new ItemPrice(54, 0, 0, 0), ShrineEffects.AddTempEnergyRegen(1.25f, 240));
                _effects.Add(new ItemPrice(60, 0, 0, 0), ShrineEffects.AddTempCelestialCharge(180));
                _effects.Add(new ItemPrice(66, 0, 0, 0), ShrineEffects.AddTempEnergyRegen(1.5f, 240));
                _effects.Add(new ItemPrice(72, 0, 0, 0), ShrineEffects.AddTempCelestialCharge(240));
                _effects.Add(new ItemPrice(78, 0, 0, 0), ShrineEffects.AddTempEnergyRegen(2f, 240));
                //Add energy regen
                _effects.Add(new ItemPrice(84, 0, 0, 0), ShrineEffects.ModifyStat(1, MinorItemType.EnergyModule));
                _effects.Add(new ItemPrice(90, 0, 0, 0), ShrineEffects.ModifyStat(1, MinorItemType.EnergyModule));
                _effects.Add(new ItemPrice(96, 0, 0, 0), ShrineEffects.ModifyStat(1, MinorItemType.EnergyModule));
                _effects.Add(new ItemPrice(0, 1, 1, 1), ShrineEffects.ModifyStat(3, MinorItemType.EnergyModule));
                //scrap trade ins
                _effects.Add(new ItemPrice(0, 1, 0, 0), new ShrineEffects()
                {
                    outcome = (p) => { return Outcome.Neutral; },
                    immediateEffect = (p) => { p.GainScrap(CurrencyType.Gray, 15); return "Give me what you don't want. I'll give you what you need."; },
                });
                _effects.Add(new ItemPrice(0, 0, 1, 0), new ShrineEffects()
                {
                    outcome = (p) => { return Outcome.Neutral; },
                    immediateEffect = (p) => { p.GainScrap(CurrencyType.Gray, 15); return "Give me what you don't want. I'll give you what you need."; },
                });
                _effects.Add(new ItemPrice(0, 0, 0, 1), new ShrineEffects()
                {
                    outcome = (p) => { return Outcome.Neutral; },
                    immediateEffect = (p) => { p.GainScrap(CurrencyType.Gray, 15); return "Give me what you don't want. I'll give you what you need."; },
                });
                _effects.Add(new ItemPrice(0, 0, 1, 1), new ShrineEffects()
                {
                    outcome = (p) => { return Outcome.Neutral; },
                    immediateEffect = (p) => { p.GainScrap(CurrencyType.Red, 1); return "Give me what you don't want. I'll give you what you need."; },
                });
                _effects.Add(new ItemPrice(0, 1, 0, 1), new ShrineEffects()
                {
                    outcome = (p) => { return Outcome.Neutral; },
                    immediateEffect = (p) => { p.GainScrap(CurrencyType.Green, 1); return "Give me what you don't want. I'll give you what you need."; },
                });
                _effects.Add(new ItemPrice(0, 1, 1, 0), new ShrineEffects()
                {
                    outcome = (p) => { return Outcome.Neutral; },
                    immediateEffect = (p) => { p.GainScrap(CurrencyType.Blue, 1); return "Give me what you don't want. I'll give you what you need."; },
                });
                _effects.Add(new ItemPrice(12, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.EnergyVorb } });
                _effects.Add(new ItemPrice(18, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.PowerPauldrons, MajorItem.LilPhaestus } });
                _effects.Add(new ItemPrice(18, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.MegaEnergy, MajorItem.EnergyAxe } });
                _effects.Add(new ItemPrice(18, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.ArtificeHelm } });
                _effects.Add(new ItemPrice(24, 1, 1, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.CelestialCharge } });
                break;
            case ShrineType.WadjetMikail: //Speed
                for (int i = 1; i <= 7; i ++)
                {
                    _effects.Add(new ItemPrice(i*14, 0, 0, 0), ShrineEffects.AddTempMod(PlayerStatType.Speed, i/7f));
                }
                _effects.Add(new ItemPrice(7, 0, 0, 0), ShrineEffects.AddTempSheild(60));
                _effects.Add(new ItemPrice(21, 0, 0, 0), ShrineEffects.AddTempSheild(90));
                _effects.Add(new ItemPrice(35, 0, 0, 0), ShrineEffects.AddTempSheild(120));
                _effects.Add(new ItemPrice(49, 0, 0, 0), ShrineEffects.AddTempSheild(150));
                _effects.Add(new ItemPrice(63, 0, 0, 0), ShrineEffects.AddTempSheild(210));
                _effects.Add(new ItemPrice(77, 0, 0, 0), ShrineEffects.AddTempSheild(270));
                _effects.Add(new ItemPrice(14, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.HunterKiller, MajorItem.PowerShield } });
                _effects.Add(new ItemPrice(14, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.MegaSpeed, MajorItem.SpeedBoostAura } });
                _effects.Add(new ItemPrice(14, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.SpeedShell, MajorItem.CloakingDevice } });
                _effects.Add(new ItemPrice(21, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.RoyalOrb, MajorItem.LilWadjet } });
                _effects.Add(new ItemPrice(21, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.LaserBeam, } });
                _effects.Add(new ItemPrice(21, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.HomingBolt, MajorItem.SearchBurst } });
                break;
            case ShrineType.BulucChabtan: //Attack
                for (int i = 1; i <= 12; i++)
                {
                    _effects.Add(new ItemPrice(i*8, 0, 0, 0), ShrineEffects.AddTempMod(PlayerStatType.Attack, i/12f));
                }
                _effects.Add(new ItemPrice(16, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.TriOrb, MajorItem.LilBuluc, MajorItem.Mechapirote } });
                _effects.Add(new ItemPrice(16, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.MachineGun, MajorItem.AttackBoostAura, MajorItem.RadialBolts } });
                _effects.Add(new ItemPrice(16, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.ScatterGun, MajorItem.WaveBomb } });
                _effects.Add(new ItemPrice(24, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.MegaAttack, MajorItem.LilBuluc } });
                _effects.Add(new ItemPrice(24, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.LaserBeam } });
                _effects.Add(new ItemPrice(24, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.BigBolt, MajorItem.ToxinCloud } });
                break;
            case ShrineType.Orphiel:
                _effects.Add(new ItemPrice(9, 0, 0, 0), ShrineEffects.SpawnNanobots(3));
                _effects.Add(new ItemPrice(18, 0, 0, 0), ShrineEffects.SpawnNanobots(7));
                _effects.Add(new ItemPrice(27, 0, 0, 0), ShrineEffects.AddTempSheildOrbs(60));
                _effects.Add(new ItemPrice(36, 0, 0, 0), ShrineEffects.SpawnNanobots(14));
                _effects.Add(new ItemPrice(45, 0, 0, 0), ShrineEffects.SpawnNanobots(17));
                _effects.Add(new ItemPrice(54, 0, 0, 0), ShrineEffects.AddTempSheildOrbs(120));
                _effects.Add(new ItemPrice(63, 0, 0, 0), ShrineEffects.SpawnNanobots(21));
                _effects.Add(new ItemPrice(72, 0, 0, 0), ShrineEffects.SpawnNanobots(25));
                _effects.Add(new ItemPrice(81, 0, 0, 0), ShrineEffects.AddTempSheildOrbs(180));
                _effects.Add(new ItemPrice(90, 0, 0, 0), ShrineEffects.SpawnNanobots(30));
                _effects.Add(new ItemPrice(99, 0, 0, 0), ShrineEffects.AddTempSheildOrbs(240));
                _effects.Add(new ItemPrice(9, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.Explorb, MajorItem.NanoswarmGenerator } });
                _effects.Add(new ItemPrice(9, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.ShootOrb, MajorItem.WaveBomb } });
                _effects.Add(new ItemPrice(9, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.ShieldOrb, MajorItem.PhantasmalOrbs } });
                _effects.Add(new ItemPrice(18, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.Mothorb, MajorItem.LilOrphy} });
                _effects.Add(new ItemPrice(18, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.RoyalOrb } });
                _effects.Add(new ItemPrice(18, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.HunterKiller } });
                _effects.Add(new ItemPrice(27, 0, 1, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.HealthOrb } });
                _effects.Add(new ItemPrice(27, 0, 0, 1), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.HealthVorb } });
                _effects.Add(new ItemPrice(36, 1, 0, 0), new ShrineEffects() { possibleItemRewards = new List<MajorItem> { MajorItem.TheThief } });
                break;
        }
    }
}
