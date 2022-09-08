using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AirState
{
    Grounded = 0,
    Rising = 1,
    Falling = 2,
}

public class GravStarAgent : BaseAgent
{
    public readonly static int maxAgentVertLeap = 10;
    public readonly static int maxAgentHorizLeap = 20;

    public GravStarPathFinder gravStarPathFinder;
    public override BasePathFinder pathFinder { get { return gravStarPathFinder; } }

    [Header("Animation")]
    public float jumpWarmUp = 0;
    public AirState airState { get; set; }

    protected float _speedMod = 1;
    protected float _gravity;
    public float gravity { get { return _gravity; } }
    protected float _maxJumpVelocity;

    public override void Start()
    {
        base.Start();
        CalculateJump(stats.timeToApex, stats.maxJump, out _gravity, out _maxJumpVelocity);
    }

    protected override void PreNavigate()
    {
        if (_doNotInterupt) return;

        Vector2Int nodeIndex;
        if (!currentNodeIndex.HasValue)
        {
            nodeIndex = pathFinder.PositionToIndex(transform.position + Vector3.down * nodeOffset);
        }
        else
        {
            nodeIndex = currentNodeIndex.Value;
        }

        var node = pathFinder.NodeAtIndex(nodeIndex);

        if (node == null)
        {
            Stop();
            StartCoroutine(Fall());
        }
    }

