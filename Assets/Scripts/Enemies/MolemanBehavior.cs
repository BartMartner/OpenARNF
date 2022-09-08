using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MolemanBehavior : MonoBehaviour
{
    public float paceSpeed = 1.5f;
    public float paceRange = 2;
    public bool groundAhead;
    private Vector3 _lastPosition;
    private Vector3 _paceStartPosition;
    public int stuckCount;

    public float chaseSpeed = 7;
    private Controller2D _controller2D;
    private Quaternion _targetFacing;
    private Animator _animator;

    public float leapForce = 9;
    public int leapRays = 4;
    public float leapRayArc = 45;
    public float leapCheckDistance = 6;
    public UnityEvent onLeapWarmUp;
    
    private Vector3 _velocity;
    private bool _alert;
    private bool _leaping;
    private bool _waitFacing;
    private bool _justLept;
    
    private int _playerMask;

    private IDamageable _target;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _animator = GetComponent<Animator>();
        _paceStartPosition = -transform.right * paceRange * 0.5f;
        _playerMask = LayerMask.GetMask("Player");
    }

    public void Update()
    {
        if (!_leaping)
        {
            _velocity = Vector3.zero;

            if (!_controller2D.bottomEdge.touching)
            {
                _velocity = transform.up * Physics2D.gravity.y;
            }

            bool leftEdgeWall = _controller2D.leftEdge.touching && _controller2D.leftEdge.angle % 90 == 0;
            bool rightEdgeWall = _controller2D.rightEdge.touching && _controller2D.rightEdge.angle % 90 == 0;
            bool canMove = !_waitFacing && _controller2D.bottomEdge.touching && !(leftEdgeWall && rightEdgeWall);

            if (canMove)
            {
                if (!_alert)
                {
                    groundAhead = _controller2D.CheckForGroundAhead(0.25f, 0.5f);
                    if (_lastPosition == transform.position || (_controller2D.bottomEdge.angle % 90 == 0 && !groundAhead))
                    {
                        stuckCount++;
                    }
                    else
                    {
                        stuckCount = 0;
                    }

                    if (stuckCount > 4 || (_controller2D.rightEdge.touching && _controller2D.rightEdge.angle >= 90) || Vector3.Distance(_paceStartPosition, transform.position) > paceRange)
                    {
                        StartCoroutine(WaitAndChangeFacing());
                    }
                    else
                    {
                        _velocity += transform.right * paceSpeed;
                    }
                }
                else
                {
                    if (!PlayerManager.CanTarget(_target))
                    {
                        _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
                    }

                    var validTarget = _target != null && _target.targetable;
                    var playerDeltaX = validTarget ? _target.position.x - transform.position.x : 999;
                    var absDeltaY = validTarget ? Mathf.Abs(_target.position.y - transform.position.y): 999;

                    if (validTarget && (Mathf.Abs(playerDeltaX) > 1 || absDeltaY < 2))
                    {
                        stuckCount = 0;
                        _targetFacing = playerDeltaX > 0 ? Quaternion.identity : Constants.flippedFacing;
                        transform.rotation = _targetFacing;
                        _velocity += transform.right * chaseSpeed;
                    }
                    else
                    {
                        stuckCount++;
                        if (stuckCount > 4)
                        {
                            _alert = false;
                            _paceStartPosition = transform.position;
                        }
                    }
                }
            }

            _animator.SetBool("Alert", _alert);

            _lastPosition = transform.position;
            _controller2D.Move(_velocity * Time.deltaTime);
            var moving = canMove || (!_waitFacing && !(rightEdgeWall && leftEdgeWall));
            _animator.SetBool("Moving", moving);

            if (!_justLept && _controller2D.bottomEdge.touching && _target != null && _target.targetable)
            {
                var rayDirection = (transform.right);
                for (int i = 0; i < leapRays; i++)
                {
                    float angleMod = ((i / (leapRays - 1f)) * 2f) - 1f;
                    Vector3 direction = (Quaternion.AngleAxis(angleMod * leapRayArc / 2, Vector3.forward) * rayDirection).normalized;
                    if (Physics2D.Raycast(transform.position, direction.normalized, leapCheckDistance, _playerMask))
                    {
                        StartCoroutine(Leap());
                        break;
                    }
                }

                if (_alert && rightEdgeWall)
                {
                    StartCoroutine(Leap());
                }
            }
        }
    }

    private IEnumerator WaitAndChangeFacing()
    {
        _waitFacing = true;

        _paceStartPosition = transform.position;

        yield return new WaitForSeconds(1f);

        if (!_leaping && !_alert)
        {
            var eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y == 0 ? 180 : 0, eulerRotation.z);
        }

        stuckCount = 0;
        _waitFacing = false;
    }

    private IEnumerator Leap()
    {
        //LeapWarmUp
        _leaping = true;
        _alert = true;
        _animator.SetBool("Moving", false);
        
        if (onLeapWarmUp != null)
        {
            onLeapWarmUp.Invoke();
        }

        _animator.SetTrigger("JumpStart");

        yield return new WaitForSeconds(5 / 24f);
        yield return new WaitForFixedUpdate();

        //LeapStart

        Vector2 velocity = ((Vector2)transform.right + Vector2.up).normalized * leapForce;

        while (velocity.y > 0 || !_controller2D.bottomEdge.touching)
        {
            _controller2D.Move(velocity * Time.deltaTime);
            yield return null;
            velocity += Physics2D.gravity * Time.deltaTime;
        }

        _animator.SetTrigger("JumpEnd");
        _leaping = false;
        _justLept = true;

        //LeapRecover
        yield return new WaitForSeconds(0.125f);

        _justLept = false;
    }

    public void OnDrawGizmosSelected()
    {
        var rayDirection = (transform.right);
        for (int i = 0; i < leapRays; i++)
        {
            float angleMod = ((i / (leapRays - 1f)) * 2f) - 1f;
            Vector3 direction = (Quaternion.AngleAxis(angleMod * leapRayArc / 2, Vector3.forward) * rayDirection).normalized;
            Debug.DrawLine(transform.position, transform.position + direction * leapCheckDistance, Color.green);
        }
    }

    public void OnHurt()
    {
        _alert = true;
    }
}
