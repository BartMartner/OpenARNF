using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedMolemanRager : AdvancedAI
{
    private bool _animateAttack;
    private Controller2D _controller2D;

    protected override void Awake()
    {
        base.Awake();
        _controller2D = GetComponent<Controller2D>();
    }

    protected override bool CheckForTarget(Vector3 fromPos, float facing, bool setAim = true)
    {
        var result = false;
        for (int i = -4; i <= 4; i++)
        {
            var direction = Quaternion.Euler(0,0,i*7.5f) * (Vector3.right * facing);
            var raycast = Physics2D.Raycast(fromPos, direction, maxRange, _lineOfSight);
            if (raycast.collider && raycast.transform == _closestPlayer)
            {
                result = true;
                break;
            }
        }
        
        return result;
    }

    public override void SetAnimatorFlags()
    {
        base.SetAnimatorFlags();
        _animator.SetBool("Attacking", _animateAttack);
    }

    public override void Attack()
    {
        _agent.Stop();

        if (_closestPlayer.position.x > transform.position.x && transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity;
        }
        else if (_closestPlayer.position.x < transform.position.x && transform.rotation != Constants.flippedFacing)
        {
            transform.rotation = Constants.flippedFacing;
        }

        StartCoroutine(JumpAttack(_closestPlayer.transform.position + transform.right));
    }

    private IEnumerator JumpAttack(Vector3 target)
    {
        if (transform.position == target) { yield break; }
        _attacking = true;
        _animateAttack = true;
        _coroutineControl = true;

        var origin = transform.position;

        _controller2D.enabled = true;
        Vector3 initialVelocity;
        if (_agent.GetInitialJumpVelocity(target, out initialVelocity))
        {
            var timer = 0f;

            var lastPosition = transform.position;
            bool hasCollided = false;
            while (!hasCollided)
            {
                timer += Time.fixedDeltaTime;
                var newPosition = origin + (initialVelocity * timer) + (Vector3.down * _agent.gravity * 0.5f * timer * timer);
                newPosition.z = 0;
                _agent.airState = newPosition.y > transform.position.y ? AirState.Rising : AirState.Falling;
                var moveDelta = newPosition - lastPosition;
                _controller2D.Move(moveDelta);
                lastPosition = newPosition;
                hasCollided = _controller2D.collisions.below;
                yield return new WaitForFixedUpdate();
            }
            _agent.airState = AirState.Grounded;
        }
        else
        {
            Debug.Log(gameObject.name + " found no solutions for jump attack.");
        }

        _coroutineControl = false;
        _animateAttack = false;
        yield return new WaitForSeconds(0.05f);
        _attacking = false;
    }
}
