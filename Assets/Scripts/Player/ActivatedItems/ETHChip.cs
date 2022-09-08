using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ETHChip", menuName = "Player Activated Items/ETH Chip", order = 1)]
public class ETHChip : PlayerActivatedItem
{    
    public float holdDelay = 1;
    public AudioClip sound;
    private float _timer;

    public override void ButtonDown()
    {
        base.ButtonDown();
        TryHealPlayer();
    }

    public override void Button()
    {
        base.Button();

        _timer += Time.deltaTime;
        if (_timer > holdDelay)
        {
            TryHealPlayer();
        }
    }

    public void TryHealPlayer()
    {
        if(Usable())
        {
            _timer = 0;
            _player.energy -= energyCost;
            _player.GainHealth(1);
            if (sound)
            {
                _player.PlayOneShot(sound);
            }
        }
    }

    public override bool Usable()
    {
        return _player.health < _player.maxHealth && _player.energy >= energyCost;
    }
}
