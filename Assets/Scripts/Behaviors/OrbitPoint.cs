using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitPoint : BaseMovementBehaviour
{
    public float offsetRatio;

    public float distance = 2f;    
    public float rotationTime = 3f;
    public float maxVelocity = 10f;
    public bool randomOffset = true;

    private float _velocity;
    
    private Vector3 _originalLocal;
    private Vector3 _targetPosition;

    protected override void Start()
    {
        base.Start();
        _originalLocal = transform.localPosition;
        if(randomOffset)
        {
            offsetRatio = Random.value;
        }
    }

    public void Update()
    {
        var offsetAngle = (offsetRatio) * 360;
        offsetAngle += (Time.time % rotationTime) / rotationTime * 360;

        var pivot = transform.parent ? transform.parent.TransformPoint(_originalLocal) : _originalLocal;
        
        _targetPosition = pivot + Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * distance;

        if (transform.position != _targetPosition)
        {
            var direction = (_targetPosition - transform.position).normalized;
            var distance = Vector3.Distance(_targetPosition, transform.position);

            if (distance > 0.25f)
            {
                if (_velocity == 0)
                {
                    _velocity = Player.instance.maxSpeed * 0.8f;
                }

                _velocity += distance * Time.deltaTime;
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
}
