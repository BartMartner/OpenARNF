using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopTriggerBounds : ButtonTriggerBounds, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public ShopInfo shopInfo;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.BossRush)
        {
            shopInfo.items.Clear();
            var items = new List<MajorItem>(roomAbstract.shopOfferings);
            foreach (var item in roomAbstract.shopOfferings)
            {
                var info = ItemManager.items[item];

                if (info.orbSmithPool)
                {
                    if (shopInfo.shopType == ShopType.OrbSmith) { shopInfo.items.Add(item); }
                }
                else if (info.gunSmithPool)
                {
                    if (shopInfo.shopType == ShopType.GunSmith) { shopInfo.items.Add(item); }
                }
                else if (info.artificerPool)
                {
                    if (shopInfo.shopType == ShopType.Artificer) { shopInfo.items.Add(item); }
                }
            }
            return;
        }

        shopInfo.items = roomAbstract.shopOfferings;
    }

    public override void OnSubmit()
    {
        UISounds.instance.Confirm();
        NPCDialogueManager.instance.ShowShopScreen(shopInfo, PlayerManager.instance.player1);
        base.OnSubmit();
    }
}
