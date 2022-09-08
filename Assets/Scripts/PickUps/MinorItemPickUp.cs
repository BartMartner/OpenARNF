using UnityEngine;
using System.Collections;

public class MinorItemPickUp : PickUp
{
    public MinorItemData data;
    public DestructibleTileBounds linkedDestructable;

    public override void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        if(linkedDestructable)
        {
            linkedDestructable.onStartDeath.AddListener(()=> gameObject.SetActive(true));
            linkedDestructable.onRegenerate.AddListener(() => gameObject.SetActive(false));
            gameObject.SetActive(false);
        }
    }

    public override void OnPickUp(Player player)
    {
        bool everCollected = false;

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            everCollected = activeGame.minorItemTypesCollected.Contains(data.type);

            if(!everCollected)
            {
                activeGame.minorItemTypesCollected.Add(data.type);
            }

            if(activeGame.minorItemIdsCollected.Contains(data.globalID) && data.globalID != -99)
            {                
                Debug.LogError("Trying to collect a minor item with a global ID that's already been collected! globalID = " + data.globalID);
                return;
            }

            if (data.globalID != -99)
            {
                activeGame.minorItemIdsCollected.Add(data.globalID);
            }
        }
        else
        {
            Debug.LogWarning("SaveGameManager.instance.activeGame == null");
        }

        ItemCollectScreen.instance.Show(ItemManager.GetMinorItemInfo(data.type), everCollected);

        if (PlayerManager.instance.trueCoOp &&
            (data.type == MinorItemType.HealthTank ||
             data.type == MinorItemType.EnergyModule))
        {
            player = PlayerManager.instance.player1;
        }

        player.CollectMinorItem(data.type);
        //^^^ SaveGameManager.instance.Save(); will Save in CollectMinorItem
        base.OnPickUp(player);
    }

}
