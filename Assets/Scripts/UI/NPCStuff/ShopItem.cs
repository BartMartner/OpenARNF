using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : ShopOption
{
    public Image itemIcon;
    public Sprite graySprite;
    public Sprite redSprite;
    public Sprite greenSprite;
    public Sprite blueSprite;
    public Sprite questionMark;

    public List<CurrenyBlock> currencyBlocks;

    private MajorItemInfo _item;
    public MajorItemInfo item
    {
        get { return _item; }
    }

    public void AssignItem(MajorItemInfo info, ShopInfo shopInfo)
    {
        _item = info;

        itemIcon.sprite = shopInfo.hideItemIcons ? questionMark : _item.icon;
        var player = PlayerManager.instance.player1;
        var halfOff = player.itemsPossessed.Contains(MajorItem.ChromeDome);
        var costs = (new ItemPrice(info, shopInfo, halfOff)).GetCost();

        //TODO: set costs on currency blocks
        for (int i = 0; i < currencyBlocks.Count; i++)
        {
            var block = currencyBlocks[i];
            if(i < costs.Count)
            {
                var cost = costs[i];
                block.icon.gameObject.SetActive(true);
                block.text.gameObject.SetActive(true);

                block.icon.sprite = GetIcon(cost.type);
                block.text.text = cost.amount.ToString();
                block.text.color = cost.CanAfford(player) ? Color.white : new Color(0.4f,0.4f,0.4f,1);       
            }
            else
            {
                block.icon.gameObject.SetActive(false);
                block.text.gameObject.SetActive(false);
            }
        }
    }

    public Sprite GetIcon(CurrencyType currencyType)
    {
        switch(currencyType)
        {
            case CurrencyType.Gray:
                return graySprite;
            case CurrencyType.Blue:
                return blueSprite;
            case CurrencyType.Green:
                return greenSprite;
            case CurrencyType.Red:
                return redSprite;
        }
        return null;
    }
}

[Serializable]
public class CurrenyBlock
{
    public Image icon;
    public Text text;
}

public struct ItemCost
{
    public CurrencyType type;
    public int amount;

    public bool CanAfford(Player player)
    {
        if (!player) return false;

        switch (type)
        {
            case CurrencyType.Gray:
                return player.grayScrap >= amount;
            case CurrencyType.Red:
                return player.redScrap >= amount;
            case CurrencyType.Green:
                return player.greenScrap >= amount;
            case CurrencyType.Blue:
                return player.blueScrap >= amount;
        }

        return false;
    }
}
