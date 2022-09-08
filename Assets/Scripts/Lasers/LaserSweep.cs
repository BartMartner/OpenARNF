using UnityEngine;
using System.Collections;

public class LaserSweep : MonoBehaviour
{
    public float speed;
    public float range;
    private float _minAngle;
    private float _maxAngle;
    private float _timer;
    private bool _initialized = false;

    public void Awake()
    {
        if (!_initialized)
        {
            var euler = transform.rotation.eulerAngles;
            _minAngle = euler.z - range;
            _maxAngle = euler.z + range;
            _timer = range / speed;
            _initialized = true;
        }
    }

    public void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, _minAngle + Mathf.PingPong(_timer * speed, (_maxAngle - _minAngle)));

        _timer += Time.deltaTime;

        if (_timer > float.MaxValue - 1)
        {
            _timer = 0;
        }
    }

    public void LookAtPlayer()
    {
        LookAtObject(PlayerManager.instance.player1.transform);
    }

    public void LookAtObject(Transform target)
    {
        var euler = Quaternion.FromToRotation(Vector3.right, target.position - transform.position).eulerAngles;
        _minAngle = euler.z - range;
        _maxAngle = euler.z + range;
        _timer = range / speed;
        _initialized = true;
        transform.rotation = Quaternion.Euler(0, 0, _minAngle + Mathf.PingPong(_timer * speed, (_maxAngle - _minAngle)));
    }
}
