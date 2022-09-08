using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//#if STEAM
//using Steamworks;
//#endif

[DisallowMultipleComponent]
public partial class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    private List<AchievementID> _achievementQueue = new List<AchievementID>();
    public int queuedAchievements
    {
        get { return _achievementQueue.Count; }
    }

    public void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }

    public void Update()
    {
        if(_achievementQueue.Count > 0 && AchievementScreen.instance && !AchievementScreen.instance.visible && (!ItemCollectScreen.instance || !ItemCollectScreen.instance.visible))
        {
            var achievement = _achievementQueue[0];
            _achievementQueue.Remove(achievement);
            TryEarnAchievement(achievement);
        }
    }

//    public void SyncAchievements()
//    {
//#if STEAM
//        if (SaveGameManager.activeSlot != null)
//        {
//            StartCoroutine(SyncAchievementsRoutine(SaveGameManager.activeSlot.achievements));
//        }
//#endif
//    }

//#if STEAM
//    public IEnumerator SyncAchievementsRoutine(List<AchievementID> achievements)
//    {
//        if(!SteamManager.Initialized)
//        {
//            Debug.LogError("SteamManager is Not Initialized");
//            yield break;
//        }

//        bool earned;
//        string achievementName;
//        for (int i = 0; i < achievements.Count; i++)
//        {
//            achievementName = achievements[i].ToString();
//            earned = false;
//            SteamUserStats.GetAchievement(achievementName, out earned);

