using UnityEngine;
using System.Collections;

public class FollowTransform : MonoBehaviour
{
    public Transform toFollow;
    public float followDistance;
    public float followLurch;
    public float maxSpeed = 100;
    public bool autoFollowDistance;
    private Vector3 _target;
    private float _lurchTimer;
    private Vector3 _lastRecorded;

    public void Start()
    {
        if(autoFollowDistance)
        {
            followDistance = Vector3.Distance(transform.position, toFollow.transform.position);
        }
    }

	public void Update ()
    {
        if (_lurchTimer > 0)
        {
            _lurchTimer -= Time.deltaTime;
            _target = Vector3.Lerp(toFollow.transform.position, _lastRecorded, _lurchTimer/followLurch);
        }
        else
        {
            _lurchTimer = followLurch;
            _target = _lastRecorded = toFollow.transform.position;
        }

        var difference = _target - transform.position;
        var difDelta = difference.magnitude - followDistance;
        if (difDelta > 0)
        {
            var speed = Mathf.Clamp(maxSpeed * Time.deltaTime, 0, difDelta);
            transform.position += difference.normalized * speed;
        }
    }
}
