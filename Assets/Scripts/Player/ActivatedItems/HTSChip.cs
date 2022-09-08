using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HTSChip", menuName = "Player Activated Items/HTS Chip", order = 1)]
public class HTSChip : PlayerActivatedItem
{
    public float holdDelay = 1;
    public AudioClip sound;
    private float _timer;

    public override void ButtonDown()
    {
        base.ButtonDown();
        TryHurtPlayer();
    }

    public override void Button()
    {
        base.Button();

        _timer += Time.deltaTime;
        if (_timer > holdDelay)
        {
            TryHurtPlayer();
        }
    }

    public void TryHurtPlayer()
    {
        if (Usable())
        {
            _timer = 0;
            if (_player.Hurt(1))
            {
                _player.GainScrap(CurrencyType.Gray, 1);
                if (sound)
                {
                    _player.PlayOneShot(sound);
                }
            }
        }
    }

    public override bool Usable()
    {
        return _player.health > 0 && !_player.aegisActive;
    }
}
