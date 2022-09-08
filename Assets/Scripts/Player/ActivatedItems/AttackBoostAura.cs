using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackBoostAura", menuName = "Player Activated Items/Attack Boost Aura", order = 1)]
public class AttackBoostAura : PlayerActivatedItem
{
    public PlayerAura auraPrefab;
    public int bonusAttack = 2;
    private bool _active;
    private float _timer;
    private PlayerAura _auraInstance;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _auraInstance = Instantiate(auraPrefab, _player.transform);
        _auraInstance.gameObject.SetActive(false);
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

        if(_active)
        {
            _timer += Time.deltaTime;

            if(_timer > 1)
            {
                _player.energy -= energyCost;
                _timer -= 1;
            }

            if(!Usable())
            {
                Deactivate();
            }
        }
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        if (_active) { _player.bonusAttack -= bonusAttack; }
        _active = false;
        _auraInstance.HideDestroy();
        return base.Unequip(position, parent, setJustSpawned);
    }

    public void Activate()
    {
        _active = true;
        _player.bonusAttack += bonusAttack;
        _auraInstance.Show();
    }

    public void Deactivate()
    {
        if(_active) _player.bonusAttack -= bonusAttack;
        _active = false;
        _auraInstance.Hide();
    }

    public void OnDestroy()
    {
        if (_player && _active) { _player.bonusAttack -= bonusAttack; }
        if (_auraInstance) { Destroy(_auraInstance.gameObject, 1); }
    }
}
