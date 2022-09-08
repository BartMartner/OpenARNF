using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CollectionScreenState : GridMenuScreenState
{
    private List<MajorItem> _items = new List<MajorItem>();

    protected override IList _collection { get { return _items; } }

    public void Awake()
    {
        foreach (MajorItem item in Enum.GetValues(typeof(MajorItem)))
        {
            if (item == MajorItem.None || Constants.excludeItems.Contains(item)) { continue; }
            _items.Add(item);
        }
    }

    public override Sprite GetSprite(int collectionIndex)
    {
        return Resources.Load<Sprite>("Sprites/Items/" + _items[collectionIndex].ToString());
    }

    public override bool GetEnabled(int collectionIndex)
    {
        return SaveGameManager.activeSlot.itemsCollected.Contains(_items[collectionIndex]);
    }

    public override bool GetActive(int collectionIndex)
    {
        return true;        
    }

    public override string GetName(int collectionIndex)
    {
        if (_items != null && collectionIndex < _items.Count)
        {
            return ItemManager.items[_items[collectionIndex]].fullName;
        }
        else
        {
            return string.Empty;
        }
    }

    public override string GetDescriptor(int collectionIndex)
    {
        if (_items != null && collectionIndex < _items.Count)
        {
            var item = _items[collectionIndex];
            if (SaveGameManager.activeSlot.itemsCollected.Contains(item))
            {
                var info = ItemManager.items[item];
                return (string.IsNullOrEmpty(info.itemPageDescription) ? info.description : info.itemPageDescription).ToUpperInvariant();
            }
            else
            {
                return "???";
            }
        }
        else
        {
            return string.Empty;
        }
    }
}
