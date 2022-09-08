using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingChasePlayer : BaseMovementBehaviour
{
    public Vector3 direction;
    [Range(0.1f,1)]
    public float homing;
    public float speed;
    public float fleeTime = 3;
    private bool _flee;
    private IDamageable _target;



    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
        direction = _target != null ? _target.position - transform.position : (Vector3)Random.insideUnitCircle.normalized;
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 newDirection;
        if (!PlayerManager.CanTarget(_target))
        {
            _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
        }

        if (_target == null)
        {
            newDirection = Quaternion.Euler(0, 0, 180 * Time.deltaTime) * direction;
        }
        else
        {
            newDirection = (_target.position - transform.position).normalized;
        }

        if (_flee) { newDirection = -newDirection; }
        direction = Vector3.Lerp(direction, newDirection, homing).normalized;

        transform.position += direction * speed * _slowMod * Time.deltaTime;
    }

    public void Flee()
    {
        if (!_flee)
        {
            StartCoroutine(FleeSequence());
        }
    }

    private IEnumerator FleeSequence()
    {
        direction = -(_target.position - transform.position).normalized;
        _flee = true;
        yield return new WaitForSeconds(fleeTime);
        _flee = false;
    }
}
