using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravitator : BaseMovementBehaviour
{
    public float acceleration = 12f;
    public float maxMagnitude = 12f;
    public float directionCorrectTime = 3f;
    public float knockbackStrength = 1;
    public bool changeFacing;
    public float targetDistance = 0;
    private float _directionCorrectTimer;
    private Vector3 _direction;
    private Vector3 _velocity;

    private IDamageable _target;
    private Controller2D _controller2D;

    protected override void Start()
    {
        base.Start();
        _controller2D = GetComponent<Controller2D>();
        SetDirection();
    }
    
    private void Update ()
    {
        if(_directionCorrectTimer < directionCorrectTime)
        {
            _directionCorrectTimer += Time.deltaTime;
        }
        else
        {
            _directionCorrectTimer = 0;
            SetDirection();            
        }
        
        _velocity += _direction * acceleration * _slowMod * Time.deltaTime;

        if (_velocity.magnitude > maxMagnitude * _slowMod)
        {
            _velocity = _velocity.normalized * maxMagnitude * _slowMod;
        }

        if (_controller2D)
        {
            _controller2D.Move(_velocity * Time.deltaTime);
        }
        else
        {
            transform.position += _velocity * Time.deltaTime;
        }
        

        if (!_paused && _target != null)
        {
            if (changeFacing)
            {
                if (_direction.x < 0 && transform.rotation != Constants.flippedFacing)
                {
                    transform.rotation = Constants.flippedFacing;
                }
                else if (_direction.x > 0 && transform.rotation != Quaternion.identity)
                {
                    transform.rotation = Quaternion.identity;
                }
            }
        }
    }

    public void SetDirection()
    {
        if(!PlayerManager.CanTarget(_target))
        {
            _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
        }

        var angle = directionCorrectTime != 0 ? 90 / directionCorrectTime : 0;

        if (_target != null)
        {
            _direction = (_target.position - transform.position).normalized;
            if(Vector2.SqrMagnitude(transform.position - _target.position) < (targetDistance * targetDistance))
            {
                _direction = Vector3.Cross(_direction, Vector3.forward);
            }
        }
        else
        {
            _direction = Quaternion.Euler(0, 0, angle) * _direction;
        }
    }

    public void Knockback()
    {
        if(_target != null)
        {
            _velocity -= (_target.position-transform.position).normalized * knockbackStrength;
        }
        else
        {
            _velocity -= _direction * knockbackStrength;
        }
    }
}
