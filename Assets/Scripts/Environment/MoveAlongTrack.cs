using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAlongTrack : MonoBehaviour
{
    public TrackPoint currentTrackPoint;
    public bool moveTorwardsNext = true;
    public float speed;
    private TrackPoint _targetTrackPoint;

    public void Awake()
    {
        if (!currentTrackPoint)
        {
            LockToClosestTrackPoint();
        }
    }

    public void FixedUpdate()
    {
        if(Time.timeScale == 0)
        {
            return;
        }

        if (_targetTrackPoint == null)
        {
            if (moveTorwardsNext)
            {
                if (currentTrackPoint.nextTrackPoint == null)
                {
                    moveTorwardsNext = false;
                    _targetTrackPoint = currentTrackPoint.prevTrackPoint;
                }
                else
                {
                    _targetTrackPoint = currentTrackPoint.nextTrackPoint;
                }
            }
            else
            {
                if (currentTrackPoint.prevTrackPoint == null)
                {
                    moveTorwardsNext = true;
                    _targetTrackPoint = currentTrackPoint.nextTrackPoint;
                }
                else
                {
                    _targetTrackPoint = currentTrackPoint.prevTrackPoint;
                }
            }
        }

        if(_targetTrackPoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetTrackPoint.transform.position, speed * Time.deltaTime);
            if(Vector3.Distance(transform.position, _targetTrackPoint.transform.position) < 0.1f)
            {
                currentTrackPoint = _targetTrackPoint;
                _targetTrackPoint = null;
            }
        }
    }

    public void LockToClosestTrackPoint()
    {
        var trackPoints = FindObjectsOfType<TrackPoint>();
        var distance = float.MaxValue;
        foreach (var trackPoint in trackPoints)
        {
            var newDistance = Vector2.Distance(trackPoint.transform.position, transform.position);
            if (newDistance < distance)
            {
                currentTrackPoint = trackPoint;
                distance = newDistance;
            }
        }

        transform.position = currentTrackPoint.transform.position;
    }
}
