using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchMapScreen : GridMenuScreenState
{
    protected override IList _collection { get { return DeathmatchManager.allMaps; } }

    private bool _optionsChanged;

    public override void ReadyUpdate()
    {
        if (SaveGameManager.deathmatchSettings == null) return;

        base.ReadyUpdate();

        if (_controller.GetButtonDown("UISubmit"))
        {
            var selected = DeathmatchManager.allMaps[_activeCollectionIndices[activeIndex]];
            if (SaveGameManager.deathmatchSettings.mapRotation.Contains(selected))
            {
                if (SaveGameManager.deathmatchSettings.mapRotation.Count > 1)
                {
                    UISounds.instance.Cancel();
                    SaveGameManager.deathmatchSettings.mapRotation.Remove(selected);
                }
                else
                {
                    UISounds.instance.UIFail();
                }
            }
            else
            {
                UISounds.instance.Confirm();
                SaveGameManager.deathmatchSettings.mapRotation.Add(selected);
                SaveGameManager.deathmatchSettings.mapRotation.Sort();
            }

            _optionsChanged = true;
            Refresh();
        }
    }

    public override string GetName(int collectionIndex)
    {
        if (collectionIndex < DeathmatchManager.allMaps.Count)
        {
            var name = DeathmatchManager.allMaps[collectionIndex];
            switch(name)
            {
                case "Deathmatch01":
                    return "THE CAVES";
                case "Deathmatch02":
                    return "THE FACTORY";
                case "Deathmatch03":
                    return "THE BURIED CITY";
                case "Deathmatch04":
                    return "THE GUTS OF THE BEAST";
                case "Deathmatch05":
                    return "THE FOREST SLUMS";
                case "Deathmatch06":
                    return "THE COOLANT SEWERS";
                case "Deathmatch07":
                    return "THE CRYSTAL MINES";
                case "DeathmatchStressTest":
                    return "STRESS TEST";
                default:
                    return "UNKNOWN";
            }
            
        }
        else
        {
            return "UNKNOWN";
        }
    }

    public override string GetDescriptor(int collectionIndex)
    {
        var settings = SaveGameManager.deathmatchSettings;

        if (settings != null && collectionIndex < DeathmatchManager.allMaps.Count)
        {
            return settings.mapRotation.Contains(DeathmatchManager.allMaps[collectionIndex]) ? "Enabled" : "Disabled";
        }
        else
        {
            return "Error";
        }
    }

    public override Sprite GetSprite(int collectionIndex)
    {
        return Resources.Load<Sprite>("Sprites/DeathmatchMapThumbnails/" + DeathmatchManager.allMaps[collectionIndex]);        
    }

    protected override IEnumerator GoToStateCoroutine(ScreenState newState)
    {
        if (_optionsChanged)
        {
            SaveGameManager.instance.Save(false, true);
        }
        return base.GoToStateCoroutine(newState);
    }

    public override bool GetActive(int collectionIndex)
    {
        var achievements = SaveGameManager.activeSlot == null ? null : SaveGameManager.activeSlot.achievements;
        var valid = DeathmatchManager.ValidMaps(DeathmatchManager.allMaps, achievements);
        var map = DeathmatchManager.allMaps[collectionIndex];
        return valid.Contains(map);
    }

    public override bool GetEnabled(int collectionIndex)
    {
        var map = DeathmatchManager.allMaps[collectionIndex];
        var settings = SaveGameManager.deathmatchSettings;
        return settings != null && settings.mapRotation.Contains(map);
    }
}
