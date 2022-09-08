using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChampionStats", menuName = "Champion Stats/Create Champion Stats", order = 1)]
public class ChampionStats : ScriptableObject
{
    public AchievementID[] requiredAchievements;
    public float dropMinorItemChance;
    public MinorItemType[] minorItemDrops;
    public int dropBonus;
    public float scrapChanceBonus;
    public GameMode prohibittedGameModes;
}
