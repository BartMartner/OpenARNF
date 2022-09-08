using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;

public class NewGameLoadingScreen : MonoBehaviour
{
    IEnumerator Start()
    {
        var activeGame = SaveGameManager.activeGame;
        if (activeGame == null)
        {
            Debug.LogError("You should call SaveGameManager.instance.NewGame before starting this scene!");
        }

        SeedParameters parameters = null;
        if (!string.IsNullOrEmpty(activeGame.password) && !SeedHelper.SecretPasswords.Contains(activeGame.password))
        {
            parameters = SeedHelper.KeyToParameters(activeGame.password);
            if (parameters == null)
            {
                Debug.LogError("NewGameLoadingScreen passed an invalid seed");
            }
            else
            {
                activeGame.gameMode = parameters.gameMode;
            }
        }

        var layoutGeneration = new LayoutGenerator();
        layoutGeneration.stepByStep = true;
        activeGame.layout = new RoomLayout();
        activeGame.layout.gameMode = activeGame.gameMode;

        if (parameters != null)
        {
            var now = System.DateTime.Now;
            if (UnityEngine.Random.value < 0.05f && now.Month == 10 && now.Day == 31)
            {
                activeGame.gameMode = GameMode.Spooky;
                activeGame.layout.gameMode = GameMode.Spooky;
            }
        }

        var stepStartTime = Time.realtimeSinceStartup;
        var tolerance = 1 / 15f;

        var activeSlot = SaveGameManager.activeSlot;
        float glitchChance = 0;
        if (activeGame.gameMode == GameMode.Normal)
        {
            glitchChance = Mathf.Clamp(activeSlot.megaBeastKills, 0, 6) / (120f);
            ushort coreKills;
            if (activeSlot.bossKills.TryGetValue(BossName.MegaBeastCore, out coreKills))
            {
                glitchChance += Mathf.Clamp((float)coreKills, 0f, 20f) * (1f / 40f);
            }
        }

        Debug.Log("Glitch Chance: " + glitchChance);

        var seed = activeGame.seed;
        var achievements = SaveGameManager.activeSlot.achievements;
        List<MajorItem> traversalItems = null;

        if (parameters != null)
        {
            seed = parameters.seed;
            activeGame.seed = seed;
            traversalItems = parameters.traversalItems;
            if (activeGame.raceMode)
            {
                //all achievements unlocked
                var allAchievements = Enum.GetValues(typeof(AchievementID)).Cast<AchievementID>().ToList();
                //except for those associated with the selected mode
                var modeAchievements = SeedHelper.GetAchievementList(activeGame.gameMode);
                allAchievements.RemoveAll(a => modeAchievements.Contains(a));
                //those are set by the seed
                achievements = SeedHelper.MergeAchievements(allAchievements, parameters.achievements);
                //and all starting item achievements are removed
                achievements.Remove(AchievementID.FastBot);
                achievements.Remove(AchievementID.FightBot);
                achievements.Remove(AchievementID.GunBot);
                achievements.Remove(AchievementID.OrbBot);
                achievements.Remove(AchievementID.ThoroughBot);
            }
            else
            {
                achievements = SeedHelper.MergeAchievements(achievements, parameters.achievements);
            }
        }

        if (activeGame.gameMode == GameMode.ClassicBossRush)
        {
            foreach (var step in layoutGeneration.ClassicBossRush(seed, activeGame.layout, achievements))
            {
                if (Time.realtimeSinceStartup - stepStartTime > tolerance)
                {
                    stepStartTime = Time.realtimeSinceStartup;
                    yield return null;
                }
            }
        }
        else if (activeGame.gameMode == GameMode.BossRush)
        {
            foreach (var step in layoutGeneration.BossRush(seed, activeGame.layout, achievements))
            {
                if (Time.realtimeSinceStartup - stepStartTime > tolerance)
                {
                    stepStartTime = Time.realtimeSinceStartup;
                    yield return null;
                }
            }
        }
        else
        {
            foreach (var step in layoutGeneration.PopulateLayout(seed, activeGame.layout, achievements, traversalItems, SaveGameManager.activeSlot.itemsCollected, null, glitchChance))
            {
                if (Time.realtimeSinceStartup - stepStartTime > tolerance)
                {
                    stepStartTime = Time.realtimeSinceStartup;
                    yield return null;
                }
            }
        }

        if (activeGame.layout.startingMinorItem != MinorItemType.None)
        {
            switch(activeGame.layout.startingMinorItem)
            {
                case MinorItemType.AttackModule:
                    activeGame.attackUpsCollected++;
                    break;
                case MinorItemType.BlueScrap:
                    activeGame.blueScrap++;
                    break;
                case MinorItemType.DamageModule:
                    activeGame.damageUpsCollected++;
                    break;
                case MinorItemType.EnergyModule:
                    activeGame.energyUpsCollected++;
                    break;
                case MinorItemType.GreenScrap:
                    activeGame.greenScrap++;
                    break;
                case MinorItemType.HealthTank:
                    activeGame.healthUpsCollected++;
                    break;
                case MinorItemType.RedScrap:
                    activeGame.redScrap++;
                    break;
                case MinorItemType.SpeedModule:
                    activeGame.speedUpsCollected++;
                    break;
                case MinorItemType.ShotSpeedModule:
                    activeGame.shotSpeedUpsCollected++;
                    break;
            }
        }

        if(activeGame.layout.startingActivatedItem != MajorItem.None)
        {
            activeGame.itemsCollected.Add(activeGame.layout.startingActivatedItem);
            activeGame.currentActivatedItem = activeGame.layout.startingActivatedItem;
        }

        if(activeGame.layout.startingPassiveItem != MajorItem.None)
        {
            activeGame.itemsCollected.Add(activeGame.layout.startingPassiveItem);
            activeGame.itemsPossessed.Add(activeGame.layout.startingPassiveItem);
        }

        if (activeGame.layout.startingWeapon != MajorItem.None)
        {
            activeGame.itemsCollected.Add(activeGame.layout.startingWeapon);
            activeGame.itemsPossessed.Add(activeGame.layout.startingWeapon);
        }

        if (activeGame.layout.startingFollower != MajorItem.None)
        {
            activeGame.itemsCollected.Add(activeGame.layout.startingFollower);
            activeGame.itemsPossessed.Add(activeGame.layout.startingFollower);
        }

        var seededModes = new GameMode[] { GameMode.Normal, GameMode.BossRush, GameMode.Exterminator, GameMode.MegaMap };
        if(seededModes.Contains(activeGame.gameMode) && !string.IsNullOrEmpty(activeGame.layout.password))
        {
            activeSlot.AddSeedToPastSeeds(activeGame.layout.password);
        }

        SaveGameManager.instance.Save(true, true);
        SceneManager.LoadScene("MainScene");
    }
}