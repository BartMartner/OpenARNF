using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Modeled after skeleton movement in Shovel Knight.
/// +Always faces player.
/// +Moves backwards if it gets too close to player
/// +Moves forwards if it gets too far from player
/// Will move over cliffs.
/// Will stop shuffling at walls
/// +Has gravity
/// +If player is out of range, won't move
/// </summary>
public class Shuffler : MonoBehaviour, IPausable, IReactsToStatusEffect
{
    public float speed = 1f;
    public float minPlayerXDelta = 1f;
    public float maxPlayerXDelta = 6f;
    public float activeRange = 24f;
    public float turnHalfTime;
    public bool startForward = true;
    public bool resetForwardOnStop;

    public UnityEvent onTurnStart;
    public UnityEvent onTurnEnd;

    public UnityEvent onStartMoveForwards;
    public UnityEvent onStartMoveBackwards;
    public UnityEvent onStopMovement;

    private bool _forward;
    private bool _turning;
    private bool _moving;
    private float _movementTimer;
    private Controller2D _controller2D;
    private float _slowMod = 1;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _forward = startForward;
    }

    public void FixedUpdate()
    {
        //face player
        var playerDelta = Player.instance.transform.position - transform.position;
        var playerDistance = playerDelta.magnitude;
        bool tooClose = Mathf.Abs(playerDelta.x) - _controller2D.extents.x < minPlayerXDelta;
        bool tooFar = Mathf.Abs(playerDelta.x) - _controller2D.extents.x > maxPlayerXDelta;

        bool canMove = playerDistance < activeRange && !_turning;
        if (canMove)
        {
            if (_forward && _controller2D.rightEdge.touching)
            {
                if (!tooFar)
                {
                    _forward = false;
                }
                else
                {
                    canMove = false;
                }
            }
            else if (!_forward && _controller2D.leftEdge.touching)
            {
                if (!tooClose)
                {
                    _forward = true;
                }
                else
                {
                    canMove = false;
                }
            }

            if (!_moving && canMove)
            {
                _moving = true;
                if (_forward)
                {
                    if (onStartMoveForwards != null) onStartMoveForwards.Invoke();
                }
                else if (onStartMoveBackwards != null)
                {
                    onStartMoveBackwards.Invoke();
                }
            }
        }
        
        if(_moving && !canMove)
        {
            TryStopMoving();
        }

        if (!_turning)
        {
            if (turnHalfTime > 0)
            {
                if (playerDelta.x > 0 && transform.rotation != Quaternion.identity)
                {
                    StartCoroutine(Flip(Quaternion.identity));
                }
                else if (playerDelta.x < 0 && transform.rotation != Constants.flippedFacing)
                {
                    StartCoroutine(Flip(Constants.flippedFacing));
                }
            }
            else
            {
                transform.rotation = playerDelta.x > 0 ? Quaternion.identity : Constants.flippedFacing;
            }
        }

        //gravity
        var velocity = transform.up * Physics2D.gravity.y;

        //adjust velocity based on movement
        if (_moving)
        {
            if (tooClose && _forward)
            {
                _forward = false;
                if (onStartMoveBackwards != null)
                {
                    onStartMoveBackwards.Invoke();
                }
            }
            else if(tooFar && !_forward)
            {
                _forward = true;
                if (onStartMoveForwards != null)
                {
                    onStartMoveForwards.Invoke();
                }
            }

            if (_forward)
            {
                velocity += transform.right * speed * _slowMod;
            }
            else
            {
                velocity -= transform.right * speed * _slowMod;
            }
        }

        _controller2D.Move(velocity * Time.deltaTime);
    }

    public IEnumerator Flip(Quaternion targetRotation)
    {
        TryStopMoving();
        _turning = true;
        onTurnStart.Invoke();
        yield return new WaitForSeconds(turnHalfTime);
        transform.rotation = targetRotation;
        yield return new WaitForSeconds(turnHalfTime);
        onTurnEnd.Invoke();
        _turning = false;
    }

    public void TryStopMoving()
    {
        if(_moving)
        {
            _moving = false;
            if(resetForwardOnStop)
            {
                _forward = startForward;
            }

            if(onStopMovement != null)
            {
                onStopMovement.Invoke();
            }
        }
    }

    private void OnDisable()
    {
        TryStopMoving();
    }

    public void Pause()
    {
        enabled = false;
    }

    public void Unpause()
    {
        enabled = true;
    }

    public void OnAddEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = effect.amount; }
    }

    public void OnRemoveEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = 1; }
    }

    public void OnStackEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = Mathf.Clamp(_slowMod * effect.amount, 0.33f, 1); }
    }
}
