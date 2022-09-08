using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheFleshening : RoomBasedRandom
{
    public bool test;
    public override void Randomize()
    {
        base.Randomize();

        var spawnChance = test ? 1 : 0f;

        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null)
        {
            if (activeSlot.achievements.Contains(AchievementID.TheFleshening))
            {
                spawnChance += 0.40f;
            }

            if (activeSlot.achievements.Contains(AchievementID.TheFlesheningII))
            {
                spawnChance += 0.35f;
            }
        }

        if (spawnChance == 0 || (SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.Exterminator))
        {
            Destroy(gameObject);
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (_random.Value() >= spawnChance)
                {
                    Destroy(transform.GetChild(i).gameObject);
                    return;
                }
            }
        }
    }
}
