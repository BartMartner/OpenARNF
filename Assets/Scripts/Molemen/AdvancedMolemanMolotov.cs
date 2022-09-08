using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedMolemanMolotov : AdvancedAI
{
    public ProjectileStats projectileStats;
    public Transform throwPoint;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Attack()
    {
        if (_closestPlayer)
        {
            Vector3 angle1, angle2;
            var solutions = ProjectileManager.instance.SolveBallisticArc(throwPoint.position, projectileStats.speed, _closestPlayer.transform.position, projectileStats.gravity, out angle1, out angle2);
            if (solutions > 0)
            {
                Vector3 angle;
                if(solutions == 1)
                {
                    angle = angle1;
                }
                else
                {
                    var delta = _closestPlayer.transform.position - throwPoint.position;
                    angle = angle1.y > angle2.y ? angle2 : angle1;
                }
                StartCoroutine(Throw(angle));
            }
        }
    }

    protected override bool CheckForTarget(Vector3 fromPos, float facing, bool setAim = true)
    {
        if (_closestPlayer)
        {
            var result = Physics2D.Raycast(fromPos, (_closestPlayer.position - fromPos).normalized, maxRange, _lineOfSight);

            if (result.collider && result.transform == _closestPlayer)
            {
                var throwOffset = throwPoint.position - transform.position;
                Vector3 angle1, angle2;
                var solutions = ProjectileManager.instance.SolveBallisticArc(fromPos + throwOffset, projectileStats.speed, _closestPlayer.transform.position, projectileStats.gravity, out angle1, out angle2);
                return solutions > 0;
            }
        }
        return false;
    }

    public IEnumerator Throw(Vector3 angle)
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
        yield return new WaitForSeconds(5f/12f); //anim warm up
        ProjectileManager.instance.Shoot(projectileStats, throwPoint.position, angle);
        yield return new WaitForSeconds(2f / 12f); //anim recovery
        yield return new WaitForSeconds(0.5f); //delay before attacking again
        _attacking = false;
    }
}
