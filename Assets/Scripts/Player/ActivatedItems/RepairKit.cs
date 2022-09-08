using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RepairKit", menuName = "Player Activated Items/Repair Kit", order = 1)]
public class RepairKit : PlayerActivatedItem
{
    private IRepairable presentRepairable;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        if(LayoutManager.instance)
        {
            OnRoomLoaded();
            LayoutManager.instance.onRoomLoaded += OnRoomLoaded;
        }
    }

    public void OnRoomLoaded()
    {
        if(LayoutManager.instance)
        {
            presentRepairable = LayoutManager.instance.currentRoom.gameObject.GetComponentInChildren<IRepairable>();
        }
    }

    public override void ButtonDown()
    {
        if (Usable() && _player)
        {
            presentRepairable.Repair();
            _player.StartCoroutine(DestroyEndOfFrame());
        }
    }

    private IEnumerator DestroyEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        if (_player && _player.activatedItem == this) { _player.activatedItem = null; }
        Destroy(this);

        if (SaveGameManager.activeGame != null) { SaveGameManager.activeGame.currentActivatedItem = MajorItem.None; }
        SaveGameManager.instance.Save();
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        if (LayoutManager.instance)
        {
            LayoutManager.instance.onRoomLoaded -= OnRoomLoaded;
        }

        return base.Unequip(position, parent, setJustSpawned);
    }

    public override bool Usable()
    {
        return presentRepairable != null && presentRepairable.CanRepair();
    }
}
