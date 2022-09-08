using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeChomper : AdvancedAI
{
    public ProjectileStats projectileStats;
    public Transform eye;
    private float _attackDelay = 0.75f;    

    public override void Attack()
    {
        StartCoroutine(Shoot());
    }

    protected override bool CheckForTarget(Vector3 fromPos, float facing, bool setAim = true)
    {
        if(mode != AIMode.Hunt && ((eye.right.x == 1 && _closestPlayer.position.x < eye.position.x) ||
            (eye.right.x == -1 && _closestPlayer.position.x > eye.position.x)))
        {
            Extensions.DrawX(fromPos, 1f, setAim ? Color.yellow : Color.grey, 0.1f);
            return false;
        }

        var castPoint = fromPos + eye.localPosition;

        var result = Physics2D.CircleCast(castPoint, 0.5f, (_closestPlayer.position - castPoint).normalized, maxRange, _lineOfSight);
        if(result.collider && result.collider.transform == _closestPlayer.transform)
        {
            Extensions.DrawX(fromPos, 1f, setAim ? Color.blue : Color.white, 0.1f);
            return true;
        }

        Extensions.DrawX(fromPos, 1f, setAim ? Color.yellow : Color.grey, 0.1f);
        return false;
    }    

    private IEnumerator Shoot()
    {
        _agent.Stop();
        _speedFactor = 0;

        if (_closestPlayer.position.x > transform.position.x && transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity;
        }
        else if (_closestPlayer.position.x < transform.position.x && transform.rotation != Constants.flippedFacing)
        {
            transform.rotation = Constants.flippedFacing;
        }

        _attacking = true;
        _animator.SetTrigger("Attack");
        yield return new WaitForSeconds(4f / 12f);
        ProjectileManager.instance.Shoot(projectileStats, eye.position, (_closestPlayer.position - eye.position).normalized);
        yield return new WaitForSeconds(4f / 12f);
        _attacking = false;
        _attackCooldown = true;
        yield return new WaitForSeconds(_attackDelay);
        _attackCooldown = false;
    }
}
