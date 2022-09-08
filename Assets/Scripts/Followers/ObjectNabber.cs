using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectNabber : Follower
{
    public override bool orbital { get { return true; } }

    public float offsetRatio;
    public float spotDistance = 10;
    public string spotState = "Spot";
    public float spotTime = 10 / 12f;
    public float minVelocity = 2;
    public float initialVelocityMod = 0.8f;

    protected DamageCreatureTrigger _damageTrigger;

    protected float _distance = 2f;
    //time it takes for a full rotation;
    protected float _rotationTime = 3f;

    protected GameObject _targetObject;
    protected float _timer;
    protected Animator _animator;
    protected float _velocity;
    protected Vector3 _targetPosition;
    protected bool _noMove;
    protected bool _justCollected;
    protected Vector3 _lastTarget;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public virtual void Update()
    {
        if (_noMove) { return; }

        var offsetAngle = GetOrbitalAngle();

        if (!_targetObject)
        {
            var closestObject = GetClosestObject();
            if (closestObject)
            {
                _targetObject = closestObject;
                _lastTarget = _targetObject.transform.position;
                if (!_justCollected)
                {
                    if (_animator) _animator.Play(spotState);
                    StartCoroutine(Spot(spotTime));
                }
            }
        }

        if (!_targetObject)
        {
            _targetPosition = player.transform.position + Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * _distance;
        }
        else
        {
            _targetPosition = _targetObject.transform.position;
        }

        var targetDelta = _targetObject ? Vector3.Distance(_targetPosition, _lastTarget) : 0;

        var distance = Vector3.Distance(_targetPosition, transform.position);
        if (distance > 0.125f)
        {
            var direction = (_targetPosition - transform.position).normalized;

            if (distance > 0.25f || targetDelta > 0)
            {
                var estDistance = distance + targetDelta;
                if (_velocity == 0) { _velocity = player.maxSpeed * initialVelocityMod; }
                var speed = estDistance >= 1 ? estDistance * estDistance : 1;
                _velocity += speed * Time.deltaTime;
                var tSpeed = targetDelta / Time.deltaTime;
                if (tSpeed > player.maxSpeed) tSpeed = player.maxSpeed;
                var min = tSpeed * 2 > minVelocity ? tSpeed * 2 : minVelocity; //Always move faster than the target
                var maxVelocity = estDistance * 5f < min ? min : estDistance * 5f;
                _velocity = Mathf.Clamp(_velocity, min, maxVelocity);
                transform.position += direction * _velocity * Time.deltaTime;
            }
            else
            {
                _velocity -= distance * Time.deltaTime;
                if (_velocity < minVelocity) { _velocity = minVelocity; }
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _velocity * Time.deltaTime);
            }
        }
        else
        {
            if (_targetObject) { OnObjectReached(); }
            _velocity = 0;
        }

        _lastTarget = _targetPosition;
    }

    private float GetOrbitalAngle()
    {
        if (player.orbitalFollowerCount > 0)
        {
            offsetRatio = (float)positionNumber / player.orbitalFollowerCount;
        }

        return (offsetRatio * 360) + ((Time.time % _rotationTime) / _rotationTime * 360);
    }

    private IEnumerator Spot(float time)
    {
        _noMove = true;
        yield return new WaitForSeconds(time);
        _noMove = false;
    }

    protected IEnumerator JustCollected(float time)
    {
        _justCollected = true;
        yield return new WaitForSeconds(time);
        _justCollected = false;
    }
    
    public abstract GameObject GetClosestObject();

    public virtual void OnObjectReached()
    {   
        Destroy(_targetObject.gameObject);
        StartCoroutine(JustCollected(0.25f));
    }

    public override void OnRespawn()
    {
        base.OnRespawn();
        _targetObject = null;
        _justCollected = false;
        _noMove = false;
        var offsetAngle = GetOrbitalAngle();
        transform.position = player.transform.position + Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * _distance;
    }
}
