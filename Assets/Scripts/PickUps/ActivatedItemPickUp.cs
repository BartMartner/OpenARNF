using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatedItemPickUp : PickUp
{
    public MajorItem itemType;
    public Action<ActivatedItemPickUp> onReplaced;
    private bool _justSpawned;
    public bool justSpawned
    {
        get { return _justSpawned; }
        set
        {
            _justSpawned = value;
            if(_spriteRenderer)
            {
                _spriteRenderer.color = _justSpawned ? new Color(1,1,1,0.5f) : Color.white;
            }
        }
    }

    public override void OnPickUp(Player player)
    {
        if (!justSpawned)
        {
            if (onPickUp != null)
            {
                onPickUp.Invoke(player);
            }

            if (!PlayerManager.instance.itemsCollected.Contains(itemType))
            {
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
            }
            else if (pickUpSound)
            {
                AudioManager.instance.PlayOneShot(pickUpSound);
            }

            var parentRoom = LayoutManager.CurrentRoom ?? GetComponentInParent<Room>();
            var roomID = parentRoom && parentRoom.roomAbstract != null ? parentRoom.roomAbstract.roomID : string.Empty;
            var activeGame = SaveGameManager.activeGame;

            if (activeGame != null)
            {
                List<LooseItemData> looseItems;    
                if(activeGame.looseItems.TryGetValue(roomID, out looseItems))
                {
                    if(looseItems.RemoveAll((i) => i.item == itemType) > 0)
                    {
                        activeGame.looseItems[roomID] = looseItems;
                        SaveGameManager.instance.Save();
                    }
                }
            }

            if (player.activatedItem)
            {
                var replacement = player.DropEquippedActiveItem(transform.position, parentRoom);
                if(replacement && onReplaced != null)
                {
                    onReplaced(replacement);
                }
            }

            player.CollectMajorItem(itemType);

            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            justSpawned = false;
        }
    }
}

[Serializable]
public class LooseItemData
{
    public string roomID;
    public MajorItem item;
    public Vector3 position;
}
