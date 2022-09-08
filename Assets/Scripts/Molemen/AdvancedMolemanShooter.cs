using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdvancedMolemanShooter : AdvancedAI
{
    public ProjectileStats projectileStats;
    public Transform shootUpPoint;
    public Transform shootStraightPoint;
    public Transform shootDownPoint;

    //shooting
    private float _aim;
    private Transform _aimTransform;
    private float _attackDelay = 1.5f / 12f;

    private Vector3 _shootUpOffset;
    private Vector3 _shootUpDirection;
    private Vector3 _shootStraightOffset;
    private Vector3 _shootStraightDirection;
    private Vector3 _shootDownOffset;
    private Vector3 _shootDownDirection;

    protected override void Awake()
    {
        base.Awake();
        _aimTransform = shootStraightPoint;
        _allowInAirAttack = true;
        _allowFacingAttack = true;

        _shootUpOffset = transform.InverseTransformPoint(shootUpPoint.position);
        _shootUpDirection = transform.InverseTransformDirection(shootUpPoint.right);
        _shootStraightOffset = transform.InverseTransformPoint(shootStraightPoint.position);
        _shootStraightDirection = transform.InverseTransformDirection(shootStraightPoint.right);
        _shootDownOffset = transform.InverseTransformPoint(shootDownPoint.position);
        _shootDownDirection = transform.InverseTransformDirection(shootDownPoint.right);
    }

    public override void SetAnimatorFlags()
    {
        base.SetAnimatorFlags();
        _animator.SetBool("Attacking", _canAttack);
        if(!_targetInSight) { _aim = 0; }
        _animator.SetFloat("Aim", _aim);
    }

    protected override bool CheckForTarget(Vector3 fromPos, float facing, bool setAim = true)
    {        
        var shootUp = fromPos;
        shootUp.x += _shootUpOffset.x * facing;
        shootUp.y += _shootUpOffset.y;
        var direction = _shootUpDirection;
        direction.x *= facing;

        //size of bullet
        var radius = 0.25f;

        var result = Physics2D.Raycast(shootUp, direction, maxRange, _playerMask);
        if (result.collider && result.transform == _closestPlayer)
        {
            if (setAim)
            {
                _aim = 1;
                _aimTransform = shootUpPoint;
            }
            result = Physics2D.CircleCast(shootUp, radius, direction, maxRange, _lineOfSight);
            return result.collider && result.transform == _closestPlayer;
        }

        var shootStraight = fromPos;
        shootStraight.x += _shootStraightOffset.x * facing;
        shootStraight.y += _shootStraightOffset.y;
        direction = _shootStraightDirection;
        direction.x *= facing;

        result = Physics2D.Raycast(shootStraight, direction, maxRange, _playerMask);
        if (result.collider && result.transform == _closestPlayer)
        {
            if (setAim)
            {
                _aim = 0;
                _aimTransform = shootStraightPoint;
            }
            result = Physics2D.CircleCast(shootStraight, radius, direction, maxRange, _lineOfSight);
            return result.collider && result.transform == _closestPlayer;
        }

        var shootDown = fromPos;
        shootDown.x += _shootDownOffset.x * facing;
        shootDown.y += _shootDownOffset.y;
        direction = _shootDownDirection;
        direction.x *= facing;

        result = Physics2D.Raycast(shootDown, direction, maxRange, _playerMask);
        if (result.collider && result.transform == _closestPlayer)
        {
            if (setAim)
            {
                _aim = -1;
                _aimTransform = shootDownPoint;
            }
            result = Physics2D.CircleCast(shootDown, radius, direction, maxRange, _lineOfSight);
            return result.collider && result.transform == _closestPlayer;
        }

        return false;
    }

    public override void Attack()
    {
        StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        var distance = Vector3.Distance(transform.position, _closestPlayer.transform.position);
        if (distance > minRange && distance < maxRange)
        {
            _agent.Stop();
            _speedFactor = 0;
        }

        _attacking = true;
        ProjectileManager.instance.Shoot(projectileStats, _aimTransform.position, _aimTransform.right);
        yield return new WaitForSeconds(_attackDelay);
        _attacking = false;
    }
}
