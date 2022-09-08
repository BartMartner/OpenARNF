using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MajorItemRoulettePickUp : PickUp
{
    public float respawnTime = 10f;
    public float cycleTime = 0.5f;
    public MajorItem currentItemType;
    public MajorItem[] possibleItems;
    new public GameObject light;

    private float _cycleTimer;
    private float _respawnTimer;
    private bool _active = true;
    private int _currentIndex;

    public void Start()
    {
        if (DeathmatchManager.instance && 
            SaveGameManager.deathmatchSettings != null && 
            !SaveGameManager.deathmatchSettings.spawnRoomItems)
        {
            Destroy(gameObject);
        }

        SetItem();
        _respawnTimer = respawnTime;
        _cycleTimer = cycleTime;
        _active = true;
    }

    public void Update()
    {
        if (PauseMenu.instance && PauseMenu.instance.visible) { return; }

        if (_active)
        {
            if (_cycleTimer > 0)
            {
                _cycleTimer -= Time.deltaTime;
            }
            else
            {
                _cycleTimer = cycleTime;
                _currentIndex = (_currentIndex + 1) % possibleItems.Length;
                SetItem();
            }
        }
        else
        {
            if (_respawnTimer > 0)
            {
                _respawnTimer -= Time.deltaTime;
            }
            else if(!_spriteRenderer.isVisible)
            {
                FXManager.instance.SpawnFX(FXType.AnimeSplode, transform.position, true);
                _respawnTimer = respawnTime;
                _active = true;
                _spriteRenderer.color = Color.white;
                light.SetActive(true);
            }
        }
    }

    public void SetItem()
    {
        currentItemType = possibleItems[_currentIndex];
        _spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Items/" + currentItemType.ToString());
    }

    public override void OnPickUp(Player player)
    {
        if (onPickUp != null) { onPickUp.Invoke(player); }

        MajorItemInfo itemInfo = new MajorItemInfo() { fullName = "Error", description = "Error" };
        ItemManager.items.TryGetValue(currentItemType, out itemInfo);
        if (ItemCollectScreen.instance)
        {
            ItemCollectScreen.instance.Show(itemInfo);
        }
        else if (pickUpSound)
        {
            AudioManager.instance.PlayOneShot(pickUpSound);

            if(DeathmatchManager.instance)
            {
                DeathmatchManager.instance.ui[player.playerId].ShowNotification(itemInfo.fullName);
            }
        }

        player.CollectMajorItem(currentItemType);

        if (respawnTime <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            _active = false;
            _spriteRenderer.color = Color.clear;
            light.SetActive(true);
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_active) return;

        base.OnTriggerEnter2D(collision);
    }
}