    protected override IEnumerator NavigateTo(Vector3 position)
    {
        _navigatingTo = position;
        position.y += nodeOffset;

        if ((position - transform.position).magnitude <= 1.3f)
        {
            while (transform.position != position)
            {
                var speed = _currentMoveSpeed * _speedMod;
                transform.position = Vector3.MoveTowards(transform.position, position, speed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            yield return StartCoroutine(BallisticJump(position));
        }

        currentNodeIndex = pathFinder.PositionToIndex(_navigatingTo.Value);
        _navigatingTo = null;
        _navigationCoroutine = null;
    }

    protected IEnumerator BallisticJump(Vector3 target)
    {
        if (transform.position == target) { yield break; }

        _doNotInterupt = true;

        yield return new WaitForFixedUpdate();

        var origin = transform.position;
        var delta = target - origin;
        var timer = 0f;
        var gravity = _gravity;

        if (Mathf.Abs(delta.x) <= 1)
        {
            if (delta.y < 0)
            {
                origin.x += delta.x;
                while (transform.position != origin)
                {
                    transform.position = Vector3.MoveTowards(transform.position, origin, stats.maxSpeed * Time.fixedDeltaTime);
                    yield return new WaitForFixedUpdate();
                }

                airState = AirState.Falling;
                bool hasCollided = false;
                while (!hasCollided)
                {
                    timer += Time.fixedDeltaTime;
                    var newPosition = origin + (Vector3.down * gravity * 0.5f * timer * timer);
                    newPosition.z = 0;
                    var moveDelta = newPosition - transform.position;
                    hasCollided = GravMove(moveDelta);
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                var yVel = MinimumVelocity(delta.y + 0.5f, _gravity);

                //Move straight up until above target
                airState = AirState.Rising;

                if(jumpWarmUp > 0)
                {
                    yield return new WaitForSeconds(jumpWarmUp);
                    yield return new WaitForFixedUpdate();
                }

                while (transform.position.y < target.y)
                {
                    timer += Time.fixedDeltaTime;
                    var newPosition = origin + (yVel * Vector3.up * timer) + (Vector3.down * gravity * 0.5f * timer * timer);
                    newPosition.z = 0;
                    transform.position = newPosition;
                    yield return new WaitForFixedUpdate();
                }

                //Continue Jump but start moving torwards target on x
                while (transform.position.y > target.y)
                {
                    timer += Time.fixedDeltaTime;
                    var newPosition = origin + (yVel * Vector3.up * timer) + (Vector3.down * gravity * 0.5f * timer * timer);
                    newPosition.x = Mathf.MoveTowards(transform.position.x, target.x, stats.maxSpeed * Time.deltaTime);
                    newPosition.z = 0;
                    airState = newPosition.y > transform.position.y ? AirState.Rising : AirState.Falling;
                    transform.position = newPosition;
                    if(newPosition.y < target.y)
                    {
                        newPosition.y = target.y;
                        break;
                    }
                    yield return new WaitForFixedUpdate();
                }

                airState = AirState.Grounded;
                //finish moving
                while (transform.position != target)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, stats.maxSpeed * Time.fixedDeltaTime);
                    yield return new WaitForFixedUpdate();
                }
            }
            transform.position = target;
        }
        else
        {
            Vector3 initialVelocity;
            if (GetInitialJumpVelocity(target, out initialVelocity))
            {
                bool hasCollided = false;

                if (jumpWarmUp > 0)
                {
                    airState = AirState.Rising;
                    yield return new WaitForSeconds(jumpWarmUp);
                    yield return new WaitForFixedUpdate();
                }

                while (!hasCollided)
                {
                    timer += Time.fixedDeltaTime;
                    var newPosition = origin + (initialVelocity * timer) + (Vector3.down * gravity * 0.5f * timer * timer);
                    newPosition.z = 0;

                    airState = newPosition.y > transform.position.y ? AirState.Rising : AirState.Falling;

                    if ((target - newPosition).sqrMagnitude < 1)
                    {
                        var moveDelta = newPosition - transform.position;
                        hasCollided = GravMove(moveDelta);
                    }
                    else
                    {
                        transform.position = newPosition;
                    }
                    yield return new WaitForFixedUpdate();
                }
                transform.position = target;
            }            
        }

        airState = AirState.Grounded;
        _currentMoveSpeed = 0;
        _doNotInterupt = false;        
    }

    public bool GetInitialJumpVelocity(Vector3 target, out Vector3 initialVelocity)
    {
        var origin = transform.position;
        var delta = target - origin;

        var minSpeed = Mathf.Min(_maxJumpVelocity, stats.maxSpeed);
        if (delta.y > 0)
        {
            var minY = MinimumVelocity(delta.y + 0.5f, _gravity);
            var minX = Mathf.Abs(delta.x);
            if (minX > stats.maxSpeed) { minX = stats.maxSpeed; }
            minSpeed = Mathf.Floor(new Vector2(minX, minY).magnitude);
        }

        var maxSpeed = Mathf.Ceil(stats.maxBallisticVelocity) + 2; //fudging a bit
        int solutions = 0;

        var testSpeed = minSpeed;
        Vector3 s0, s1;
        initialVelocity = Vector3.zero;

        var attempts = 0;
        while (solutions <= 0 && testSpeed <= maxSpeed)
        {
            solutions = Ballistics.SolveBallisticArc(origin, testSpeed, target, gravity, out s0, out s1);
            if (solutions > 0)
            {
                initialVelocity = solutions > 1 ? s1 : s0;
                return true;
            }
            attempts++;
            testSpeed += 0.5f;
        }

        Debug.Log("FAIL! Attempts: " + attempts + ", MinSpeed: " + minSpeed + ", Max Speed: " + maxSpeed + ", FinalSpeed: " + testSpeed + ", DeltaMag: " + delta.magnitude);
        return false;
    }

    public IEnumerator Fall()
    {        
        _doNotInterupt = true;

        var origin = transform.position;
        var timer = 0f;
        var gravity = _gravity;

        bool hasCollided = false;
        airState = AirState.Falling;

        while (!hasCollided)
        {
            timer += Time.fixedDeltaTime;
            var newPosition = origin + (Vector3.down * gravity * 0.5f * timer * timer);
            newPosition.z = 0;
            var moveDelta = newPosition - transform.position;
            hasCollided = GravMove(moveDelta);            
            yield return new WaitForFixedUpdate();
        }

        airState = AirState.Grounded;

        currentNodeIndex = null;
        _currentMoveSpeed = 0;
        _doNotInterupt = false;
    }

    public void CalculateJump(float timeToApex, float maxJumpHeight, out float gravity, out float maxVelocity)
    {
        gravity = (2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        maxVelocity = gravity * timeToApex;
    }

    public float MinimumVelocity(float height, float gravity)
    {
        var x = Mathf.Abs(2 * gravity * height);
        return Mathf.Sqrt(x);
    }

    public float timeToPeak(float velocity, float gravity)
    {
        var x = Mathf.Abs(2 * velocity / gravity);
        return Mathf.Sqrt(x);
    }

    public bool GravMove(Vector3 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        bool collided = false;

        if (directionY < 0)
        {
            float rayLength = Mathf.Abs(moveAmount.y);
            Vector2 rayOrigin = transform.position;
            rayOrigin.y -= stats.height * 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, defaultMask);

            if (hit)
            {
                moveAmount.y = (hit.distance) * directionY;
                rayLength = hit.distance;
                collided = true;
            }
        }

        transform.position += moveAmount;
        return collided;
    }

    public override void Stop()
    {
        _currentPath = null;
        if (!_doNotInterupt && _navigationCoroutine != null)
        {
            StopCoroutine(_navigationCoroutine);
            _navigatingTo = null;
        }
    }
}