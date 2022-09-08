using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleDasher : BaseMovementBehaviour 
{
    public float maxVelocity = 5f;
    public float acceleration = 5f;
    public float directlyAtPlayerRange = 3f;
    public float waitTime = 0.6666f;
    public float immediateVelocity = 3f;
    public float initialWait = 0.5f;

    private Animator _animator;
    private Vector3 _direction;
    private float _velocity;
    private bool _accelerating;
    private bool _waiting;


    protected override void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
        StartCoroutine(InitialWait());
    }

    public IEnumerator InitialWait()
    {
        _waiting = true;
        yield return new WaitForSeconds(initialWait);
        _waiting = false;
    }

    public void Update()
    {
        if(_waiting || (NPCDialogueManager.instance && NPCDialogueManager.instance.dialogueActive))
        {
            return;
        }

        if (_velocity <= 0 && !_accelerating)
        {
            _accelerating = true;
            _velocity = immediateVelocity;

            if (waitTime > 0)
            {
                StartCoroutine(Wait());
            }
            else
            {
                PickDirection();
                SetDirectionAnimation();
            }
        }

        if(_velocity >= (maxVelocity * _slowMod) && _accelerating)
        {
            _accelerating = false;
        }

        _velocity += (_accelerating ? acceleration : -acceleration) * Time.deltaTime;
        transform.position += _velocity * _direction * Time.deltaTime;
    }

    public void PickDirection()
    {
        var target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);

        if (target != null && Vector3.Distance(transform.position, target.position) <= directlyAtPlayerRange)
        {
            _direction = target.position - transform.position;
        }
        else
        {
            _direction = Vector3.zero;
            _direction.x = target == null || target.position.x > transform.position.x ? 1 : -1;
            _direction.y = target == null || target.position.y > transform.position.y ? 1 : -1;
        }

        _direction.Normalize();
    }

    public void SetDirectionAnimation()
    {
        string animState = string.Empty;
        animState += _direction.y > 0 ? "Up" : "Down";
        animState += _direction.x > 0 ? "Right" : "Left";

        _animator.Play(animState);
    }

    public IEnumerator Wait()
    {
        _waiting = true;

        if(_animator)
        {
            _animator.Play("Wait");
        }

        yield return new WaitForSeconds(waitTime);

        _waiting = false;

        PickDirection();
        SetDirectionAnimation();
    }
}
