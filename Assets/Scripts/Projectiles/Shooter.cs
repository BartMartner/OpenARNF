using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Shooter : MonoBehaviour, IPausable
{
    public ProjectileStats projectileStats;
    public Vector3 direction = Vector3.right;
    public bool targetPlayer;
    public bool targetClosestEnemy;
    public bool trackTarget;
    public bool onlyShootForward;
    public float targetRange = 100;
    public bool applyTransform;

    public float preShootDelay;
    public float postShootDelay;

    public int burstCount = 1;
    public float burstTime;
    public float burstArc;

    public int arcShots = 1;
    [Range(0, 360)]
    public float fireArc = 0;

    public UnityEvent onShootStart;
    public UnityEvent onShootEnd;

    protected bool _shooting;
    public bool shooting
    {
        get { return _shooting; }
    }

    private Vector3 _currentDirection;
    private bool _paused;

    private Enemy _closestEnemy;
    private Transform _closestPlayer;

    protected virtual void Start()
    {
        direction.Normalize();
    }

    public void Shoot()
    {
        if(projectileStats.team == Team.Enemy || targetPlayer)
        {
            _closestPlayer = PlayerManager.instance.GetClosestPlayerInRange(transform, targetRange, onlyShootForward);
            if (_closestPlayer == null) { return; }
        }
        else
        {
            _closestPlayer = null;
        }

        if (targetClosestEnemy)
        {   
            if (onlyShootForward)
            {
                _closestEnemy = EnemyManager.instance.GetClosestInArc(transform.position, transform.right, targetRange, 180);
            }
            else
            {
                float lowestMagnitude;
                _closestEnemy = EnemyManager.instance.GetClosest(transform.position, out lowestMagnitude);
                if (lowestMagnitude > targetRange * targetRange)
                {
                    return;
                }
            }

            if (_closestEnemy == null)
            {
                return;
            }
        }

        if (projectileStats.projectileMotion && (targetClosestEnemy || targetPlayer))
        {
            var target = targetClosestEnemy ? _closestEnemy.transform.position : _closestPlayer.transform.position;
            Vector3 angle1, angle2;
            var solutions = ProjectileManager.instance.SolveBallisticArc(transform.position, projectileStats.speed, target, projectileStats.gravity, out angle1, out angle2);
            if (solutions > 0)
            {
                var angle = solutions == 1 || angle1.y > angle2.y ? angle1 : angle2;
                StartCoroutine(ProjectileMotionShootCoroutine(angle));
            }
        }
        else
        {
            StartCoroutine(ShootCoroutine());
        }
    }

    public virtual IEnumerator ProjectileMotionShootCoroutine(Vector3 angle)
    {
        _shooting = true;
        if (onShootStart != null) { onShootStart.Invoke(); }
        if (preShootDelay > 0) { yield return new WaitForSeconds(preShootDelay); }

        if (burstCount <= 1)
        {
            if (fireArc != 0 && arcShots > 1)
            {
                ProjectileManager.instance.ArcShoot(projectileStats, transform.position, angle, arcShots, fireArc);
            }
            else
            {
                ProjectileManager.instance.Shoot(projectileStats, transform.position, angle);
            }
        }
        else
        {
            if (burstArc > 0)
            {
                ProjectileManager.instance.ArcBurstShoot(projectileStats, transform.position, angle, burstCount, burstTime, burstArc, arcShots, fireArc);
                yield return new WaitForSeconds(burstTime); ;
            }
            else
            {
                yield return StartCoroutine(BurstShoot(projectileStats, transform.position, angle, burstCount, burstTime, arcShots, fireArc));
            }
        }

        if (postShootDelay > 0) { yield return new WaitForSeconds(postShootDelay); }

        _shooting = false;
        if (onShootEnd != null) { onShootEnd.Invoke(); }
    }

    public virtual IEnumerator ShootCoroutine()
    {
        _shooting = true;
        if (onShootStart != null) { onShootStart.Invoke(); }
        if (preShootDelay > 0) { yield return new WaitForSeconds(preShootDelay); }

        if ((targetPlayer || targetClosestEnemy) && trackTarget)
        {
            var delay = new WaitForSeconds(burstTime / burstCount);
            var shotsFired = 0;
            while (shotsFired < burstCount)
            {
                if (targetClosestEnemy && (_closestEnemy == null || _closestEnemy.state != DamageableState.Alive))
                {
                    break;
                }

                if (targetPlayer && _closestPlayer == null) { break; }
                shotsFired++;
                direction = targetPlayer ? (_closestPlayer.transform.position - transform.position).normalized : (_closestEnemy.transform.position - transform.position).normalized;

                if (fireArc != 0 && arcShots > 1)
                {
                    ProjectileManager.instance.ArcShoot(projectileStats, transform.position, direction, arcShots, fireArc);
                }
                else
                {
                    ProjectileManager.instance.Shoot(projectileStats, transform.position, direction);
                }

                yield return delay;
            }
        }
        else
        {
            if (targetPlayer)
            {
                _currentDirection = (_closestPlayer.transform.position - transform.position).normalized;
            }
            else if(targetClosestEnemy)
            {
                _currentDirection = (_closestEnemy.transform.position - transform.position).normalized;
            }
            else if (applyTransform)
            {
                _currentDirection = transform.TransformDirection(direction.normalized);
            }
            else
            {
                _currentDirection = direction.normalized;
            }

            if (burstCount <= 1)
            {
                if (fireArc != 0 && arcShots > 1)
                {
                    ProjectileManager.instance.ArcShoot(projectileStats, transform.position, _currentDirection, arcShots, fireArc);
                }
                else
                {
                    ProjectileManager.instance.Shoot(projectileStats, transform.position, _currentDirection);
                }
            }
            else
            {
                if (burstArc > 0)
                {
                    ProjectileManager.instance.ArcBurstShoot(projectileStats, transform.position, _currentDirection, burstCount, burstTime, burstArc, arcShots, fireArc);
                    yield return new WaitForSeconds(burstTime); ;
                }
                else
                {
                    yield return StartCoroutine(BurstShoot(projectileStats, transform.position, _currentDirection, burstCount, burstTime, arcShots, fireArc));
                }
            }
        }

        if (postShootDelay > 0) { yield return new WaitForSeconds(postShootDelay); }

        _shooting = false;
        if (onShootEnd != null) { onShootEnd.Invoke(); }
    }

    private IEnumerator BurstShoot(ProjectileStats stats, Vector3 origin, Vector3 direction, int burstCount, float burstTime, int arcShots, float fireArc)
    {
        var shotsFired = 0;
        while (shotsFired < burstCount)
        {
            shotsFired++;

            if (fireArc != 0 && arcShots > 1)
            {
                ProjectileManager.instance.ArcShoot(stats, origin, direction, arcShots, fireArc);
            }
            else
            {
                ProjectileManager.instance.Shoot(stats, origin, direction);
            }

            yield return new WaitForSeconds(burstTime / burstCount);
        }
    }

    public void SetShotAngle(float angle)
    {
        direction = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle), 0).normalized;
    }

    public void OnDrawGizmosSelected()
    {
        var color = new Color(1, 0, 0, 0.5f);

        if (applyTransform)
        {
            _currentDirection = transform.TransformDirection(direction);
        }
        else
        {
            _currentDirection = direction;
        }

        if (fireArc != 0 && arcShots > 1)
        {
            for (int i = 0; i < arcShots; i++)
            {
                float divisor = fireArc < 360 ? arcShots - 1 : arcShots;
                float angleMod = (((float)i / divisor) * 2f) - 1f;
                Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * fireArc / 2, Vector3.forward) * _currentDirection).normalized;
                if (projectileStats.gravity > 0)
                {
                    DrawGravityShot(shotDirection, color);
                }
                else
                {
                    var speed = projectileStats.speed + Random.Range(-projectileStats.speedDeviation, projectileStats.speedDeviation);
                    Debug.DrawLine(transform.position, transform.position + shotDirection * projectileStats.lifeSpan * speed, color);
                }
            }
        }
        else if (projectileStats.gravity > 0)
        {
            DrawGravityShot(_currentDirection, color);
        }
        else
        {
            var speed = projectileStats.speed + Random.Range(-projectileStats.speedDeviation, projectileStats.speedDeviation);
            Debug.DrawLine(transform.position, transform.position + _currentDirection * projectileStats.lifeSpan * speed, color);
        }
    }

    private void DrawGravityShot(Vector3 direction, Color color)
    {
        var timeDelta = 1f / 15f;
        var start = transform.position;

        var gravity = projectileStats.gravity + Random.Range(-projectileStats.gravityDeviation, projectileStats.gravityDeviation);
        var speed = projectileStats.speed + Random.Range(-projectileStats.speedDeviation, projectileStats.speedDeviation);

        if (projectileStats.projectileMotion)
        {
            var lifeSpan = 0f;
            while (lifeSpan < projectileStats.lifeSpan)
            {
                lifeSpan += timeDelta;
                var end = transform.position + (speed * direction * lifeSpan) + 0.5f * projectileStats.gravity * Vector3.down * lifeSpan * lifeSpan;
                Debug.DrawLine(start, end, color);
                start = end;
            }
        }
        else
        {
            var lifeSpan = projectileStats.lifeSpan;
            while (lifeSpan > 0)
            {
                lifeSpan -= timeDelta;
                if (direction.y > -1) { direction.y -= gravity * timeDelta; }
                var end = start + direction * timeDelta * speed;
                Debug.DrawLine(start, end, color);
                start = end;
            }
        }
    }

    public virtual void Pause()
    {
        if (enabled)
        {
            _paused = true;
            enabled = false;
        }
    }

    public virtual void Unpause()
    {
        if (_paused)
        {
            enabled = true;
        }
    }

    public virtual void OnEnable()
    {
        _paused = false;
    }
}
