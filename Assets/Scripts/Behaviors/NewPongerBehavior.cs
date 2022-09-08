using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NewPongerBehavior : BaseMovementBehaviour
{
    public float speed = 5f;
    public Vector3 startingDirection = new Vector3(1, 1, 0);
    public bool randomStartingDirection = true;
    public float acceleration = 0;
    public float minSpeed = 0;
    public float waitTime = 0;
    public UnityEvent onWaitStart;
    public UnityEvent onSwitchDirections;

    private float _currentSpeed;
    public float currentSpeed
    {
        get { return _currentSpeed; }
    }

    private Vector3 _direction;
    public Vector3 direction
    {
        get { return _direction; }
    }

    private float _yDirection;
    private float _xDirection;
    private Controller2D _controller2D;
    private bool _switchingDirections;

    public bool smartPonging;
    public float smarts;

    [Header("Sinusodal")]
    public float sineAmplitude = 0;
    public float sinePeriod = 0;
    private float _sineTime = 0;
    private float _sine;
    private float _lastSine;
    private bool _wait;

    protected override void Start()
    {
        base.Start();
        _controller2D = GetComponent<Controller2D>();
        if (randomStartingDirection)
        {
            _direction = new Vector3();
            _direction.x = Random.value > 0.5f ? 1 : -1;
            _direction.y = Random.value > 0.5f ? 1 : -1;
        }
        else
        {
            _direction = startingDirection;
        }

        _direction.Normalize();
        _yDirection = Mathf.Abs(_direction.y);
        _xDirection = Mathf.Abs(_direction.x);
    }

    private void FixedUpdate()
    {
        if(!_switchingDirections && (_controller2D.collisions.above || _controller2D.collisions.below || _controller2D.collisions.right || _controller2D.collisions.left))
        {
            StartCoroutine(SwitchDirections());
        }

        if (_wait) return;

        if (acceleration != 0)
        {
            if (_currentSpeed < speed * _slowMod)
            {
                _currentSpeed += acceleration * Time.deltaTime;
            }
            else
            {
                _currentSpeed = speed * _slowMod;
            }
        }
        else
        {
            _currentSpeed = speed * _slowMod;
        }

        if (sinePeriod > 0 && sineAmplitude != 0)
        {
            _sineTime += Time.deltaTime / sinePeriod;
            if (_sineTime > 1) { _sineTime = 0; }
            _lastSine = _sine;
            _sine = Mathf.Sin(_sineTime * (2 * Mathf.PI)); //multiply by speed, because that's the distance moved in that amount of time
            var movement = (Vector3.right * _currentSpeed * Time.deltaTime) + Vector3.up * (_sine - _lastSine) * sineAmplitude;
            movement = Quaternion.FromToRotation(Vector3.right, _direction) * movement;
            _controller2D.Move(transform.rotation * movement);
        }
        else
        {
            _controller2D.Move(transform.rotation * _direction * Time.deltaTime * _currentSpeed);
        }
    }

    public IEnumerator SwitchDirections()
    {
        _switchingDirections = true;

        var above = _controller2D.collisions.above;
        var below = _controller2D.collisions.below;
        var left = _controller2D.collisions.left;
        var right = _controller2D.collisions.right;
        bool ySwitch = above || below;
        bool xSwitch = left || right;
        
        if (waitTime > 0)
        {
            if(onWaitStart != null)
            {
                onWaitStart.Invoke();
            }

            _wait = true;
            yield return new WaitForSeconds(waitTime);
            _wait = false;
        }

        if (onSwitchDirections != null)
        {
            onSwitchDirections.Invoke();
        }

        _currentSpeed = minSpeed;

        if(xSwitch && ySwitch)
        {
            _direction.y = _direction.y < 0 ? _yDirection : -_yDirection;
            _direction.x = _direction.x < 0 ? _xDirection : -_xDirection;
        }
        else if (smartPonging)
        {
            var target = PlayerManager.instance.GetClosestPlayerTransform(transform.position);
            var playerDelta = (target.position - transform.position);
            var playerDirection = playerDelta.normalized;
            var playerDistance = playerDelta.magnitude;
            var playerSeen = !Physics2D.Raycast(transform.position, playerDirection, playerDistance, _controller2D.collisionMask);

            if (ySwitch)
            {
                _direction.y = above ? -_yDirection : _yDirection;
            }
            else
            {
                if (playerSeen || smarts == 0)
                {
                    _direction.y = playerDirection.y > 0 ? _yDirection : -_yDirection;
                }
                else
                {
                    _direction.y = _direction.y < 0 ? -_yDirection : _yDirection;
                }
            }

            if (xSwitch)
            {
                _direction.x = right ? -_xDirection : _xDirection;
            }
            else
            {
                if (playerSeen || smarts == 0)
                {
                    _direction.x = playerDirection.x > 0 ? _xDirection : -_xDirection;
                }
                else
                {
                    _direction.x = _direction.x < 0 ? -_xDirection : _xDirection;
                }
            }

            _direction.Normalize();
            if (smarts > 0 && playerSeen)
            {
                _direction.y = Mathf.Sign(_direction.y) * Mathf.Lerp(Mathf.Abs(_direction.y), Mathf.Abs(playerDirection.y), smarts);
                _direction.x = Mathf.Sign(_direction.x) * Mathf.Lerp(Mathf.Abs(_direction.x), Mathf.Abs(playerDirection.x), smarts);
            }
        }
        else
        {
            if (ySwitch)
            {
                _direction.y = above ? -_yDirection : _yDirection;
            }
            else
            {
                _direction.y = _direction.y < 0 ? -_yDirection : _yDirection;
            }

            if (xSwitch)
            {
                _direction.x = right ? -_xDirection : _xDirection;
            }
            else
            {
                _direction.x = _direction.x < 0 ? -_xDirection : _xDirection;
            }

            _direction.Normalize();
        }

        while ((_controller2D.collisions.below && _direction.y > 0) ||
             (_controller2D.collisions.left && _direction.x > 0) ||
             (_controller2D.collisions.right && _direction.x < 0) ||
             (_controller2D.collisions.above && _direction.y < 0))
        {
            yield return null;
        }

        _switchingDirections = false;
    }    

    public void OnDrawGizmosSelected()
    {
        var time = 0f;
        var lastPoint = transform.position;
        var sineTime = 0f;
        var sine = 0f;
        var lastSine = 0f;
        var frame = 1 / 60f;

        while (time < sinePeriod)
        {
            time += frame;

            sineTime += frame / sinePeriod;

            if (sineTime > 1) { sineTime = 0; }

            lastSine = sine;
            sine = Mathf.Sin(sineTime * (2 * Mathf.PI));

            var movement = (transform.right * speed * frame) + Vector3.up * (sine - lastSine) * sineAmplitude;

            Debug.DrawLine(lastPoint, lastPoint + movement);
            lastPoint = lastPoint + movement;
        }
    }
}
