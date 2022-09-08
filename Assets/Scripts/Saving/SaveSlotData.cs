using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

[Serializable]
public class SaveSlotData
{
    public SaveSlotData()
    {
        started = DateTime.Now;
    }

    [JsonProperty(PropertyName = "slot")]
    public int slotNumber;
    [JsonProperty(PropertyName = "strt")]
    public DateTime started;
    [JsonProperty(PropertyName = "agd")]
    public SaveGameData activeGameData;
    [JsonProperty(PropertyName = "achv")]
    public List<AchievementID> achievements = new List<AchievementID>();
    [JsonProperty(PropertyName = "envDisc")]
    public List<EnvironmentType> environmentsDiscovered = new List<EnvironmentType>();
    [JsonProperty(PropertyName = "rewiredCfg")]
    public Dictionary<string, string> rewiredConfig = new Dictionary<string, string>();
    [JsonProperty(PropertyName = "tVis")]
    public bool timeAlwaysVisible = true;
    [JsonProperty(PropertyName = "sVis")]
    public bool scrapAlwaysVisible = true;
    [JsonProperty(PropertyName = "numHE")]
    public bool numericHealthAndEnergy = false;
    [JsonProperty(PropertyName = "achUp")]
    public bool needAchievementUpdate = false;
    [JsonProperty(PropertyName = "bld")]
    public bool blood = true;
    [JsonProperty(PropertyName = "fllt")]
    public bool flashingLights = true;
    [JsonProperty(PropertyName = "aLk")]
    public bool lookControls = true;
    [JsonProperty(PropertyName = "shtchrg")]
    public bool shotCharging = true;

    //Stats
    [JsonProperty(PropertyName = "deathS")]
    public int totalDeaths;
    [JsonProperty(PropertyName = "mbk")]
    public int megaBeastKills;
    [JsonProperty(PropertyName = "bskils")]
    public Dictionary<BossName, ushort> bossKills = new Dictionary<BossName, ushort>();
    [JsonProperty(PropertyName = "totRuns")]
    public uint totalRuns;
    [JsonProperty(PropertyName = "hCmpRt")]
    public float highestCompletionRate;
    [JsonProperty(PropertyName = "lCmpRt")]
    public float lowestCompletionRate = -1;
    [JsonProperty(PropertyName = "chmpKld")]
    public ushort championsKilled;
    [JsonProperty(PropertyName = "scrpClctd")]
    public Dictionary<CurrencyType, long> scrapCollected = new Dictionary<CurrencyType, long>();
    [JsonProperty(PropertyName = "mpScsUnc")]
    public long mapSpacesUncovered;
    [JsonProperty(PropertyName = "totItmPur")]
    public uint totalItemsPurchased;
    [JsonProperty(PropertyName = "totKil")]
    public long totalKills;
    [JsonProperty(PropertyName = "totBlkD")]
    public long totalBlocksDestroyed;
    [JsonProperty(PropertyName = "itmsCol")]
    public HashSet<MajorItem> itemsCollected = new HashSet<MajorItem>();
    [JsonProperty(PropertyName = "scrpSpnt")]
    public Dictionary<CurrencyType, long> scrapSpent = new Dictionary<CurrencyType, long>();
    [JsonProperty(PropertyName = "scrpDonated")]
    public Dictionary<CurrencyType, long> scrapDonated = new Dictionary<CurrencyType, long>();
    [JsonProperty(PropertyName = "eggsDst")]
    public long eggsDestroyed;
    [JsonProperty(PropertyName = "vict")]
    public uint victories;
    [JsonProperty(PropertyName = "crntStk")]
    public short currentStreak;
    [JsonProperty(PropertyName = "lngStk")]
    public short bestStreak;
    [JsonProperty(PropertyName = "bstTm")]
    public float bestTime;
    [JsonProperty(PropertyName = "tSmthV")]
    public bool tutorialSmithVisted;
    [JsonProperty(PropertyName = "pstSds")]
    public List<string> pastSeeds = new List<string>();
    [JsonProperty(PropertyName = "scrtSds")]
    public List<string> secretSeeds = new List<string>();

