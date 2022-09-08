//#define DrawDebugPath
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseAgent : MonoBehaviour
{
    public readonly static LayerMask defaultMask = 1;
    public readonly static Quaternion flippedFacing = Quaternion.Euler(0, 180, 0);

    public const float warmUpDelay = 1f;
    public BaseAgentStats stats;
    public abstract BasePathFinder pathFinder { get; }
    public bool drawNodes;
    public bool lockRotation;
    public Vector2Int? currentNodeIndex;

    protected float _currentMoveSpeed;
    public float currentMoveSpeed { get { return _currentMoveSpeed; } }
    new public Collider2D collider2D;
    protected IEnumerator _navigationCoroutine;
    protected float _warmUpTimer;
    protected Vector3 _lastPosition;

    protected List<Vector2> _currentPath;
    public bool hasPath { get { return _currentPath != null && _currentPath.Count > 0; } }

    protected Vector2? _navigatingTo;
    public bool navigating { get { return _navigatingTo.HasValue; } }
    public Vector2? navigatingTo { get { return _navigatingTo; } }
    public Vector2? destination
    {
        get
        {
            if(hasPath)
            {
                return _currentPath.Last();
            }
            else
            {
                return null;
            }
        }
    }
    protected bool _doNotInterupt;
    public bool doNotInterupt { get { return _doNotInterupt; } }

    public float nodeOffset
    {
        get
        {
            return collider2D.bounds.extents.y - collider2D.offset.y - 0.5f;
        }
    }

    public Vector2 offsetPos
    {
        get { return transform.position + Vector3.down * nodeOffset; }
    }

    protected virtual void Awake()
    {
        if (!collider2D) { collider2D = GetComponent<Collider2D>(); }
        _warmUpTimer = warmUpDelay;
    }

    public virtual void Start()
    {
        if (pathFinder == null)
        {
            Debug.LogError("GravStarAgent must have a pathFinder assigned.");
            enabled = false;
            return;
        }
        else
        {
            pathFinder.onNodesChange += OnPathFinderNodeChange;
        }
    }

    public void OnPathFinderNodeChange()
    {
        if (_currentPath != null && _currentPath.Count > 0)
        {
            _currentPath = pathFinder.GetPath(transform.position, _currentPath.Last(), stats);
        }
    }

    public void FixedUpdate()
    {
        if (_warmUpTimer > 0)
        {
            _warmUpTimer -= Time.fixedDeltaTime;
            return;
        }

        PreNavigate();
        Navigate();
#if DrawDebugPath
        DrawDebugPath();
#endif
        if (drawNodes)
        {
            pathFinder.DebugDrawNodes(stats);
        }
    }

    protected virtual void PreNavigate()
    {
        if (!currentNodeIndex.HasValue)
        {
            currentNodeIndex = pathFinder.GetClosestNode(transform.position + Vector3.down * nodeOffset).index;
        }
    }

    public void MoveTo(Vector3 position)
    {
        _currentPath = pathFinder.GetPath(transform.position, position, stats);
    }

    public bool IsNodeInCurrentPath(GravStarNode node, out int index)
    {
        index = 0;

        if (_currentPath != null)
        {
            for (int i = _currentPath.Count - 1; i >= 0; i--)
            {
                if (_currentPath[i] == node.position)
                {
                    index = i;
                    return true;
                }
            }
        }

        return false;
    }

    public void MoveTo(GravStarNode node)
    {
        if(node.position + Vector2.up * nodeOffset == (Vector2)transform.position) { return; }

        int index = 0;
        bool nodeInCurrentPath = IsNodeInCurrentPath(node, out index);

        if (!nodeInCurrentPath)
        {
            var startPos = navigating ? _navigatingTo.Value : (Vector2)transform.position;
            var start = pathFinder.GetClosestNode(startPos);
            _currentPath = pathFinder.GetPath(start, node, stats);
        }
        else if (index < _currentPath.Count - 1)
        {
            _currentPath.RemoveRange(index + 1, _currentPath.Count - index - 1);
        }
    }

    public void SetPath(List<Vector2> newPath)
    {
        var startPos = navigating ? _navigatingTo.Value : (Vector2)transform.position;
        var start = pathFinder.GetClosestNode(startPos);
        newPath.RemoveAll(n => n == start.position);
        _currentPath = newPath;
    }

    public virtual void Stop()
    {
        _currentPath = null;
        if (_navigationCoroutine != null)
        {
            StopCoroutine(_navigationCoroutine);
            _navigatingTo = null;
        }
    }

    protected virtual void Navigate()
    {
        if (navigating && _currentMoveSpeed < stats.maxSpeed)
        {
            _currentMoveSpeed += stats.maxSpeed * Time.fixedDeltaTime * stats.acceleration;
            if (_currentMoveSpeed > stats.maxSpeed) { _currentMoveSpeed = stats.maxSpeed; }
        }

        if (!doNotInterupt && !navigating && _currentPath != null && _currentPath.Count > 0)
        {
            var node = _currentPath[0];
            _currentPath.Remove(node);
            _navigationCoroutine = NavigateTo(node);
            StartCoroutine(_navigationCoroutine);
        }

        if (navigating && !lockRotation && _currentMoveSpeed > 0 ) //don't change facing if not moving
        {
            var delta = transform.position - _lastPosition;

            if (delta.x > 0 && transform.rotation != Quaternion.identity)
            {
                transform.rotation = Quaternion.identity;
            }
            else if (delta.x < 0 && transform.rotation != flippedFacing)
            {
                transform.rotation = flippedFacing;
            }
        }

        if(!navigating) { _currentMoveSpeed = 0; }

        _lastPosition = transform.position;
    }

    protected abstract IEnumerator NavigateTo(Vector3 position);

    protected void DrawDebugPath()
    {
        Vector2 p, p2;

        if (currentNodeIndex.HasValue && _navigatingTo.HasValue)
        {
            var index = currentNodeIndex.Value;            
            p = pathFinder.IndexToPosition(index.x, index.y);
            p2 = _navigatingTo.Value;
            Extensions.DrawX(p, 1, Color.green, Time.fixedDeltaTime);
            Extensions.DrawX(p2, 1, Color.red, Time.fixedDeltaTime);
            Debug.DrawLine(p, p2, Color.magenta, Time.fixedDeltaTime);
        }

        if (_currentPath != null && _currentPath.Count > 0)
        {
            if(_navigatingTo.HasValue)
            {
                p = _navigatingTo.Value;
                p2 = _currentPath[0];
                Debug.DrawLine(p, p2, Color.white, Time.fixedDeltaTime);
            }

            for (int i = 1; i < _currentPath.Count; i++)
            {
                p = _currentPath[i - 1];
                p2 = _currentPath[i];
                Extensions.DrawX(p, 1, Color.white, Time.fixedDeltaTime);
                Debug.DrawLine(p, p2, Color.white, Time.fixedDeltaTime);
            }

            Extensions.DrawX(_currentPath[_currentPath.Count - 1], 1, Color.white, Time.deltaTime);
        }
    }

    protected void OnDestroy()
    {
        if (pathFinder) { pathFinder.onNodesChange -= OnPathFinderNodeChange; }
    }

    public List<GravStarNode> GetNodesInRange(Vector2 position, float range)
    {
        var target = pathFinder.PositionToIndex(position);
        var adjustedTarget = pathFinder.PositionToIndex(position + Vector2.down * nodeOffset);
        var adjRange = (int)(range);
        var minX = adjustedTarget.x - adjRange;
        var maxX = adjustedTarget.x + adjRange;
        var minY = adjustedTarget.y - adjRange;
        var maxY = adjustedTarget.y + adjRange;

        var nodes = new List<GravStarNode>();
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x == target.x && y == target.y) continue; //don't walk to a node directly on top of where you're placing the tile
                var node = pathFinder.NodeAtIndex(x, y);
                if (node != null) { nodes.Add(node); }
            }
        }

        return nodes;
    }

    public bool CanGetInRangeOfPosition(Vector2 position, float range)
    {
        if (!currentNodeIndex.HasValue) return false;

        var start = pathFinder.NodeAtIndex(currentNodeIndex.Value);
        var nodes = GetNodesInRange(position, range);
        if (nodes == null) { return false; }

        foreach (var node in nodes)
        {
            var path = pathFinder.GetPathLinkedList(start, node, stats, true);
            if (path != null) { return true; }
        }

        return false;
    }

    public bool GetReachableNodeInRange(Vector2 position, float range, out Vector2Int targetNodeIndex)
    {
        targetNodeIndex = Vector2Int.zero;
        if (!currentNodeIndex.HasValue) return false;

        var start = pathFinder.NodeAtIndex(currentNodeIndex.Value);
        var nodes = GetNodesInRange(position, range);
        if(nodes == null) { return false; }

        var sorted = nodes.OrderBy(n =>
        {
            var x = n.position.x - start.position.x;
            var y = n.position.y - start.position.y;
            return x * x + y * y;
        });

        foreach (var node in sorted)
        {
            var path = pathFinder.GetPathLinkedList(start, node, stats, true);
            if (path != null)
            {
                targetNodeIndex = node.index;
                return true;
            }
        }

        return false;
    }
}
