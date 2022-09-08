using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Swooper : BaseMovementBehaviour
{    
    public float swoopSpeed;
    public int rays = 3;
    public float preSwoopDelay;
    public float preSwoopSpeed = 2f;
    public float rayArc = 30;
    public float swoopAngle = -45f;
    public float checkDistance = 5;
    public UnityEvent onSwoopDelay;
    public UnityEvent onSwoopBegin;
    public UnityEvent onSwoopEnd;    
    private Controller2D _controller2D;
    private bool _swooping;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
    }

    protected void Update()
    {
        if (!_swooping)
        {
            var rayDirection = Quaternion.Euler(0, 0, swoopAngle) * transform.right; ;
            for (int i = 0; i < rays; i++)
            {
                float angleMod = ((i / (rays - 1f)) * 2f) - 1f;
                Vector3 direction = (Quaternion.AngleAxis(angleMod * rayArc / 2, Vector3.forward) * rayDirection).normalized;
                var hit = Physics2D.Raycast(transform.position, direction.normalized, checkDistance, LayerMask.GetMask("Player"));
                if (hit)
                {
                    var target = hit.transform.gameObject.GetComponent<IDamageable>();
                    if (PlayerManager.CanTarget(target))
                    {
                        StartCoroutine(Swoop(hit.transform));
                    }
                    break;
                }
            }
        }
    }

    private IEnumerator Swoop(Transform target)
    {
        _swooping = true;
        
        if(onSwoopDelay != null) { onSwoopDelay.Invoke(); }

        var timer = 0f;
        var targetPosition = target.position;

        if (preSwoopDelay > 0)
        {
            while (timer < preSwoopDelay)
            {
                var direction = -(targetPosition - transform.position).normalized;
                var movement = direction * preSwoopSpeed * _slowMod * Time.deltaTime;
                if (_controller2D)
                {
                    _controller2D.Move(movement);
                }
                else
                {
                    transform.position += movement;
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        if (onSwoopBegin != null) { onSwoopBegin.Invoke(); }

        timer = 0f;
        var time = 1f;

        targetPosition = target.position;
        var deltaY = targetPosition.y - transform.position.y;
        var origin = transform.position;

        while (timer < time)
        {
            var newPosition = origin + (transform.right * timer * swoopSpeed * _slowMod) + (Vector3.up * deltaY * Mathf.Sin(timer / time * Mathf.PI));
            if (_controller2D)
            {
                _controller2D.Move(newPosition - transform.position);
            }
            else
            {
                transform.position = newPosition;
            }

            timer += Time.deltaTime;
            yield return null;

            origin += transform.position - newPosition; //if colliison has thrown the swooper off course
        }

        if (onSwoopEnd != null)
        {
            onSwoopEnd.Invoke();
        }

        _swooping = false;
    }

    public void OnDrawGizmosSelected()
    {
        var rayDirection = Quaternion.Euler(0, 0, swoopAngle) * transform.right;

        for (int i = 0; i < rays; i++)
        {
            float angleMod = ((i / (rays - 1f)) * 2f) - 1f;
            Vector3 direction = (Quaternion.AngleAxis(angleMod * rayArc / 2, Vector3.forward) * rayDirection).normalized;
            Debug.DrawLine(transform.position, transform.position + direction * checkDistance, Color.green);
        }
    }
}
