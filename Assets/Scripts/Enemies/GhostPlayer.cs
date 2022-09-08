using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(JumpWithPlayer))]
public class GhostPlayer : MonoBehaviour
{
    public Transform shootPoint;
    public Transform shootPointUp;
    public Transform shootPointDown;
    public Transform shootPointAngleUp;
    public Transform shootPointAngleDown;
    public ProjectileStats projectileStats;

    private Animator _animator;
    private Pacer _pacer;
    private JumpWithPlayer _jumpWithPlayer;
    private Controller2D _controller2D;
    private float _animatorRunSpeed = 8.25f;
    private bool _attacking;
    private int _aiming;
    private bool _moving;
    private bool _onGround;
    private Player _player1;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _pacer = GetComponentInChildren<Pacer>();
        _jumpWithPlayer = GetComponentInChildren<JumpWithPlayer>();
        _controller2D = GetComponentInChildren<Controller2D>();
    }

    private void Start()
    {
        _player1 = PlayerManager.instance.player1;
    }

    private void Update()
    {
        _moving = _pacer.enabled;
        if (!_player1) return;

        _onGround = _controller2D.bottomEdge.touching;

        if (!_attacking)
        {
            SetAiming();
        }

        if (_player1.state == DamageableState.Alive && !_attacking)
        {
            if(_player1.controller.GetButton(_player1.attackString))
            {
                StartCoroutine(Attack(projectileStats, _player1.attackDelay));
            }
            else
            {
                _pacer.enabled = true;
            }
        }
    }

    public void SetAiming()
    {
        var p1 = transform.position;
        var p2 = _player1.transform.position;
        var angle = Vector2.Angle(transform.up, (p2 - p1).normalized);

        if (angle < 36 && !_moving)
        {
            _aiming = 2;
        }
        else if (angle < 72)
        {
            _aiming = 1;
        }
        else if (angle < 108)
        {
            _aiming = 0;
        }
        else if (angle < 144 || _onGround)
        {
            _aiming = -1;
        }
        else
        {
            _aiming = -2;
        }
    }

    public AimingInfo GetAimingInfo()
    {
        var direction = transform.right;
        var origin = shootPoint.position;

        switch (_aiming)
        {
            case -2:
                direction = Vector3.down;
                origin = shootPointDown.position;
                break;
            case -1:
                direction = (transform.right + Vector3.down).normalized;
                origin = shootPointAngleDown.position;
                break;
            case 1:
                direction = (transform.right + Vector3.up).normalized;
                origin = shootPointAngleUp.position;
                break;
            case 2:
                direction = Vector3.up;
                origin = shootPointUp.position;
                break;
        }

        if (_controller2D.rightEdge.near)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, LayerMask.GetMask("Default"));
            if (hit.collider)
            {
                if (_aiming == 0)
                {
                    origin.x = hit.point.x - direction.x * (1 / 4f);
                }
                else
                {
                    origin = (Vector3)hit.point - direction * (1 / 4f);
                }
            }
        }

        return new AimingInfo(origin, direction);
    }

    public IEnumerator Attack(ProjectileStats stats, float attackDelay)
    {
        _attacking = true;
        yield return new WaitForSeconds(0.1f);

        _pacer.enabled = false;
        _moving = false;

        SetAiming();

        transform.rotation = _player1.transform.position.x < transform.transform.position.x ? Constants.flippedFacing : Quaternion.identity;

        var aimingInfo = GetAimingInfo();

        //calculate PreShotInvisTime
        stats.preShotInvisTime = 0.48f / stats.speed;

        ProjectileManager.instance.Shoot(stats, aimingInfo);

        var timer = 0f;
        while (timer < attackDelay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _attacking = false;
    }

    private void LateUpdate ()
    {
        _animator.speed = (_onGround && _moving) ? Mathf.Abs(_pacer.velocity.x) / _animatorRunSpeed : 1;
        _animator.SetBool("Moving", _moving);
        _animator.SetBool("OnGround", _onGround);
        _animator.SetFloat("VelocityY", _jumpWithPlayer.velocity.y);

        _animator.SetLayerWeight(1, transform.rotation == Quaternion.identity ? 0 : 1);

        if (_moving && _onGround)
        {
            switch (_aiming)
            {
                case 1:
                    _animator.SetFloat("Aiming", 1);
                    break;
                case -1:
                    _animator.SetFloat("Aiming", -1);
                    break;
                default:
                    _animator.SetFloat("Aiming", _attacking ? 0.5f : 0);
                    break;
            }
        }
        else
        {
            if (_attacking && _aiming == 0 && !_onGround)
            {
                _animator.SetFloat("Aiming", 0.5f);
            }
            else
            {
                _animator.SetFloat("Aiming", _aiming);
            }
        }
    }
}
