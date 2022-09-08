using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CloakingDevice", menuName = "Player Activated Items/Cloaking Device", order = 1)]
public class CloakingDevice : PlayerActivatedItem
{
    private bool _active;
    private float _timer;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
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

            if (!base.Usable())
            {
                Deactivate();
            }
        }
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        if (_active) { Deactivate(); }
        return base.Unequip(position, parent, setJustSpawned);
    }

    public void Activate()
    {
        if (_player && !_player.cloaked)
        {
            _active = true;
            _player.Cloak();
        }
    }

    public void Deactivate()
    {
        if (_player) { _player.Decloak(_active); }
        _active = false;
    }

    public void OnDestroy() { Deactivate(); }

    public override bool Usable()
    {
        return base.Usable() && _player && !_player.cloaked;
    }
}
