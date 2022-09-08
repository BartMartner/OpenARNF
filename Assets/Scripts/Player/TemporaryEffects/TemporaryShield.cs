using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryShield : TemporaryPlayerEffect
{
    private bool _active;
    private float _timer;
    private PlayerAura _shieldInstance;

    public override void Equip(Player player, float duration)
    {
        base.Equip(player, duration);

        var existingShield = player.GetComponent<TemporaryShield>();
        if (existingShield != this)
        {
            Destroy(this);
            return;
        }

        var shieldPrefab = Resources.Load<PlayerAura>("ItemFX/PowerShield");
        _shieldInstance = Instantiate(shieldPrefab, _player.transform);
        _shieldInstance.transform.localPosition = Vector3.zero;
        _shieldInstance.gameObject.SetActive(false);
        _shieldInstance.Show();
    }

    public override void Unequip()
    {
        _shieldInstance.HideDestroy();
        base.Unequip();
    }
}
