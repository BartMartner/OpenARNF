using UnityEngine;
using System.Collections;

public class MajorItemPickUp : PickUp
{
    public MajorItem itemType;

    public override void OnPickUp(Player player)
    {
        if (onPickUp != null) { onPickUp.Invoke(player); }

        MajorItemInfo itemInfo = new MajorItemInfo() { fullName = "Error", description = "Error" };
        ItemManager.items.TryGetValue(itemType, out itemInfo);
        if (ItemCollectScreen.instance)
        {
            ItemCollectScreen.instance.Show(itemInfo);
        }
        else if (pickUpSound)
        {
            AudioManager.instance.PlayOneShot(pickUpSound);
        }
        
        player.CollectMajorItem(itemType);
        Destroy(gameObject);
    }
}
