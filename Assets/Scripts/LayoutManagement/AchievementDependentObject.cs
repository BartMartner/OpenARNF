using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementDependentObject : RoomBasedRandom
{
    public AchievementID requiredAchievement;
    [Range(0f,1f)]
    private float _spawnChance = 0.5f;

    public override void Randomize()
    {
        base.Randomize();

        if (_random.Value() > _spawnChance)
        {
            Destroy(gameObject);
            return;
        }

        if (SaveGameManager.activeSlot != null)
        {
            if (!SaveGameManager.activeSlot.achievements.Contains(requiredAchievement))
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}
