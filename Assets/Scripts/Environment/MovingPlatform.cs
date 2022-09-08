using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : BoundsCheck, ISpecialPlatform
{
    public bool checkDown;
    private HashSet<Transform> _passengers = new HashSet<Transform>();
    protected Vector3 _lastPosition;
    protected Vector3 _lastLocalPosition;

    protected override void Awake()
    {
        collisionMask = LayerMask.GetMask("Player", "Enemy");
        base.Awake();

        TestEdges();
        _lastLocalPosition = transform.localPosition;
        _lastPosition = transform.position;
    }

    public override void FixedUpdate()
    {
        var move = (transform.position - _lastPosition);

        //Sanity Check for when rooms are first loaded and moved into position
        if (Vector3.Distance(transform.position, _lastPosition) > 24)
        {
            move = (transform.localPosition - _lastLocalPosition);
        }

        foreach (var transform in _passengers)
        {
            if (transform)
            {
                var controller = transform.GetComponent<Controller2D>();
                if (controller) { controller.Move(move, false); }
            }
        }

        _lastPosition = transform.position;
        _lastLocalPosition = transform.localPosition;

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
}
