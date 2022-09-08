using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAndOrbit : BaseMovementBehaviour
{
    public float offsetRatio;

    public float distance = 2f;    
    public float rotationTime = 3f;
    public float maxVelocity = 10f;

    private float _velocity;
    private Vector3 _targetAxis;
    private Vector3 _targetPosition;
    private IDamageable _target;

    protected override void Start()
    {
        base.Start();
        _targetAxis = transform.position;
        EnemyManager.instance.RegisterOrbital(this);
    }

    public void Update()
    {
        var offsetAngle = (offsetRatio) * 360;
        offsetAngle += (Time.time % rotationTime) / rotationTime * 360;

        if(!PlayerManager.CanTarget(_target))
        {
            _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
        }

        if (_target != null) { _targetAxis = _target.position; }
        
        _targetPosition = _targetAxis + Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * distance;

        if (transform.position != _targetPosition)
        {
            var direction = (_targetPosition - transform.position).normalized;
            var distance = Vector3.Distance(_targetPosition, transform.position);

            if (distance > 0.25f)
            {
                if (_velocity == 0)
                {
                    _velocity = PlayerManager.instance.player1.maxSpeed * 0.8f;
                }

                _velocity += distance * _slowMod * Time.deltaTime;
                _velocity = Mathf.Clamp(_velocity, 0, maxVelocity);
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
    }

    private void OnDestroy()
    {
        if (EnemyManager.instance)
        {
            EnemyManager.instance.DestroyOrbital(this);
        }
    }
}
