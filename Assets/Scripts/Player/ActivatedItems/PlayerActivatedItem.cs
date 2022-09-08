using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActivatedItem : ScriptableObject
{
    public Sprite icon;
    public MajorItem item;
    public float energyCost;

    protected Player _player;

    public virtual void Update() { }
    public virtual void ButtonDown() { }
    public virtual void Button() { }
    public virtual void ButtonUp() { }

    public virtual ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        if (_player)
        {
            _player.activatedItem = null;
            var pickUp = Instantiate(Resources.Load<ActivatedItemPickUp>("MajorItemPickUps/" + item.ToString()), position, Quaternion.identity, parent);
            pickUp.justSpawned = setJustSpawned;
            Destroy(this);
            return pickUp;
        }

        return null;
    }

    public virtual void Initialize(Player player)
    {
        _player = player;
    }

    public virtual bool Usable()
    {
        return _player.energy >= energyCost && (!NPCDialogueManager.instance || !NPCDialogueManager.instance.dialogueActive);
    }
}
