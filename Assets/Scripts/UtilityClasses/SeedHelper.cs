//#define TRUECOOP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SeedHelper
{
    public const string ClassicBossRush = "ENGAGERIDLEYMOTHERFUCKER";
    public const string Spooky = "SKELETONWARNORESTINPEACE";
    public const string MirrorWorld = "METALSTORMTHENIGHTVVVVVV";
#if TRUECOOP
    public const string TrueCoOp = "UPTOFOURROBOTSNAMEDFIGHT";
#endif

    public static readonly string[] SecretPasswords = new string[] { ClassicBossRush, Spooky, MirrorWorld, };

    public const string baseDefinition = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private static List<MajorItem> _traversalItems;
    private static List<MajorItem> _finalItems;
    public static List<MajorItem> traversalItems
    {
        get
        {
            if (_traversalItems == null) { CreateItemLists(); }
            return _traversalItems;
        }
    }

    private static void CreateItemLists()
    {
        _traversalItems = new List<MajorItem>();        

        var infos = ItemManager.items.Values.Where((i) => i.isTraversalItem).OrderBy((i) =>
        {
            //places BuzzsawGun after PhaseShell always 
            var index = (i.type == MajorItem.BuzzsawGun) ? 29.5f : (float)i.type;
            return index;
        });

        foreach (var info in infos)
        {
            _traversalItems.Add(info.type);
        }
    }

    private static List<AchievementID> _skipAchiements = new List<AchievementID>()
    {
        AchievementID.WallJump, AchievementID.Exterminator, AchievementID.MegaMap, AchievementID.BossRush,
        AchievementID.BeastGuts, AchievementID.TheFleshening, AchievementID.TheFlesheningII,
    };

    private static List<AchievementID> _megaMapSkipAchievements = new List<AchievementID>()
    {
        AchievementID.ForestSlums, AchievementID.CoolantSewers, AchievementID.CrystalMines,
        AchievementID.OrbBot, AchievementID.FightBot, AchievementID.FastBot, AchievementID.ThoroughBot,
        AchievementID.GunBot, AchievementID.TheQuickening,
    };

    private static List<AchievementID> _bossRushSkipAchievements = new List<AchievementID>()
    {
        AchievementID.OrbBot, AchievementID.FightBot, AchievementID.FastBot, AchievementID.ThoroughBot,
        AchievementID.AllyBot, AchievementID.GunBot, AchievementID.TheQuickening, AchievementID.TheTraitor,
        AchievementID.GlitchModule, AchievementID.GlitchScrap, AchievementID.RevenantStation,
    };

    private static List<AchievementID> _nonItemAchievements;
    private static List<AchievementID> _bossRushAchievements;
    private static List<AchievementID> _megaMapAchievements;

    public static int GetAchievementsPerGroup(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.MegaMap:
                return 5;
            default:
                return 10;
        }
    }

    public static int GetAchievementGroupCharCount(GameMode gameMode)
    {
        switch(gameMode)
        {
            case GameMode.MegaMap:
                return 1;
            default:
                return 2;
        }
    }

    public static List<AchievementID> GetAchievementList(GameMode gameMode)
    {
        if (_nonItemAchievements == null || _bossRushAchievements == null || _megaMapAchievements == null)
        {
            _nonItemAchievements = new List<AchievementID>();
            _bossRushAchievements = new List<AchievementID>();
            _megaMapAchievements = new List<AchievementID>();

            foreach (var a in AchievementManager.achievements)
            {
                if (a.Value.associatedItem == MajorItem.None)
                {
                    if (!_skipAchiements.Contains(a.Key))
                    {
                        _nonItemAchievements.Add(a.Key);
                        if (!_megaMapSkipAchievements.Contains(a.Key))
                        {
                            _megaMapAchievements.Add(a.Key);
                        }

                        if(!_bossRushSkipAchievements.Contains(a.Key))
                        {
                            _bossRushAchievements.Add(a.Key);
                        }
                    }
                }
                else if (a.Value.associatedBoss != BossName.None &&
                    a.Value.associatedBoss != BossName.MegaBeastCore &&
                    a.Value.associatedBoss != BossName.GlitchBoss)
                {
                    _bossRushAchievements.Add(a.Key);
                }
                //else if (a.Value.associatedItem != MajorItem.None)
                //{
                //    _bossRushAchievements.Add(a.Key);
                //}
            }
        }

        switch (gameMode)
        {
            case GameMode.BossRush:
                return _bossRushAchievements;
            case GameMode.MegaMap:
                return _megaMapAchievements;
            default:
                return _nonItemAchievements;
        }
    }

    public static string ParametersToKey(SeedParameters parameters)
    {
        return ParametersToKey(parameters.gameMode, parameters.seed, parameters.traversalItems, parameters.achievements);
    }

    public static string ParametersToKey(GameMode gameMode, int seed, List<MajorItem> traversalItems, List<AchievementID> achievements)
    {
        string gameModeKey = GameModeToKey(gameMode);
        string seedKey = ConvertToBaseK(seed, baseDefinition).PadLeft(6, baseDefinition[0]);
        string traversalKey = TraversalItemListToKey(traversalItems, gameMode);
        string achievementKey = AchievementsToKey(achievements, gameMode);

        //Mega Map : 1 + 6 + 16 + 1 characters = 24 characters
        //Boss Rush: 1 + 6 + 0 + 8 characters = 15 characters
        //Other Modes : 1 + 6 + 10 + 6 characters = 23 characters
        var key = gameModeKey + seedKey + traversalKey + achievementKey;
        key = key.PadRight(24, baseDefinition[0]);
        return key;
    }

    public static string GameModeToKey(GameMode gameMode)
    {
        switch(gameMode)
        {
            case GameMode.BossRush:
                return "B";
            case GameMode.Exterminator:
                return "E";
            case GameMode.MegaMap:
                return "M";
            case GameMode.MirrorWorld:
                return "R";
            case GameMode.Spooky:
                return "4";
            default:
                return "A";
        }
    }

    public static GameMode KeyToGameMode(string key)
    {
        switch (key)
        {
            case "B":
                return GameMode.BossRush;
            case "E":
                return GameMode.Exterminator;
            case "M":
                return GameMode.MegaMap;
            case "R":
                return GameMode.MirrorWorld;
            case "4":
                return GameMode.Spooky;
            default:
                return GameMode.Normal;
        }
    }

    public static SeedParameters KeyToParameters(string key)
    {
        string gameModeKey = key.Substring(0, 1);
        var gameMode = KeyToGameMode(gameModeKey);

        string seedKey = key.Substring(1, 6);

        var traversalKeyLength = GetTraversalItemKeyLength(gameMode);
        string traversalKey = key.Substring(7, traversalKeyLength);

        var aCount = GetAchievementGroupCount(gameMode);
        var startIndex = 7 + traversalKeyLength;
        var achvGroupCharCount = GetAchievementGroupCharCount(gameMode);
        string achievementKey = key.Substring(startIndex, aCount*achvGroupCharCount);

        var parameters = new SeedParameters()
        {
            gameMode = gameMode,
            seed = (int)ConvertToBase10(seedKey, baseDefinition),
            achievements = KeyToAchievements(achievementKey, gameMode),
        };

        if(gameMode != GameMode.BossRush)
        {
            parameters.traversalItems = KeyToTraversalItemList(traversalKey, gameMode);
        }
        else if (parameters.achievements != null)
        {
            return parameters;
        }

        return (parameters.achievements != null && parameters.traversalItems != null) ? parameters : null;
    }

    public static int GetTraversalItemCount(GameMode gameMode)
    {
        if (gameMode == GameMode.MegaMap)
        {
            return 12;
        }
        else
        {
            return 7;
        }
    }

    public static int GetTraversalItemKeyLength(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.MegaMap:
                return 16;
            case GameMode.BossRush:
                return 0;
            default:
                return 10;
        }
    }

    public static int GetAchievementGroupCount(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.MegaMap:
                return 1;
            case GameMode.BossRush:
                return 4;
            default:
                return 3;
        }
    }

    public static string AchievementsToKey(List<AchievementID> achievements, GameMode gameMode)
    {
        var copy = new List<AchievementID>(achievements);
        var achievementList = GetAchievementList(gameMode);

        copy.RemoveAll((a) => !achievementList.Contains(a));
        var aCount = GetAchievementGroupCount(gameMode);
        var achievementGroups = new ulong[aCount];
        var achievementsPerGroup = GetAchievementsPerGroup(gameMode);
        var achievementGroupCharCount = GetAchievementGroupCharCount(gameMode);

        foreach (var achievement in copy)
        {
            var index = achievementList.IndexOf(achievement);
            var group = (index / achievementsPerGroup);
            var spot = index % achievementsPerGroup;

            if(group < aCount)
            {
                achievementGroups[group] += (ulong)(1 << spot);
            }
        }

        string key = string.Empty;
        foreach (var g in achievementGroups)
        {
            key += ConvertToBaseK(g, baseDefinition).PadLeft(achievementGroupCharCount, baseDefinition[0]);
        }

        return key;
    }

    public static List<AchievementID> KeyToAchievements(string key, GameMode gameMode)
    {
        var achievementList = GetAchievementList(gameMode);
        var achievements = new List<AchievementID>();
        var groupsCount = GetAchievementGroupCount(gameMode);
        var achvPerGroup = GetAchievementsPerGroup(gameMode);
        var achvGroupCharCount = GetAchievementGroupCharCount(gameMode);

        var groups = new int[groupsCount];

        if (key.Length != groupsCount * achvGroupCharCount) //X characters per group of Y achievements
        {
            Debug.LogWarning("Invalid Achievement Key Passed");
            return null;
        }

        for (int i = 0; i < groupsCount; i++)
        {
            groups[i] = (int)ConvertToBase10(key.Substring(i* achvGroupCharCount, achvGroupCharCount), baseDefinition);
        }

        for (int g = 0; g < groups.Length; g++)
        {
            var group = groups[g];
            for (int i = 0; i < achvPerGroup; i++)
            {
                var flag = 1 << i;
                var index = g * achvPerGroup + i;
                if ((group & flag) == flag && index < achievementList.Count)
                {
                    achievements.Add(achievementList[index]);
                }
            }
        }
        
        if(gameMode == GameMode.MegaMap)
        {
            achievements.Add(AchievementID.ForestSlums);
            achievements.Add(AchievementID.CoolantSewers);
            achievements.Add(AchievementID.CrystalMines);
        }

        return achievements;
    }

    public static string TraversalItemListToKey(List<MajorItem> items, GameMode gameMode)
    {
        if(gameMode == GameMode.BossRush) { return string.Empty; }

        if (items.Count != GetTraversalItemCount(gameMode))
        {
            Debug.LogWarning("TraversalItems passed to TraversalItemListToKey have an invalid count");
            return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (!traversalItems.Contains(item))
            {
                Debug.LogWarning("TraversalItems passed to TraversalItemListToKey contained an invalid a non-traversal item");
                return null;
            }
        }

        //only 9 indices can fit in a ulong
        decimal indices = 0;
        for (int i = 0; i < items.Count; i++)
        {
            var mod = Pow(100, i);
            var index = traversalItems.IndexOf(items[i]);
            //Debug.Log("Index of " + items[i] + " is " + index);
            indices += index * mod;
        }

        var key = ConvertToBaseK(indices, baseDefinition);

        //Debug.Log("Raw Indices: " + indices);
        
        var keyLength = GetTraversalItemKeyLength(gameMode);
        return key.PadLeft(keyLength, baseDefinition[0]);
    }

    public static List<MajorItem> KeyToTraversalItemList(string key, GameMode gameMode)
    {
        var keyLength = GetTraversalItemKeyLength(gameMode);
        var itemCount = GetTraversalItemCount(gameMode);

        if(key.Length != keyLength)
        {
            Debug.LogWarning("Key passed to KeyToTraversalItemList is not " + keyLength + " characters long");
            return null;
        }

        var items = new MajorItem[itemCount];
        var rawIndices = ConvertToBase10(key, baseDefinition);

        for (int i = itemCount-1; i >= 0; i--)
        {
            var index = (int)((rawIndices % Pow(100, i+1)) / Pow(100, i));
            if (index < traversalItems.Count)
            {
                items[i] = traversalItems[index];
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("Key converted to an invalid index (" + index + " of " + traversalItems.Count + ") for item " + i);
                sb.AppendLine("Raw Indices: " + rawIndices);
                foreach (var item in items) { sb.AppendLine(item.ToString()); }
                Debug.LogWarning(sb.ToString());
                return null;
            }
        }

        var itemList = items.ToList();

        if(itemList.Count != itemCount || itemList.Distinct().Count() != itemCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Unique Item Count " + itemList.Count + " Not Equal to " + itemCount);
            foreach (var item in itemList) { sb.AppendLine(item.ToString()); }
            Debug.LogWarning(sb.ToString());
            return null;
        }

        return itemList;
    }

    public static List<AchievementID> MergeAchievements(List<AchievementID> baseAchievements, List<AchievementID> seedAchievements)
    {
        if (seedAchievements == null || seedAchievements.Count == 0) { return new List<AchievementID>(baseAchievements); }
        if (baseAchievements == null || baseAchievements.Count == 0) { return new List<AchievementID>(seedAchievements); }

        var newList = new List<AchievementID>(baseAchievements);
        newList.Remove(AchievementID.None);
        newList.RemoveAll((a) => AchievementManager.achievements[a].associatedItem == MajorItem.None || seedAchievements.Contains(a));
        newList.AddRange(seedAchievements);
        return newList;
    }

    public static string ConvertToBaseK(decimal val, string baseDef)
    {
        string result = string.Empty;
        var targetBase = baseDef.Length;

        do
        {
            var index = val % targetBase;
            result = baseDef[(int)index] + result;
            val = decimal.Floor(val / targetBase);    
        }
        while (val > 0);

        return result;
    }

    public static decimal ConvertToBase10(string str, string baseDef)
    {
        decimal result = 0;
        decimal originalBase = baseDef.Length;
        for (int idx = 0; idx < str.Length; idx++)
        {
            var idxOfChar = baseDef.IndexOf(str[idx]);
            result += idxOfChar * Pow(originalBase, (str.Length - 1) - idx);
        }

        return result;
    }

    public static decimal Pow(decimal x, int y)
    {
        decimal result = 1;
        for (int i = 0; i < y; i++)
        {
            result *= x;
        }
        return result;
    }

    public static void StartClassicBossRush()
    {
        StartMode(GameMode.ClassicBossRush, ClassicBossRush);
    }

    public static void StartSpookyMode(bool addSeed = true)
    {
        StartMode(GameMode.Spooky, Spooky);
    }

    public static void StartMirrorWorldMode()
    {
        StartMode(GameMode.MirrorWorld, MirrorWorld);
    }

#if TRUECOOP
    public static void StartTrueCoOpMode()
    {
        StartMode(GameMode.TrueCoOp, TrueCoOp);
    }
#endif

    public static void StartMode(GameMode gameMode, string password)
    {
        if (SaveGameManager.activeSlot != null) { SaveGameManager.activeSlot.AddSeedToSecretSeeds(password); }
        InputHelper.instance.AssignPlayer1LastActiveController();
        SaveGameManager.instance.NewGame(gameMode);
        SaveGameManager.instance.Save();
        SaveGameManager.activeGame.password = password;
        UISounds.instance.Confirm();
        SceneManager.LoadScene("NewGame");
    }
}

[Serializable]
public class SeedParameters
{
    public int seed;
    public GameMode gameMode;
    public List<AchievementID> achievements;
    public List<MajorItem> traversalItems;
}
