using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatsScreenState : ScreenState
{
    public ScreenState previousState;

    public Text totalRuns;
    public Text totalDeaths;
    public Text victories;
    public Text bestStreak;
    public Text currentStreak;
    public Text bestTime;
    public Text bossRushBestTime;
    public Text exterminatorBestTime;
    public Text megaMapBestTime;
    public Text highestCompletion;
    public Text lowestCompletion;
    public Text mapSpaces;
    public Text itemsPurchased;
    public Text blocksDestroyed;    
    public Text grayScrapCollected;
    public Text grayScrapSpent;
    public Text grayScrapDonated;
    public Text redScrapCollected;
    public Text redScrapSpent;
    public Text redScrapDonated;
    public Text greenScrapCollected;
    public Text greenScrapSpent;
    public Text greenScrapDonated;
    public Text blueScrapCollected;
    public Text blueScrapSpent;
    public Text blueScrapDonated;

    public Text totalKills;
    public Text championKills;
    public Text megabeastKills;
    public Text megabeastCoreKills;
    public UILabelAmount tutorialSmithKills;
    public UILabelAmount[] bosses;

    public override void OnEnable()
    {
        base.OnEnable();
        SetText();
    }

    public override void ReadyUpdate()
    {
        if (_controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            GoToState(previousState);
        }
    }

    public void SetText()
    {
        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null)
        {
            totalRuns.text = activeSlot.totalRuns.ToString();
            totalDeaths.text = activeSlot.totalDeaths.ToString();
            victories.text = activeSlot.victories.ToString();

            if(activeSlot.bestTime > 0)
            {
                var t = TimeSpan.FromSeconds(activeSlot.bestTime);
                bestTime.text = t.Hours.ToString("00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
            }
            else
            {
                bestTime.text = "-";
            }

            if (activeSlot.bossRushBestTime > 0)
            {
                var t = TimeSpan.FromSeconds(activeSlot.bossRushBestTime);
                bossRushBestTime.text = t.Hours.ToString("00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
            }
            else
            {
                bossRushBestTime.text = "-";
            }

            if (activeSlot.exterminatorBestTime > 0)
            {
                var t = TimeSpan.FromSeconds(activeSlot.exterminatorBestTime);
                exterminatorBestTime.text = t.Hours.ToString("00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
            }
            else
            {
                exterminatorBestTime.text = "-";
            }

            if (activeSlot.megaMapBestTime > 0)
            {
                var t = TimeSpan.FromSeconds(activeSlot.megaMapBestTime);
                megaMapBestTime.text = t.Hours.ToString("00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
            }
            else
            {
                megaMapBestTime.text = "-";
            }

            currentStreak.text = activeSlot.currentStreak.ToString();
            bestStreak.text = activeSlot.bestStreak.ToString();
            highestCompletion.text = activeSlot.highestCompletionRate > 0 ? string.Format("{0:N2}", activeSlot.highestCompletionRate * 100) : "-";
            lowestCompletion.text = activeSlot.lowestCompletionRate >= 0 ? string.Format("{0:N2}", activeSlot.lowestCompletionRate * 100) : "-";
            mapSpaces.text = activeSlot.mapSpacesUncovered.ToString();
            itemsPurchased.text = activeSlot.totalItemsPurchased.ToString();
            blocksDestroyed.text = activeSlot.totalBlocksDestroyed.ToString();

            long amount;
            grayScrapCollected.text = activeSlot.scrapCollected.TryGetValue(CurrencyType.Gray, out amount) ? amount.ToString() : "0";
            grayScrapSpent.text = activeSlot.scrapSpent.TryGetValue(CurrencyType.Gray, out amount) ? amount.ToString() : "0";
            grayScrapDonated.text = activeSlot.scrapDonated.TryGetValue(CurrencyType.Gray, out amount) ? amount.ToString() : "0";

            redScrapCollected.text = activeSlot.scrapCollected.TryGetValue(CurrencyType.Red, out amount) ? amount.ToString() : "0";
            redScrapSpent.text = activeSlot.scrapSpent.TryGetValue(CurrencyType.Red, out amount) ? amount.ToString() : "0";
            redScrapDonated.text = activeSlot.scrapDonated.TryGetValue(CurrencyType.Red, out amount) ? amount.ToString() : "0";

            greenScrapCollected.text = activeSlot.scrapCollected.TryGetValue(CurrencyType.Green, out amount) ? amount.ToString() : "0";
            greenScrapSpent.text = activeSlot.scrapSpent.TryGetValue(CurrencyType.Green, out amount) ? amount.ToString() : "0";
            greenScrapDonated.text = activeSlot.scrapDonated.TryGetValue(CurrencyType.Green, out amount) ? amount.ToString() : "0";

            blueScrapCollected.text = activeSlot.scrapCollected.TryGetValue(CurrencyType.Blue, out amount) ? amount.ToString() : "0";
            blueScrapSpent.text = activeSlot.scrapSpent.TryGetValue(CurrencyType.Blue, out amount) ? amount.ToString() : "0";
            blueScrapDonated.text = activeSlot.scrapDonated.TryGetValue(CurrencyType.Blue, out amount) ? amount.ToString() : "0";            

            #region Kills
            ushort kills;
            totalKills.text = activeSlot.totalKills.ToString();
            championKills.text = activeSlot.championsKilled.ToString();
            megabeastKills.text = activeSlot.megaBeastKills.ToString();
            megabeastCoreKills.text = activeSlot.bossKills.TryGetValue(BossName.MegaBeastCore, out kills) ? kills.ToString() : "0";
            
            if(activeSlot.bossKills.TryGetValue(BossName.GlitchBoss, out kills))
            {
                tutorialSmithKills.gameObject.SetActive(true);
                tutorialSmithKills.amount.text = kills.ToString();
            }
            else
            {
                tutorialSmithKills.gameObject.SetActive(false);
            }

            var bossKeySet = new HashSet<BossName>();
            bossKeySet.AddRange(Constants.surfaceBosses);
            if (activeSlot.achievements.Contains(AchievementID.ForestSlums)) { bossKeySet.AddRange(Constants.forestSlumsBosses); }
            if (activeSlot.achievements.Contains(AchievementID.CoolantSewers)) { bossKeySet.AddRange(Constants.coolantSewersBosses); }
            if (activeSlot.achievements.Contains(AchievementID.CrystalMines)) { bossKeySet.AddRange(Constants.crystalMinesBosses); }
            bossKeySet.AddRange(Constants.caveBosses);
            bossKeySet.AddRange(Constants.factoryBosses);
            bossKeySet.AddRange(Constants.buriedCityBosses);
            var bossKeys = bossKeySet.ToArray();

            for (int i = 0; i < bosses.Length; i++)
            {
                var entry = bosses[i];
                if(i < bossKeys.Length)
                {
                    entry.gameObject.SetActive(true);

                    var key = bossKeys[i];
                    entry.label.text = key.ToParsedString();
                    entry.amount.text = activeSlot.bossKills.TryGetValue(key, out kills) ? kills.ToString() : "0";
                }
                else
                {
                    entry.gameObject.SetActive(false);
                }
            }

            #endregion
        }
    }
}
