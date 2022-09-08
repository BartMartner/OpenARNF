using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchItemSpawn : MonoBehaviour
{
    public float spawnTime = 10f;
    private PickUp _currentPickUp;
    private MajorItem _currentItem;
    private float _timer;

	void Update ()
    {
        if (!_currentPickUp)
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
            }
            else
            {
                _timer = spawnTime;
                var item = DeathmatchManager.instance.PickItemToSpawn();
                if (item != MajorItem.None)
                {
                    FXManager.instance.SpawnFX(FXType.AnimeSplode, transform.position, true);
                    _currentPickUp = Instantiate(Resources.Load<PickUp>("MajorItemPickUps/" + item.ToString()), transform.position, Quaternion.identity, transform.parent);
                    _currentPickUp.onPickUp.AddListener(OnPickUp);
                    _currentItem = item;

                    var activated = _currentPickUp.GetComponent<ActivatedItemPickUp>();
                    if (activated) { activated.onReplaced += OnReplaced; }

                    if (_currentPickUp.pickUpSound)
                    {
                        AudioManager.instance.PlayOneShot(_currentPickUp.pickUpSound, 0.5f);
                    }
                }
            }
        }
	}

    public void OnPickUp(Player player)
    {
        DeathmatchManager.instance.itemsOnTheField.Remove(_currentItem);
        DeathmatchManager.instance.ui[player.playerId].ShowNotification(ItemManager.items[_currentItem].fullName);
        _currentItem = MajorItem.None;
        _currentPickUp = null;
    }

    public void OnReplaced(ActivatedItemPickUp replacement)
    {
        _currentPickUp = replacement;
        _currentItem = replacement.itemType;
        DeathmatchManager.instance.itemsOnTheField.Add(replacement.itemType);
        replacement.onPickUp.AddListener(OnPickUp);
        replacement.onReplaced += OnReplaced;
    }
}