//            if (!earned)
//            {
//                SteamUserStats.SetAchievement(achievementName);
//                yield return new WaitForSeconds(0.75f);
//            }
//            else
//            {
//                yield return null;
//            }
//        }
//    }
//#endif

    public bool CheckForKillAchievements()
    {
        var result = false;
        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null)
        {
            var totalKills = activeSlot.totalKills;
            if(totalKills >= 100)
            {
                if (TryEarnAchievement(AchievementID.EnergyVorb)) result = true;

                if(totalKills >= 1000)
                {
                    if (TryEarnAchievement(AchievementID.ScrapVorb)) result = true;

                    if (totalKills >= 7500)
                    {
                        if (TryEarnAchievement(AchievementID.HealthVorb)) result = true;

                        if(totalKills >= 15000)
                        {
                            if (TryEarnAchievement(AchievementID.Exterminator)) result = true;
                        }
                    }
                }
            }
        }

        return result;
    }

    public bool TryEarnAchievement(AchievementID achievementID)
    {
        var game = SaveGameManager.activeGame;
        if (DeathmatchManager.instance || (game != null && !game.allowAchievements)) { return false; }

//#if STEAM
//        if (SteamManager.Initialized && SteamUserStats.SetAchievement(achievementID.ToString()))
//        {
//            SteamUserStats.StoreStats();
//        }
//#endif

        if (SaveGameManager.activeSlot != null && !SaveGameManager.activeSlot.achievements.Contains(achievementID))
        {
            //Don't allow achievements is the run has a password

            if ((AchievementScreen.instance && AchievementScreen.instance.visible) || (ItemCollectScreen.instance && ItemCollectScreen.instance.visible))
            {
                _achievementQueue.Add(achievementID);
                return true;
            }
            else
            {
                SaveGameManager.activeSlot.achievements.Add(achievementID);

                AchievementInfo info;
                if (achievements.TryGetValue(achievementID, out info) && AchievementScreen.instance)
                {
                    AchievementScreen.instance.Show(info);
                }

                SaveGameManager.instance.Save();
                return true;
            }
        }

        return false;
    }

    public bool EnvironmentDiscovered(EnvironmentType environmentType)
    {
        if(SaveGameManager.activeGame != null && !SaveGameManager.activeGame.allowAchievements) { return false; }

        var achievement = AchievementID.None;
        switch (environmentType)
        {
            case EnvironmentType.Cave:
                achievement = AchievementID.BrightShell;
                break;
            case EnvironmentType.Factory:
                achievement = AchievementID.RailGun;
                break;
            case EnvironmentType.BuriedCity:
                achievement = AchievementID.BuzzOrb;
                break;
            case EnvironmentType.BeastGuts:
                achievement = AchievementID.RegenerationHelm;
                break;
            case EnvironmentType.Glitch:
                achievement = AchievementID.GlitchModule;
                break;
            case EnvironmentType.CoolantSewers:
                achievement = AchievementID.DiveShell;
                break;
            case EnvironmentType.CrystalMines:
                achievement = AchievementID.CrystalShell;
                break;
        }

        bool result;
        if (achievement != AchievementID.None)
        {
            result = TryEarnAchievement(achievement);
        }
        else
        {
            result = false;
        }

        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null && !activeSlot.achievements.Contains(AchievementID.MegaMap))
        {
            var allEnv = new EnvironmentType[] { EnvironmentType.BuriedCity, EnvironmentType.Cave, EnvironmentType.CoolantSewers,
            EnvironmentType.CrystalMines, EnvironmentType.Factory, EnvironmentType.ForestSlums, EnvironmentType.Surface, EnvironmentType.BeastGuts };
            if (activeSlot.environmentsDiscovered.ContainsAll(allEnv) && TryEarnAchievement(AchievementID.MegaMap))
            {
                result = true;
            }
        }

        return result;
    }

    public bool MegaBeastCoreDefeated()
    {
        var result = false;

        var slot = SaveGameManager.activeSlot;
        var game = SaveGameManager.activeGame;

        if (slot != null && game != null && game.allowAchievements)
        {    
            if (!slot.achievements.Contains(AchievementID.AspectShell))
            {
                TryEarnAchievement(AchievementID.AspectShell);
                result = true;
            }

            if (game.gameMode == GameMode.MegaMap && TryEarnAchievement(AchievementID.EmpireHelm))
            {
                result = true;
            }

            if (game.collectRate >= 1 && game.playTime <= (60 * 60) && !slot.achievements.Contains(AchievementID.FightBot))
            {
                TryEarnAchievement(AchievementID.FightBot);
                result = true;
            }
            else if (game.collectRate < 0.15f && !slot.achievements.Contains(AchievementID.TheQuickening))
            {
                TryEarnAchievement(AchievementID.TheQuickening);
                result = true;
            }
        }

        return result;
    }

    public bool TutorialSmithDefeated()
    {
        var result = false;

        if (SaveGameManager.activeSlot != null)
        {
            var slot = SaveGameManager.activeSlot;
            var game = SaveGameManager.activeGame;

            if (!slot.achievements.Contains(AchievementID.CognitiveStabilizer))
            {
                TryEarnAchievement(AchievementID.CognitiveStabilizer);
                result = true;
            }

            if (game.gameMode == GameMode.MegaMap && TryEarnAchievement(AchievementID.EmpireHelm))
            {
                result = true;
            }

            if (game.collectRate < 0.15f && !slot.achievements.Contains(AchievementID.TheQuickening))
            {
                TryEarnAchievement(AchievementID.TheQuickening);
                result = true;
            }
        }

        return result;
    }

    public bool MegaBeastDefeated()
    {
        var game = SaveGameManager.activeGame;
        var slot = SaveGameManager.activeSlot;

        if (slot != null && game != null && game.allowAchievements)
        {
            var result = false;
            var achievement = AchievementID.None;

            if (game.gameMode == GameMode.Normal || game.gameMode == GameMode.MegaMap)
            {
                slot.megaBeastKills++;
                var megaBeastKills = slot.megaBeastKills;

                switch (megaBeastKills)
                {
                    case 1:
                        achievement = AchievementID.MegaDamage;
                        break;
                    case 2:
                        achievement = AchievementID.MegaHealth;
                        break;
                    case 3:
                        achievement = AchievementID.MegaEnergy;
                        break;
                    case 4:
                        achievement = AchievementID.MegaAttack;
                        break;
                    case 5:
                        achievement = AchievementID.MegaSpeed;
                        break;
                    case 6:
                        achievement = AchievementID.MegaShotSpeed;
                        break;
                }

                if (achievement != AchievementID.None && TryEarnAchievement(achievement))
                {
                    result = true;
                }

                if (megaBeastKills >= 1 && TryEarnAchievement(AchievementID.TheFleshening))
                {
                    result = true;
                }

                if (megaBeastKills >= 2 && TryEarnAchievement(AchievementID.TheFlesheningII))
                {
                    result = true;
                }

                if (megaBeastKills >= 3 && TryEarnAchievement(AchievementID.BeastGuts))
                {
                    result = true;
                }
            }

            if(game.gameMode != GameMode.BossRush)
            { 
                if (game.playTime <= 40 * 60 && TryEarnAchievement(AchievementID.FastBot))
                {
                    result = true;
                }

                if (game.collectRate < 0.15f && TryEarnAchievement(AchievementID.TheQuickening))
                {
                    result = true;
                }
            }

            if(game.gameMode == GameMode.BossRush && TryEarnAchievement(AchievementID.TrollHelm))
            {
                result = true;
            }

            if(game.gameMode == GameMode.Exterminator && TryEarnAchievement(AchievementID.ToxinOrb))
            {
                result = true;
            }

            if (CheckForBossRush())
            {
                result = true;
            }

            return result;
        }

        return false;
    }

    public bool CheckForNewAchievements()
    {
        var result = false;

        var activeSlot = (SaveGameManager.activeSlot);
        if (activeSlot.megaBeastKills >= 1 && TryEarnAchievement(AchievementID.TheFleshening)) { result = true; }
        if (activeSlot.megaBeastKills >= 1 && TryEarnAchievement(AchievementID.MegaDamage)) { result = true; }
        if (activeSlot.megaBeastKills >= 2 && TryEarnAchievement(AchievementID.TheFlesheningII)) { result = true; }
        if (activeSlot.megaBeastKills >= 2 && TryEarnAchievement(AchievementID.MegaHealth)) { result = true; }
        if (activeSlot.megaBeastKills >= 3 && TryEarnAchievement(AchievementID.BeastGuts)) { result = true; }
        if (activeSlot.megaBeastKills >= 3 && TryEarnAchievement(AchievementID.MegaEnergy)) { result = true; }
        if (activeSlot.megaBeastKills >= 4 && TryEarnAchievement(AchievementID.MegaAttack)) { result = true; }
        if (activeSlot.megaBeastKills >= 5 && TryEarnAchievement(AchievementID.MegaSpeed)) { result = true; }
        if (activeSlot.megaBeastKills >= 6 && TryEarnAchievement(AchievementID.MegaShotSpeed)) { result = true; }
        if (activeSlot.totalDeaths > 0 && TryEarnAchievement(AchievementID.RevenantStation)) { result = true; }
        if (activeSlot.challengesCompleted.Contains(GameMode.BossRush) && TryEarnAchievement(AchievementID.TrollHelm)) { result = true; }
        if (activeSlot.challengesCompleted.Contains(GameMode.Exterminator) && TryEarnAchievement(AchievementID.ToxinOrb)) { result = true; }
        if (activeSlot.challengesCompleted.Contains(GameMode.MegaMap) && TryEarnAchievement(AchievementID.EmpireHelm)) { result = true; }

        foreach (var env in activeSlot.environmentsDiscovered)
        {
            if (EnvironmentDiscovered(env)) { result = true; }
        }

        foreach (var kvp in activeSlot.bossKills)
        {
            if (kvp.Value > 0)
            {
                var achievement = GetBossAchievement(kvp.Key);
                if (achievement != AchievementID.None && TryEarnAchievement(achievement))
                {
                    result = true;
                }
            }
        }

        if (CheckForKillAchievements()) { result = true; }
        if (CheckForBossAchievements(false)) { result = true; }

        long red, green, blue;
        if (activeSlot.scrapSpent.TryGetValue(CurrencyType.Red, out red) && red >= 3 && activeSlot.scrapSpent.TryGetValue(CurrencyType.Green, out green) && green >= 3 && 
            activeSlot.scrapSpent.TryGetValue(CurrencyType.Blue, out blue) && blue >= 3 && TryEarnAchievement(AchievementID.TheTraitor))
        {
            result = true;
        }

        //SyncAchievements();

        return result;
    }

    public AchievementID GetBossAchievement(BossName bossName)
    {
        switch (bossName)
        {
            case BossName.Abomination:
                return AchievementID.BigBolt;
            case BossName.BeakLord:
                return AchievementID.PowerJump;
            case BossName.MouthMeatSenior:
                return AchievementID.LightningGun;
            case BossName.WallCreep:
                return AchievementID.TripleShot;
            case BossName.MetalPatriarch:
                return AchievementID.LaserBeam;
            case BossName.FleshAdder:
                return AchievementID.HunterKiller;
            case BossName.Polyphemus:
                return AchievementID.RoyalOrb;                
            case BossName.MolemanShaman:
                return AchievementID.HomingBolt;
            case BossName.Cephalodiptera:
                return AchievementID.HoverBoots;
            case BossName.Sluggard:
                return AchievementID.AutoTurret;
            case BossName.WhiteWyrm:
                return AchievementID.ViridianShell;
            case BossName.MouthMeat:
                return AchievementID.TriOrb;
            case BossName.OozeHart:
                return AchievementID.NecroluminantSpray;
            case BossName.Blightbark:
                return AchievementID.HiveHelm;
            case BossName.Leviathan:
                return AchievementID.UpDog;
            case BossName.Stalkus:
                return AchievementID.Lasorb;
            case BossName.CorruptedMiner:
                return AchievementID.HazardShell;
            case BossName.SkinDeviler:
                return AchievementID.PhaseShell;
            default:
                return AchievementID.None;
        }
    }

    public void BossDefeated(BossName bossName)
    {
        var game = SaveGameManager.activeGame;
        var slot = SaveGameManager.activeSlot;

        if(game == null || slot == null || !game.allowAchievements) { return; }

        var achievement = GetBossAchievement(bossName);
        
        if (slot != null)
        {
            if (!slot.bossKills.ContainsKey(bossName)) { slot.bossKills[bossName] = 0; }

            if (slot.bossKills[bossName] < ushort.MaxValue)
            {
                SaveGameManager.activeSlot.bossKills[bossName]++;
                SaveGameManager.instance.Save();
            }
        }

        if (achievement != AchievementID.None)
        {
            StartCoroutine(WaitEarnAchievement(2, achievement));
        }

        CheckForBossAchievements(true);
    }

    public bool CheckForBossAchievements(bool wait)
    {
        var slot = SaveGameManager.activeSlot;
        var result = false;

        if(slot != null)
        {
            ushort count = 0;
            ushort kills;
            foreach (var boss in Constants.surfaceBosses)
            {
                if (slot.bossKills.TryGetValue(boss, out kills) && kills > 0) { count++; }
            }

            if(count == Constants.surfaceBosses.Count)
            {
                if (wait)
                {
                    StartCoroutine(WaitEarnAchievement(2, AchievementID.ForestSlums));
                }
                else
                {
                    TryEarnAchievement(AchievementID.ForestSlums);
                }
                result = true;
            }

            count = 0;
            foreach (var boss in Constants.caveBosses)
            {
                if (slot.bossKills.TryGetValue(boss, out kills) && kills > 0) { count++; }
            }

            if (count == Constants.caveBosses.Count)
            {
                if (wait)
                {
                    StartCoroutine(WaitEarnAchievement(2, AchievementID.CoolantSewers));
                }
                else
                {
                    TryEarnAchievement(AchievementID.CoolantSewers);
                }
                result = true;
            }

            count = 0;
            foreach (var boss in Constants.factoryBosses)
            {
                if (slot.bossKills.TryGetValue(boss, out kills) && kills > 0) { count++; }
            }

            if (count == Constants.factoryBosses.Count)
            {
                if (wait)
                {
                    StartCoroutine(WaitEarnAchievement(2, AchievementID.CrystalMines));
                }
                else
                {
                    TryEarnAchievement(AchievementID.CrystalMines);
                }
                result = true;
            }
        }

        if(CheckForBossRush())
        {
            result = true;
        }

        return result;
    }

    public bool CheckForBossRush()
    {
        var slot = SaveGameManager.activeSlot;
        var count = 0;

        foreach (var kvp in slot.bossKills)
        {
            if (kvp.Value > 0) count++;
        }

        if (slot.megaBeastKills > 0) { count++; }

        if (count >= 14)
        {
            return TryEarnAchievement(AchievementID.BossRush);
        }
        else
        {
            return false;
        }
    }

    public void WaitTryEarnAchievement(float delay, AchievementID achievement)
    {
        StartCoroutine(WaitEarnAchievement(delay, achievement));
    }

    private IEnumerator WaitEarnAchievement(float delay, AchievementID achievement)
    {
        while (LayoutManager.instance && LayoutManager.instance.transitioning) { yield return null; }
        yield return new WaitForSeconds(delay);
        TryEarnAchievement(achievement);
    }
}

[Serializable]
public class AchievementInfo
{
    public AchievementID achievementID;
    public string name;
    public string description;
    public string collectMeans;
    public MajorItem associatedItem;
    public BossName associatedBoss;
    public bool hidden;
}