using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class MoveTorwardsPlayer : BaseMovementBehaviour
{    
    public float speed = 5f;
    public bool changeFacing = true;

    public UnityEvent onPlayerFound;
    public UnityEvent onPlayerLost;

    private IDamageable _target;
    private Vector3 _direction;

    // Update is called once per frame
    void Update()
    {
        var hadTarget = _target != null;
        if (!PlayerManager.CanTarget(_target))
        {
            _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
        }

        if(onPlayerFound != null && !hadTarget && _target != null)
        {
            onPlayerFound.Invoke();
        }

        if (onPlayerLost != null && hadTarget && _target == null)
        {
            onPlayerLost.Invoke();
        }

        if (!_paused && _target != null)
        {
            _direction = _target.targetable ? (_target.position - transform.position).normalized : Quaternion.Euler(0, 0, speed * _slowMod * 15 * Time.deltaTime) * _direction;
            transform.position += _direction * speed * _slowMod * Time.deltaTime;

            if (changeFacing)
            {
                if (_direction.x < 0 && transform.rotation != Constants.flippedFacing)
                {
                    transform.rotation = Constants.flippedFacing;
                }
                else if (_direction.x > 0 && transform.rotation != Quaternion.identity)
                {
                    transform.rotation = Quaternion.identity;
                }
            }
        }
    }
}
