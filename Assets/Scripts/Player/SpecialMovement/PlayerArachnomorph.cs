using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Arachnomorph", menuName = "Player Special Moves/Arachnomorph", order = 2)]
public class PlayerArachnomorph : PlayerSpecialMovement
{
    public override void Initialize(Player player)
    {
        _priority = 3;
        base.Initialize(player);
    }

    public override void DeathStop()
    {
        //Do Nothing
    }

    public override bool TryToActivate()
    {
        _complete = true;

        var yAxis = _player.GetYAxis() * (_player.spiderForm ? -1 : 1);

        var absXAxis = Mathf.Abs(_player.controller.GetAxis("Horizontal"));
        var absYAxis = Mathf.Abs(yAxis);

        if (_player.grounded && yAxis < 0.1f && absYAxis > absXAxis)
        {
            _player.ToggleSpiderForm();
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
