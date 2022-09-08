using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolemanShooter : MonoBehaviour
{
    public ProjectileStats projectileStats;
    public float facingDelay = 0.5f;
    public Transform shootUpPoint;
    public Transform shootStraightPoint;
    public Transform shootDownPoint;
    public float range = 12f;
    public AudioClip panic;
    public AudioClip spotPlayer;
    public float shotWarmUp = 1;

    private Animator _animator;
    private AudioSource _audioSource;
    private Transform _closestPlayer;
    private bool _justChangedFacing;

    //shooting
    private LayerMask _playerMask;
    private LayerMask _clearShotMask;
    private float _aim;
    private bool _shooting;
    private Transform _aimTransform;
    private float _shootingDelay = 2/12f;
    private bool _justShot;
    private bool _inSight;
    private float _reactionTime = 0.33f;
    private float _reactionCounter;
    

    //Movement
    private Controller2D _controller2D;
    private float _velocity;
    private float _acceleration = 8f;
    private float _deceleration = -16f;
    private float _maxVelocity = 8f;
    private Vector2 _moveDirection;

    //Other
    private bool _panic;
    private bool _postPanic;
    private bool _testingForPanic;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _controller2D = GetComponent<Controller2D>();
        _playerMask = LayerMask.GetMask("Player");
        _clearShotMask = LayerMask.GetMask("Player", "Default");
        _aimTransform = shootStraightPoint;
    }

    public void Update()
    {
        _closestPlayer = PlayerManager.instance.GetClosestPlayerTransform(transform.position);

        if (_closestPlayer)
        {
            if (!_justChangedFacing)
            {
                if (_panic)
                {
                    if (_closestPlayer.position.x > transform.position.x && transform.rotation != Constants.flippedFacing)
                    {
                        transform.rotation = Constants.flippedFacing;
                        StartCoroutine(ChangeFacing());
                    }
                    else if (_closestPlayer.position.x < transform.position.x && transform.rotation != Quaternion.identity)
                    {
                        transform.rotation = Quaternion.identity;
                        StartCoroutine(ChangeFacing());
                    }
                }
                else
                {
                    if (_closestPlayer.position.x > transform.position.x && transform.rotation != Quaternion.identity)
                    {
                        transform.rotation = Quaternion.identity;
                        StartCoroutine(ChangeFacing());
                    }
                    else if (_closestPlayer.position.x < transform.position.x && transform.rotation != Constants.flippedFacing)
                    {
                        transform.rotation = Constants.flippedFacing;
                        StartCoroutine(ChangeFacing());
                    }
                }
            }

            if(shotWarmUp > 0 && (!LayoutManager.instance || !LayoutManager.instance.transitioning))
            {
                shotWarmUp -= Time.deltaTime;
            }

            if (shotWarmUp <= 0)
            {
                var spotted = CheckForPlayer();

                if (!_inSight && spotted) { _audioSource.PlayOneShot(spotPlayer); }
                _inSight = spotted;

                if (_inSight)
                {
                    _reactionCounter += Time.deltaTime;
                    _shooting = _reactionCounter > _reactionTime;
                }
                else
                {
                    _reactionCounter = 0;
                    _shooting = false;
                }

                if (_shooting && !_justShot)
                {
                    ProjectileManager.instance.Shoot(projectileStats, _aimTransform.position, _aimTransform.right);
                    StartCoroutine(JustShot());
                }
            }

            var delta = _closestPlayer.position - transform.position;
            var xDistance = Mathf.Abs(delta.x);
            bool backwards = false;
            bool needsToMove = false;

            if(_panic)
            {
                _moveDirection = transform.right;
                needsToMove = true;
                if (_velocity < _maxVelocity)
                {
                    _velocity = _maxVelocity;
                }
            }
            else if (xDistance < 4 && delta.y > 0) //move away from player
            {
                var b = -Mathf.Sign(delta.normalized.x) != transform.right.x;
                _moveDirection = (b ? -transform.right : transform.right);
                needsToMove = true;
                if (_velocity < _maxVelocity * 0.5f)
                {
                    _velocity = _maxVelocity * 0.5f;
                }
            }
            else if(!_postPanic && (xDistance > range || (delta.y < -2 && xDistance > 2))) //aggro if not post panic and player out of range or below
            {
                var b = Mathf.Sign(delta.normalized.x) != transform.right.x;
                _moveDirection = (b ? -transform.right : transform.right);
                needsToMove = true;
                if (_velocity < _maxVelocity * 0.5f)
                {
                    _velocity = _maxVelocity * 0.5f;
                }
            }

            if (_moveDirection != Vector2.zero)
            {
                backwards = _moveDirection.x != transform.right.x;

                var wall = backwards ? (_controller2D.leftEdge.touching && _controller2D.leftEdge.angle % 90 == 0) : (_controller2D.rightEdge.touching && _controller2D.rightEdge.angle % 90 == 0);
                var ground = backwards ? _controller2D.CheckForGroundBehind(0.25f, 0.5f) : _controller2D.CheckForGroundAhead();

                if (wall || !ground) // sudden stop
                {
                    _moveDirection = Vector2.zero;
                    _velocity = 0;
                }
                else //speed up or slow down
                {
                    _velocity += (needsToMove ? _acceleration : _deceleration) * Time.deltaTime;
                    if(_velocity > _maxVelocity) { _velocity = _maxVelocity; }

                    if (_velocity <= 0)
                    {
                        _moveDirection = Vector2.zero;
                        _velocity = 0;
                    }
                }
            }
            else
            {
                _velocity = 0;
            }

            if (_velocity != 0)
            {
                _controller2D.Move(_moveDirection * _velocity * Time.deltaTime);
            }
        }
        else
        {
            _moveDirection = Vector2.zero;
            _velocity = 0;
        }

        var speedFactor = 0f;

        if(_velocity != 0)
        {
            speedFactor = _velocity > _maxVelocity * 0.75 ? 1 : 0.5f;
            if (_moveDirection.x != transform.right.x) { speedFactor *= -1; }
        }

        if(!_controller2D.bottomEdge.touching)
        {
            _controller2D.Move(Physics2D.gravity);
        }

        _animator.SetFloat("Aim", _aim);
        _animator.SetBool("Shooting", _shooting);
        _animator.SetFloat("Speed", speedFactor);
    }

    private bool CheckForPlayer()
    {
        var result = Physics2D.Raycast(shootUpPoint.transform.position, shootUpPoint.right, range, _playerMask);
        if (result.collider && result.collider.transform == _closestPlayer)
        {
            _aim = 1;
            _aimTransform = shootUpPoint;
            result = Physics2D.Raycast(shootUpPoint.transform.position, shootUpPoint.right, range, _clearShotMask);
            return result.collider && result.collider.transform == _closestPlayer;
        }

        result = Physics2D.Raycast(shootStraightPoint.transform.position, shootStraightPoint.right, range, _playerMask);
        if (result.collider && result.collider.transform == _closestPlayer)
        {
            _aim = 0;
            _aimTransform = shootStraightPoint;
            result = Physics2D.Raycast(shootStraightPoint.transform.position, shootStraightPoint.right, range, _clearShotMask);
            return result.collider && result.collider.transform == _closestPlayer;
        }

        result = Physics2D.Raycast(shootDownPoint.transform.position, shootDownPoint.right, range, _playerMask);
        if (result.collider && result.collider.transform == _closestPlayer)
        {
            _aim = -1;
            _aimTransform = shootDownPoint;
            result = Physics2D.Raycast(shootDownPoint.transform.position, shootDownPoint.right, range, _clearShotMask);
            return result.collider && result.collider.transform == _closestPlayer;
        }

        return false;
    }

    public void OnHurt()
    {
        shotWarmUp = 0;

        if (!_shooting && !_panic && !_testingForPanic)
        {
            StartCoroutine(TestPanic());
        }
    }

    private IEnumerator TestPanic()
    {
        _testingForPanic = true;
        yield return new WaitForSeconds(0.25f);
        if (!_shooting && !_panic && !CheckForPlayer())
        {
            _postPanic = false;
            _panic = true;
            _audioSource.PlayOneShot(panic);
            yield return new WaitForSeconds(0.5f);
            _panic = false;
        }
        _testingForPanic = false;

        _postPanic = true;
        yield return new WaitForSeconds(2f);
        _postPanic = false;
    }

    private IEnumerator ChangeFacing()
    {
        _justChangedFacing = true;
        yield return new WaitForSeconds(facingDelay);
        _justChangedFacing = false;
    }

    private IEnumerator JustShot()
    {
        _justShot = true;
        yield return new WaitForSeconds(_shootingDelay);
        _justShot = false;
    }
}
