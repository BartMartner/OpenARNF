using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public SeedParameters parameters;
    public string seedKey;
    public string traversalKey;    
    public string achievementKey;
    public string parametersKey;

    public void ConvertKeyToTraversalItems()
    {
        var items = SeedHelper.KeyToTraversalItemList(traversalKey, parameters.gameMode);
        if(items == null)
        {
            Debug.LogError("Traversal Key invalid!");
            return;
        }
        parameters.traversalItems = items;
    }

    public void ConvertSeedToKey()
    {
        seedKey = SeedHelper.ConvertToBaseK((ulong)parameters.seed, SeedHelper.baseDefinition).PadLeft(6, SeedHelper.baseDefinition[0]); ;
    }

    public void ConverKeyToSeed()
    {
        parameters.seed = (int)SeedHelper.ConvertToBase10(seedKey, SeedHelper.baseDefinition);
    }

    public void ConvertTraversalItemsToKey()
    {
        traversalKey = SeedHelper.TraversalItemListToKey(parameters.traversalItems, parameters.gameMode);
    }

    public void ConvertAchievementsToKey()
    {
        achievementKey = SeedHelper.AchievementsToKey(parameters.achievements, parameters.gameMode);
    }

    public void ConvertKeyToAchievements()
    {
        parameters.achievements = SeedHelper.KeyToAchievements(achievementKey, parameters.gameMode);
    }

    public void ConvertParametersToKey()
    {
        parametersKey = SeedHelper.ParametersToKey(parameters);
    }

    public void ConvertKeyToParameters()
    {
        var p = SeedHelper.KeyToParameters(parametersKey);
        if (p == null)
        {
            Debug.LogError("Parameters Key invalid!");
            return;
        }
        parameters = p;
    }

    public void CountNonItemAchievements()
    {
        var achievements = SeedHelper.GetAchievementList(parameters.gameMode);
        var sb = new StringBuilder();
        sb.AppendLine("Non-Item achievement count: " + achievements.Count);
        for (int i = 0; i < achievements.Count; i++)
        {
            sb.AppendLine(i + " " + achievements[i]);    
        }
        Debug.Log(sb.ToString());
    }

    public void ScrapCostReport()
    {
        int redScrap = 0;
        int greenScrap = 0;
        int blueScrap = 0;
        int justRed = 0;
        int justGreen = 0;
        int justBlue = 0;
        int redBlue = 0;
        int redGreen = 0;
        int greenBlue = 0;
        int all = 0;
        int gunSmith = 0;
        int orbSmith = 0;
        int artificer = 0;

        foreach (var item in ItemManager.items)
        {
            if(item.Value.isTraversalItem)
            {
                continue;
            }

            if (item.Value.gunSmithPool) gunSmith++;
            if (item.Value.artificerPool) artificer++;
            if (item.Value.orbSmithPool) orbSmith++;

            redScrap += item.Value.redScrapCost;
            greenScrap += item.Value.greenScrapCost;
            blueScrap += item.Value.blueScrapCost;

            if(item.Value.redScrapCost > 0)
            {
                if (item.Value.greenScrapCost > 0)
                {
                    if (item.Value.blueScrapCost > 0)
                    {
                        all++;
                    }
                    else
                    {
                        redGreen++;
                    }
                }
                else if (item.Value.blueScrapCost > 0)
                {
                    redBlue++;
                }
                else
                {
                    justRed++;
                }
            }
            else if(item.Value.greenScrapCost > 0)
            {
                if(item.Value.blueScrapCost > 0)
                {
                    greenBlue++;
                }
                else
                {
                    justGreen++;
                }
            }
            else if (item.Value.blueScrapCost > 0)
            {
                justBlue++;
            }
        }

        Debug.Log("Gun Smith Items: " + gunSmith);
        Debug.Log("Orb Smith Items: " + orbSmith);
        Debug.Log("Artificer Items: " + artificer);

        Debug.Log("Total Red Scrap: " + redScrap);
        Debug.Log("Total Green Scrap: " + greenScrap);
        Debug.Log("Total Blue Scrap: " + blueScrap);
        Debug.Log("Just Red Scrap: " + justRed);
        Debug.Log("Just Green Scrap: " + justGreen);
        Debug.Log("Just Blue Scrap: " + justBlue);
        Debug.Log("Red and Green " + redGreen);
        Debug.Log("Red and Blue " + redBlue);
        Debug.Log("Green and Blue " + greenBlue);
        Debug.Log("All Colors " + all);
    }

    public void AddAllAchievements()
    {
        parameters.achievements = new List<AchievementID>(SeedHelper.GetAchievementList(parameters.gameMode));
    }
}
