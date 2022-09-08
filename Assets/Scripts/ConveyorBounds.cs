using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBounds : BoundsCheck, ISpecialPlatform, IPathFindingSensitive
{
    public Vector3 direction = Vector3.right;
    public float speed = 5;
    public bool checkDown;
    private HashSet<Transform> _passengers = new HashSet<Transform>();

    public bool m_pathFindingSenstive = true;
    public bool pathFindingSensitive { get { return m_pathFindingSenstive; } }    
    public Action pfTryRefresh { get; set; }
    private Collider2D[] _collider2Ds;
    public Collider2D[] collider2Ds
    {
        get
        {
            if(_collider2Ds == null) { _collider2Ds = GetComponentsInChildren<Collider2D>(); }
            return _collider2Ds;
        }
    }

    protected override void Awake()
    {
        collisionMask = LayerMask.GetMask("Player", "Enemy");
        base.Awake();
        TestEdges();
        direction.Normalize();
    }

    public override void FixedUpdate()
    {
        foreach (var transform in _passengers)
        {
            if (transform)
            {
                var controller = transform.GetComponent<Controller2D>();
                if (controller && !controller.resistConveyorsAndIce) { controller.Move(direction * speed * Time.deltaTime, false); }
            }
        }

        _passengers.Clear();
        TestEdges();
    }

    public override void OnTouch(RaycastHit2D hit, Direction direction)
    {
        if ((checkDown || direction != Direction.Down) && !_passengers.Contains(hit.transform))
        {
            _passengers.Add(hit.transform);
        }
    }

    private void OnDrawGizmos()
    {
        var d = direction.normalized;
        var end = transform.position + direction * 0.5f;
        Debug.DrawLine(transform.position - direction * 0.5f, end);
        Debug.DrawLine(end, end + Quaternion.Euler(0,0,135) * direction * 0.25f);
        Debug.DrawLine(end, end + Quaternion.Euler(0, 0, -135) * direction * 0.25f);
    }
}
