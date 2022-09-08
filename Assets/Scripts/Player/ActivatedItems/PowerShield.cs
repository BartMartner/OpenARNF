using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerShield", menuName = "Player Activated Items/Power Shield", order = 1)]
public class PowerShield : PlayerActivatedItem
{
    private bool _active;
    private float _timer;
    private PlayerAura _shieldInstance;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        var shieldPrefab = Resources.Load<PlayerAura>("ItemFX/PowerShield");
        _shieldInstance = Instantiate(shieldPrefab, _player.transform);
        _shieldInstance.transform.localPosition = Vector3.zero;
        _shieldInstance.gameObject.SetActive(false);
        _shieldInstance.team = player.team;
    }

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (_active)
        {
            Deactivate();
        }
        else if (Usable())
        {
            Activate();
        }
    }

    public override void Update()
    {
        base.Update();

        if (_active)
        {
            _timer += Time.deltaTime;

            if (_timer > 1)
            {
                _player.energy -= energyCost;
                _timer -= 1;
            }

            if (!Usable())
            {
                Deactivate();
            }
        }
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        _active = false;
        _shieldInstance.HideDestroy();
        return base.Unequip(position, parent, setJustSpawned);
    }

    public void Activate()
    {
        _active = true;        
        _shieldInstance.Show();
    }

    public void Deactivate()
    {        
        _active = false;
        _shieldInstance.Hide();
    }

    public void OnDestroy()
    {
        if (_shieldInstance) { Destroy(_shieldInstance.gameObject, 1); }
    }
}

