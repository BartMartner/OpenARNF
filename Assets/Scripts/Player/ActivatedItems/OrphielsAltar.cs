using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "OrphielsAltar", menuName = "Player Activated Items/Orphiel's Altar", order = 1)]
public class OrphielsAltar : PlayerActivatedItem
{
    public float holdDelay = 1;
    public AudioClip sound;
    private float _timer;

    public override void ButtonDown()
    {
        base.ButtonDown();
        SacrificeNanobot();
    }

    public override void Button()
    {
        base.Button();

        _timer += Time.deltaTime;
        if (_timer > holdDelay)
        {
            SacrificeNanobot();
        }
    }

    public void SacrificeNanobot()
    {
        if (Usable())
        {
            _timer = 0;
            var n = _player.nanobots.FirstOrDefault();
            if(n)
            {
                n.OnDamageEnemey();
                int roll = Random.Range(0,3);
                switch(roll)
                {
                    case 0:
                        _player.AddTempStatMod(PlayerStatType.Attack, Constants.defaultTempBuffRank);
                        break;
                    case 1:
                        _player.AddTempStatMod(PlayerStatType.Damage, Constants.defaultTempBuffRank);
                        break;
                    case 2:
                        _player.AddTempStatMod(PlayerStatType.Speed, Constants.defaultTempBuffRank);
                        break;
                }
            }
        }
    }

    public override bool Usable()
    {
        return _player.nanobots.Count > 0;
    }
}
