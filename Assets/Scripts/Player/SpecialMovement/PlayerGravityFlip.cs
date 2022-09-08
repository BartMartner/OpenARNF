using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GravityFlip", menuName = "Player Special Moves/Gravity Flip", order = 2)]
public class PlayerGravityFlip : PlayerSpecialMovement
{
    public override void Initialize(Player player)
    {
        _priority = 2;
        base.Initialize(player);
    }

    public override void DeathStop() { }

    public override bool TryToActivate()
    {
        _complete = true;

        var yAxis = _player.GetYAxis();
        var absXAxis = Mathf.Abs(_player.controller.GetAxis("Horizontal"));
        var absYAxis = Mathf.Abs(yAxis);

        if (yAxis > 0.1f && absYAxis > absXAxis)
        {
            _player.FlipGravity();
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void OnPause() { }
    public override void OnUnpause() { }
}
