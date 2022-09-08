using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailFollower : Follower
{
    public bool matchPlayerFacing = true;

    public List<Vector3> positions = new List<Vector3>();
    protected Vector3 _targetPosition;
    private Vector3 _lastPosition;
    private float _totalDistanceMoved;
    private float _spacingLerp;
    private float _totalWidth;
    private float _moveTheshhold;
    private int _positionCount;
    private float _velocity;
    
    public override IEnumerator Start()
    {
        yield return base.Start();
        _targetPosition = transform.position;
    }

    public virtual void Update()
    {
        DetermineTarget();
        MoveTorwardsTarget();
    }

    public void DetermineTarget()
    {
        _positionCount = (player.trailFollowerCount + 1);
        _totalWidth = (player.trailFollowerCount + 1) * 2f;
        _spacingLerp = ((positionNumber + 1) * 2f) / _totalWidth;
        _moveTheshhold = 2f;

        var distanceMoved = Vector3.Distance(player.transform.position, _lastPosition);

        _totalDistanceMoved += distanceMoved;
        if (_totalDistanceMoved > _moveTheshhold)
        {
            positions.Add(player.transform.position);
            _totalDistanceMoved -= _moveTheshhold;
            if (positions.Count > _positionCount)
            {
                positions.RemoveAt(0);
            }
        }
        else if (positions.Count > _positionCount) //an orb was removed;
        {
            positions.RemoveAt(0);
            distanceMoved = 99f;
        }

        _lastPosition = player.transform.position;

        if (positions.Count > 0)
        {
            float trueIndex = _positionCount * (1f - _spacingLerp);
            float remainder = trueIndex % 1;
            var minPosition = positions[Mathf.Clamp(Mathf.FloorToInt(trueIndex), 0, positions.Count - 1)];
            var maxPosition = positions[Mathf.Clamp(Mathf.CeilToInt(trueIndex), 0, positions.Count - 1)];
            var truePosition = Vector3.Lerp(minPosition, maxPosition, remainder);
            _targetPosition = Vector3.MoveTowards(_targetPosition, truePosition, distanceMoved);
        }
    }

    public void MoveTorwardsTarget()
    {
        if (transform.position != _targetPosition)
        {
            var direction = (_targetPosition - transform.position).normalized;
            var distance = Vector3.Distance(_targetPosition, transform.position);

            if (distance > 0.25f)
            {
                if (_velocity == 0)
                {
                    _velocity = player.maxSpeed * 0.8f;
                }

                _velocity += distance * distance * Time.deltaTime;
                _velocity = Mathf.Clamp(_velocity, 0, distance * 5);
                transform.position += direction * _velocity * Time.deltaTime;
            }
            else
            {
                _velocity -= distance * Time.deltaTime;

                if (_velocity < 1)
                {
                    _velocity = 1;
                }

                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _velocity * Time.deltaTime);
            }
        }
        else
        {
            _velocity = 0;
        }

        if (matchPlayerFacing)
        {
            transform.rotation = player.transform.rotation;
        }
    }

    public override void OnRespawn()
    {
        base.OnRespawn();
        DetermineTarget();
        transform.position = _targetPosition;
    }
}
