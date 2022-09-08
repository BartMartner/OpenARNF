using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeathmatchCameraBounds : MonoBehaviour
{
    public bool transition = true;
    private Collider2D[] _triggers;
    private HashSet<Player> _playersPresent = new HashSet<Player>();
    public Vector2 windowOffset;
    public float zoom = 1;
    public float zoomTime = 1;
    public Vector3 boundsPositionOffset;
    public Vector3 boundSizeOffset;
    public Vector3 boundsPositionOverride;
    public Vector3 boundsSizeOverride;
    public Collider2D boundsOverride;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
        _triggers = GetComponents<Collider2D>();
        foreach (var c in _triggers) { c.isTrigger = true; }
    }

    public void Update()
    {
        var players = DeathmatchManager.instance.players;

        if (_triggers == null || _triggers.Length <= 0)
        {
            _triggers = GetComponents<Collider2D>();
        }

        foreach (var player in players)
        {
            foreach (var c in _triggers)
            {
                var bounds = c.bounds;
                var contains = bounds.Contains(player.position);
                var present = _playersPresent.Contains(player);

                if (!contains && present)
                {
                    _playersPresent.Remove(player);
                }
                else if (contains && !present)
                {
                    StartTransition(player);
                }
            }
        }
    }

    public Bounds GetBounds()
    {
        Bounds bounds;

        if (boundsOverride)
        {
            bounds = boundsOverride.bounds;
        }
        else
        {
            bounds = _triggers.Length > 0 ? _triggers.First().bounds : new Bounds();

            if (boundsPositionOverride != Vector3.zero)
            {
                bounds.center = boundsPositionOverride;
            }
            else
            {
                bounds.center += boundsPositionOffset;
            }

            if (boundsSizeOverride != Vector3.zero)
            {
                bounds.size = boundsSizeOverride;
            }
            else
            {
                bounds.size += boundSizeOffset;
            }
        }

        return bounds;
    }

    public void StartTransition(Player player)
    {
        _playersPresent.Add(player);

        var dm = player.GetComponent<DeathmatchPlayer>();

        var reallyTransition = !dm.respawning && transition && Vector3.Distance(player.transform.position, player.mainCamera.transform.position) < 20;

        player.mainCamera.windowOffset = windowOffset;

        var bounds = GetBounds();

        player.mainCamera.SetLimits(bounds, reallyTransition);

        if (zoom != player.mainCamera.currentZoomScale)
        {
            player.mainCamera.ZoomCamera(zoom, zoomTime);
        }
    }

    public void OnDrawGizmos()
    {
        if (boundsOverride) { return; }

        if (_triggers == null || _triggers.Length <= 0)
        {
            _triggers = GetComponents<Collider2D>();
        }

        var bounds = GetBounds();

        Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.min.y, 0);
        Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0);
        Vector3 topRight = new Vector3(bounds.max.x, bounds.max.y, 0);
        Vector3 topLeft = new Vector3(bounds.min.x, bounds.max.y, 0);

        var color = Color.yellow * 0.5f;
        Debug.DrawLine(bottomLeft, bottomRight, color);
        Debug.DrawLine(bottomRight, topRight, color);
        Debug.DrawLine(topRight, topLeft, color);
        Debug.DrawLine(topLeft, bottomLeft, color);
    }

    public void OnDrawGizmosSelected()
    {
        if (_triggers == null || _triggers.Length <= 0)
        {
            _triggers = GetComponents<Collider2D>();
        }

        var bounds = GetBounds();

        Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.min.y, 0);
        Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0);
        Vector3 topRight = new Vector3(bounds.max.x, bounds.max.y, 0);
        Vector3 topLeft = new Vector3(bounds.min.x, bounds.max.y, 0);

        var color = Color.yellow;
        Debug.DrawLine(bottomLeft, bottomRight, color);
        Debug.DrawLine(bottomRight, topRight, color);
        Debug.DrawLine(topRight, topLeft, color);
        Debug.DrawLine(topLeft, bottomLeft, color);
    }
}
