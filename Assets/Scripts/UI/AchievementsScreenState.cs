using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsScreenState : GridMenuScreenState
{
    private List<AchievementID> _achievements = new List<AchievementID>();

    protected override IList _collection
    {
        get
        {
            return _achievements;
        }
    }

    public void Awake()
    {
        foreach (var kvp in AchievementManager.achievements)
        {
            _achievements.Add(kvp.Key);
        }
    }

    public override string GetName(int index)
    {
        if (_achievements != null && index < _achievements.Count)
        {
            return AchievementManager.achievements[_achievements[index]].name.ToUpperInvariant();
        }
        else
        {
            return string.Empty;
        }
    }

    public override string GetDescriptor(int index)
    {
        if (_achievements != null && index < _achievements.Count)
        {
            return AchievementManager.achievements[_achievements[index]].collectMeans.ToUpperInvariant();
        }
        else
        {
            return string.Empty;
        }
    }

    public override Sprite GetSprite(int collectionIndex)
    {
        var achievementInfo = AchievementManager.achievements[_achievements[collectionIndex]];

        if (achievementInfo.associatedItem == MajorItem.None)
        {
            return Resources.Load<Sprite>("Sprites/AchievementIcons/" + achievementInfo.achievementID.ToString());
        }
        else
        {
            return Resources.Load<Sprite>("Sprites/Items/" + achievementInfo.associatedItem.ToString());
        }
    }

    public override bool GetEnabled(int collectionIndex)
    {
        var activeSlot = SaveGameManager.activeSlot;

        if(activeSlot != null)
        {
            return activeSlot.achievements.Contains(_achievements[collectionIndex]);
        }
        else
        {
            return false;
        }
    }

    public override bool GetActive(int index)
    {
        var activeSlot = SaveGameManager.activeSlot;
        var achievement = _achievements[index];

        if (activeSlot != null && activeSlot.achievements.Contains(achievement))
        {
            return true;
        }
        else
        {
            var achievementInfo = AchievementManager.achievements[achievement];
            return !achievementInfo.hidden;
        }
    }
}
