using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Phase", menuName = "Player Special Moves/Phase", order = 1)]
public class PlayerPhase : PlayerSpecialMovement
{
    private IEnumerator _phaseRoutine;
    private Collider2D _collider2D;
    private LayerMask _layerMask;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _priority = 4;
        _collider2D = player.collider2D;
        _layerMask = LayerMask.GetMask("Default", "Caulk");
    }

    public override bool TryToActivate()
    {
        var xAxis = _player.GetXAxis();
        var yAxis = _player.GetYAxis();
        var absXAxis = Mathf.Abs(xAxis);
        var absYAxis = Mathf.Abs(yAxis);

        if (absXAxis > 0.66f || absYAxis > 0.66f)
        {
            Vector3 direction;
            if (absXAxis > absYAxis)
            {
                direction = xAxis > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                direction = yAxis > 0 ? Vector3.up : Vector3.down;
            }

            var destination = CanPhase(direction);

            if (destination.HasValue)
            {
                _phaseRoutine = Phase(destination.Value);
                _player.StartCoroutine(_phaseRoutine);
                return true;
            }
        }

        return false;
    }


    public IEnumerator Phase(Vector3 destination)
    {
        _complete = false;
        _allowMovement = false;
        _allowDeceleration = false;

        yield return null;

        FXManager.instance.SpawnFX(FXType.Teleportation, _player.transform.position, false, false, _player.facing == Direction.Left, _player.gravityFlipped);
        _player.enabled = false;
        yield return new WaitForSeconds(0.33f);
        _player.enabled = true;
        FXManager.instance.SpawnFX(FXType.Teleportation, destination, false, true, _player.facing == Direction.Left, _player.gravityFlipped);
        _player.transform.position = destination;

        _complete = true;
        _allowMovement = true;
        _allowDeceleration = true;
        _phaseRoutine = null;
    }

    public Vector3? CanPhase(Vector3 direction)
    {
        var bounds = _collider2D.bounds;
        Room room;

        if (!LayoutManager.instance || !LayoutManager.instance.currentRoom)
        {
            room = FindObjectOfType<Room>();
        }
        else
        {
            room = LayoutManager.instance.currentRoom;
        }

        var roomBounds = room.worldBounds;

        bool near = false;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            near = _player.controller2D.rightEdge.touching;
            bounds.center = bounds.center + direction * 2;
        }
        else if (direction.y < 0)
        {
            near = _player.controller2D.bottomEdge.touching;
            var center = bounds.center;
            var offset = (bounds.extents.y * 2) + 1.25f;
            center.y += _player.gravityFlipped ? offset : -offset;
            bounds.center = center;
        }
        else if (direction.y > 0)
        {
            near = _player.controller2D.topEdge.touching;
            var center = bounds.center;
            var offset = (bounds.extents.y * 2) + 1.25f;
            center.y += _player.gravityFlipped ? -offset : offset; 
            bounds.center = center;
        }

        if (roomBounds.max.x > bounds.max.x && roomBounds.max.y > bounds.max.y &&
            roomBounds.min.x < bounds.min.x && roomBounds.min.y < bounds.min.y)
        {
            if (near && !Physics2D.OverlapBox(bounds.center, bounds.size, 0, _layerMask))
            {
                return bounds.center - (Vector3)_collider2D.offset;
            }
        }

        return null;
    }

    public override void DeathStop()
    {
        _complete = true;
        if (_phaseRoutine != null)
        {
            _player.StopCoroutine(_phaseRoutine);
            _phaseRoutine = null;
        }
    }

    public override void OnPause() { }
    public override void OnUnpause() { }
}