    [JsonProperty(PropertyName = "dmStng")]
    public DeathmatchSettings deathmatchSettings = new DeathmatchSettings();

    [JsonProperty(PropertyName = "sk")]
    public bool spookyFinished;

    //Debug
    //[JsonProperty(PropertyName = "fud")]
    [JsonIgnore]
    public bool forcePhaseShell;
    //[JsonProperty(PropertyName = "fds")]
    [JsonIgnore]
    public bool forceCrystalMine;
    //[JsonProperty(PropertyName = "fclSwr")]
    [JsonIgnore]
    public bool forceCoolantSewers; //no longer implemented

    //1.5.0.30
    [JsonProperty(PropertyName = "chlngCmplt")]
    public List<GameMode> challengesCompleted = new List<GameMode>();
    [JsonProperty(PropertyName = "stsAVis")]
    public bool statsAlwaysVisible = true;
    [JsonProperty(PropertyName = "bsRBstTm")]
    public float bossRushBestTime;
    [JsonProperty(PropertyName = "mmBstTm")]
    public float megaMapBestTime;
    [JsonProperty(PropertyName = "exBstTm")]
    public float exterminatorBestTime;

    public void AddSeedToPastSeeds(string seed)
    {
        if (pastSeeds.Contains(seed)) { pastSeeds.Remove(seed); }

        var backUpPastSeeds = new List<string>(pastSeeds);
        if (backUpPastSeeds.Count > 9) { backUpPastSeeds.RemoveAt(backUpPastSeeds.Count-1); }
        pastSeeds = new List<string> { seed };
        pastSeeds.AddRange(backUpPastSeeds);
    }

    public void AddSeedToSecretSeeds(string seed)
    {
        if (secretSeeds.Contains(seed)) { secretSeeds.Remove(seed); }

        var backUpSecretSeeds = new List<string>(secretSeeds);
        if (backUpSecretSeeds.Count > 9) { backUpSecretSeeds.RemoveAt(backUpSecretSeeds.Count - 1); }
        secretSeeds = new List<string> { seed };
        secretSeeds.AddRange(backUpSecretSeeds);
    }

    public void RunCompleted()
    {
        if(activeGameData != null)
        {
            activeGameData.runCompleted = true;

            var rate = activeGameData.collectRate;

            switch(activeGameData.gameMode)
            {
                case GameMode.MegaMap:
                case GameMode.Exterminator:
                case GameMode.BossRush:
                    challengesCompleted.Add(activeGameData.gameMode);
                    break;
            }

            if (activeGameData.allowAchievements)
            {
                if (victories < uint.MaxValue) { victories++; }
                if (currentStreak < 0) { currentStreak = 0; }
                if (currentStreak < short.MaxValue) { currentStreak++; }
                if (currentStreak > bestStreak) { bestStreak = currentStreak; }

                var pTime = activeGameData.playTime;
                switch (activeGameData.gameMode)
                {
                    case GameMode.MegaMap:
                        if (pTime < megaMapBestTime || megaMapBestTime == 0) { megaMapBestTime = pTime; }
                        break;
                    case GameMode.Exterminator:
                        if (pTime < exterminatorBestTime || exterminatorBestTime == 0) { exterminatorBestTime = pTime; }
                        break;
                    case GameMode.BossRush:
                        if (pTime < bossRushBestTime || bossRushBestTime == 0) { bossRushBestTime = pTime; }
                        break;
                    default:
                        if (pTime < bestTime || bestTime == 0) { bestTime = pTime; }
                        break;
                }
                
                if (activeGameData.gameMode != GameMode.BossRush)
                {
                    if (rate > highestCompletionRate) { highestCompletionRate = rate; }
                    if (rate < lowestCompletionRate || lowestCompletionRate == -1) { lowestCompletionRate = rate; }
                }
            }

            if (SaveGameManager.instance) { SaveGameManager.instance.Save(false, true); }
        }
    }
}
