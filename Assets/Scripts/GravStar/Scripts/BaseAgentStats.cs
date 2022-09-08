using System;
using UnityEngine;

[Serializable]
public class BaseAgentStats
{
    public float height = 2;
    public float maxJump = 7;
    public float timeToApex = 1;
    public float maxSpeed = 5;
    public float acceleration = 10;

    private bool _initialized;
    private float _gravity;
    private float _maxVelocity;
    private float _maxRange;
    public float maxBallisticVelocity { get; private set; }

    public void Initialize()
    {
        _initialized = true;
        _gravity = (2 * maxJump) / Mathf.Pow(timeToApex, 2);
        _maxVelocity = _gravity * timeToApex;
        maxBallisticVelocity = new Vector2(maxSpeed, _maxVelocity).magnitude;
        _maxRange = Ballistics.BallisticRange(maxBallisticVelocity, _gravity, 0);
    }

    public bool CanNavigate(GravStarNode from, GravStarNode to)
    {
        if (!_initialized) { Initialize(); }

        if (to.verticalClearance < height) return false;
        var deltaX = to.position.x - from.position.x;
        var deltaY = to.position.y - from.position.y;
        var absDeltaX = deltaX > 0 ? deltaX : -deltaX;
        var absDeltaY = deltaY > 0 ? deltaY : -deltaY;

        if (absDeltaY <= 1 && absDeltaX <= 1) { return true; }

        if (deltaY > maxJump) return false; //jump is too high
        if (deltaY < -maxJump) //if the node is within jump range, you can jump to it, otherwise you have to fall over time
        {
            //distance traveled = (initialVelocty) * time + (acceleration * 0.5f * time * time)
            //acceleration * 0.5f = _gravity/2 and initialVelocity = 0
            //yDelta = (0) * t + (_gravity/2 * t * t)
            //yDelta = _gravity/2 * t * t
            //t*t = yDelta / _gravity/2
            var t = Mathf.Sqrt(absDeltaY / _gravity * 0.5f);
            var maxDistance = (int)(maxSpeed * t + 0.5f); //Ceil
            if (absDeltaX > maxDistance) return false;
        }

        //this should account for height
        var tEst = deltaY > 0 ? (1 + (1 - deltaY / maxJump)) * 0.5f : 1;
        if (absDeltaX > _maxRange * tEst) { return false; } //jump is too far        
        return true;
    }
}
