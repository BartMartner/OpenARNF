using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundsTrigger : MonoBehaviour
{
    public bool transition = true;
    private BoxCollider2D _boxCollider2D;
    private HashSet<Player> _playersPresent = new HashSet<Player>();
    public CameraBoundsTrigger[] canExpand;
    public Vector2 windowOffset;
    public float zoom = 1;
    public float zoomTime = 1;
    public Vector3 boundsPositionOffset;
    public Vector3 boundSizeOffset;
    
    //[HideInInspector]
    public bool expand;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _boxCollider2D.isTrigger = true;
        //Player.instance.onSpawn += OnPlayerSpawn;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //prevents trigger from firing multiple times during room transitions
        var player = collision.GetComponent<Player>();
        if (player && !_playersPresent.Contains(player))
        {
            _playersPresent.Add(player);
            
            var reallyTransition = transition && Vector3.Distance(player.transform.position, player.mainCamera.transform.position) < 24;

            player.mainCamera.windowOffset = windowOffset;

            var bounds = _boxCollider2D.bounds;
            bounds.size += boundSizeOffset;
            bounds.center += boundsPositionOffset;

            if (expand)
            {
                player.mainCamera.ExpandLimits(bounds);
            }
            else
            {
                player.mainCamera.SetLimits(bounds, reallyTransition);
            }

            if(zoom != player.mainCamera.currentZoomScale)
            {
                player.mainCamera.ZoomCamera(zoom, zoomTime);
            }

            if (canExpand != null)
            {
                foreach (var subBounds in canExpand)
                {
                    if (subBounds)
                    {
                        subBounds.expand = true;
                    }
                    else
                    {
                        Debug.LogWarning("subBounds of " + gameObject.name + " is null!");
                    }
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if (player) _playersPresent.Remove(player);
    }

    public Bounds GetBounds()
    {
        if (!_boxCollider2D)
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        return _boxCollider2D.bounds;
    }

    public Vector2 GetSize()
    {
        if (!_boxCollider2D)
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        return _boxCollider2D.size;
    }

    public void OnDrawGizmos()
    {
        if (!_boxCollider2D)
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }
        var bounds = _boxCollider2D.bounds;
        bounds.size += boundSizeOffset;
        bounds.center += boundsPositionOffset;

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
        if (!_boxCollider2D)
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }
        var bounds = _boxCollider2D.bounds;
        bounds.size += boundSizeOffset;
        bounds.center += boundsPositionOffset;

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
