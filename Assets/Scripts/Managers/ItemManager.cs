using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public static class ItemManager
{
    private static Dictionary<MajorItem, MajorItemInfo> _items;
    public static Dictionary<MajorItem, MajorItemInfo> items
    {
        get
        {
            if (_items == null)
            {
                _items = new Dictionary<MajorItem, MajorItemInfo>();
                List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();

                try
                {
                    loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
                    loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
                    loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));
                }
                catch (Exception exc)
                {
                    _items = null;
                    Debug.LogException(exc);
                    return null;
                }

                foreach (var item in loadedItemInfos)
                {
                    if(Constants.excludeItems.Contains(item.type))
                    {
                        continue;
                    }

                    _items.Add(item.type, item);
                }
            }

            return _items;
        }
    }

    public static ItemInfo GetMinorItemInfo(MinorItemType item)
    {
        switch (item)
        {
            case MinorItemType.DamageModule:
                return new ItemInfo() { fullName = "Damage Module", description = "Damage Up!" };
            case MinorItemType.HealthTank:
                return new ItemInfo() { fullName = "Health Tank", description = "Health Up!" };
            case MinorItemType.EnergyModule:
                return new ItemInfo() { fullName = "Energy Module", description = "Energy Up!" };
            case MinorItemType.RedScrap:
                return new ItemInfo() { fullName = "Red Archaic Scrap", description = "Can be donated to shrines and forged by allies." };
            case MinorItemType.GreenScrap:
                return new ItemInfo() { fullName = "Green Archaic Scrap", description = "Can be donated to shrines and forged by allies." };
            case MinorItemType.BlueScrap:
                return new ItemInfo() { fullName = "Blue Archaic Scrap", description = "Can be donated to shrines and forged by allies." };
            case MinorItemType.AttackModule:
                return new ItemInfo() { fullName = "Attack Module", description = "Rate of Fire Up!" };
            case MinorItemType.SpeedModule:
                return new ItemInfo() { fullName = "Speed Module", description = "Speed Up!" };
            case MinorItemType.ShotSpeedModule:
                return new ItemInfo() { fullName = "Shot Speed Module", description = "Shot Speed Up!" };
            case MinorItemType.GlitchModule:
                return new ItemInfo() { fullName = "Glitch Module", description = "-1 to a random stat, +1 to two others" };
            default:
                return new ItemInfo() { fullName = "Error", description = "Error" };
        }
    }
}
