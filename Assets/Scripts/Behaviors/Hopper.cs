using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Controller2D))]
public class Hopper : BaseMovementBehaviour
{
    private Controller2D _controller2D;
    public float warmUp = 1;
    public float timeToHopApex = 1;
    public float hopHeight = 3;
    public bool hopTorwardsFacing = true;
    public bool useIHasMovementDirection;
    public bool facePlayerBeforeHop = true;
    public bool randomWarmUpStart = true;
    [Range(0, 1)]
    public float minRandomWarmUpPercent = 0.5f;
    public float horizontalSpeed = 3f;
    public float warmUpTimer;
    private bool _hopping;
    private float _gravity;
    private float _jumpVelocity;
    private IHasMovementDirection _hasMovementDirection;
    private IDamageable _target;    

    [Header("Events")]
    public UnityEvent onHopStart;
    public UnityEvent onHopEnd;

    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _hasMovementDirection = gameObject.GetComponent<IHasMovementDirection>();
    }

    protected override void Start()
    {
        base.Start();
        if(randomWarmUpStart)
        {
            warmUpTimer = Random.Range(warmUp * minRandomWarmUpPercent, warmUp);
        }
        else
        {
            warmUpTimer = warmUp;
        }
    }

    public void Update()
    {
        if (!_hopping)
        {
            if (Grounded())
            {
                if (!PlayerManager.CanTarget(_target))
                {
                    _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
                }

                if (warmUpTimer > 0)
                {
                    warmUpTimer -= Time.deltaTime;
                    if (facePlayerBeforeHop && _target != null && _target.targetable) { FacePlayer(); }
                }
                else
                {
                    StartHop(facePlayerBeforeHop);
                }
            }
            else
            {
                _controller2D.Move(transform.up * Physics2D.gravity.y * Time.deltaTime);
            }
        }
    }

    public void StartHop(bool facePlayer)
    {
        if (facePlayer) { FacePlayer(); }
        warmUpTimer = warmUp;
        StartCoroutine(Hop());
    }

    public void CalculateJump()
    {
        _gravity = -(2 * hopHeight) / Mathf.Pow(timeToHopApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToHopApex;
    }

    private IEnumerator Hop()
    {
        _hopping = true;

        if (onHopStart != null)
        {
            onHopStart.Invoke();
        }

        CalculateJump();
        Vector2 velocity = transform.up * _jumpVelocity;

        if(_hasMovementDirection != null && useIHasMovementDirection)
        {
            velocity += (Vector2)_hasMovementDirection.movementDirection * horizontalSpeed * _slowMod;
        }
        else if (hopTorwardsFacing)
        {
            velocity += (Vector2)transform.right * horizontalSpeed;
        }

        while (Grounded() && velocity.y * transform.up.y > 0)
        {
            _controller2D.Move(velocity * Time.deltaTime);
            velocity += (Vector2)transform.up * _gravity * Time.deltaTime;
            yield return null;
        }

        while (!Grounded() || (velocity.y * transform.up.y > 0))
        {
            _controller2D.Move(velocity * Time.deltaTime);
            velocity += (Vector2)transform.up * _gravity * Time.deltaTime;
            yield return null;
        }

        if (onHopEnd != null)
        {
            onHopEnd.Invoke();
        }

        _hopping = false;
    }

    public bool Grounded()
    {
        return _controller2D.autoTestEdges ? _controller2D.bottomEdge.touching : _controller2D.collisions.below;
    }

    public void FacePlayer()
    {
        float directionX = (_target != null && _target.targetable) ? (_target.position.x - transform.position.x) : Random.Range(-1,2);
        if ((directionX > 0 && transform.right.x < 0) || (directionX < 0 && transform.right.x > 0))
        {
            var eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y == 0 ? 180 : 0, eulerRotation.z);
        }
    }

    public void OnDrawGizmosSelected()
    {
        var timer = 0f;
        var lastPoint = transform.position;
        var frame = 1 / 60f;
        CalculateJump();

        Vector3 velocity = transform.up * _jumpVelocity;

        if (hopTorwardsFacing)
        {
            velocity += transform.right * horizontalSpeed;
        }

        while (timer < timeToHopApex * 2)
        {
            timer += frame;

            var newPoint = lastPoint + velocity * frame;
            Debug.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;

            velocity += transform.up * _gravity * frame;
        }
    }
}
