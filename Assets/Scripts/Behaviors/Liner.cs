using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Liner : BaseMovementBehaviour
{
    public float waitTime = 3;
    public float warningTime = 1f;
    public float lineDistance = 14f;
    public float speed = 10;
    public bool randomPrewarm = true;

    public UnityEvent onWarning;
    public UnityEvent onDashStart;
    public UnityEvent onDashEnd;

    private bool _lining;
    private float _timer;
    private Controller2D _controller2D;
    private Transform _closestPlayer;

    public void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        if(randomPrewarm)
        {
            _timer = waitTime * Random.Range(0, 0.5f);
        }
    }

    public void Update()
    {
        _closestPlayer = PlayerManager.instance.GetClosestPlayerTransform(transform.position);

        if (!_lining)
        {
            if (_timer < waitTime)
            {
                _timer += Time.deltaTime;
            }
            else if(_closestPlayer)
            {
                _timer = 0;
                StartCoroutine(LineDash());
            }
        }
    }

    public IEnumerator LineDash()
    {
        _lining = true;
        var direction = (_closestPlayer.position - transform.position).normalized;

        if(onWarning != null)
        {
            onWarning.Invoke();
        }

        yield return new WaitForSeconds(warningTime);

        if(onDashStart != null)
        {
            onDashStart.Invoke();
        }

        var origin = transform.position;
        var collided = false;

        while (!collided && Vector3.Distance(origin, transform.position) < lineDistance)
        {
            if (_controller2D)
            {
                _controller2D.Move(direction * speed * _slowMod * Time.deltaTime);
                collided = _controller2D.collisions.above || _controller2D.collisions.below || _controller2D.collisions.left || _controller2D.collisions.right;
            }
            else
            {
                transform.position += direction * speed * _slowMod * Time.deltaTime;
            }
            yield return null;
        }

        if (onDashEnd != null)
        {
            onDashEnd.Invoke();
        }

        _lining = false;
    }
}
