using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Known bugs... will walk off slopes
public class AdvancedPacer : MonoBehaviour, IHasMovementDirection, IReactsToStatusEffect
{
    public UnityEvent onChangeDirection;
    public int stuckCount;
    public bool hasGravity;
    public bool flipFacing = true;

    [Tooltip("Range of 0 means pace until collision")]
    [Range(0, 24)]
    public float limitDistance;
    [Range(0, 1)]
    public float limitOffset = 0.5f;
    public float speed = 5f;

    public bool setLimitsTrigger;

    private Controller2D _controller2D;
    private Vector3 _lastPosition;
    private Vector3 _positiveLimit;
    private Vector3 _negativeLimit;
    private float _setDistance;
    private Direction _direction = Direction.Right;
    private float _slowMod = 1;

    public Vector3 movementDirection
    {
        get
        {
            if (flipFacing)
            {
                return transform.right;
            }
            else
            {
                return _direction == Direction.Right ? transform.right : -transform.right;
            }
        }
    }


    private Vector3 _velocity;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _direction = transform.right == Vector3.right ? Direction.Right : Direction.Left;
    }

    public void Start()
    {
        SetLimitsBasedOnPosition();
    }

    public void Update()
    {
        if (Vector2.Distance(_lastPosition, transform.position) > speed)
        {
            SetLimitsBasedOnPosition();
        }

        bool changeDirection = false;
        var groundAhead = _controller2D.CheckForGroundAhead(0.25f, 0.5f);
        var groundBehind = _controller2D.CheckForGroundBehind(0.25f, 0.5f);

        _velocity = Vector3.zero;

        if (hasGravity)
        {
            _velocity = transform.up * Physics2D.gravity.y;
        }

        if ((hasGravity && (!_controller2D.bottomEdge.touching || (!groundAhead && !groundBehind))) || //falling or stuck on single tile
            (_controller2D.leftEdge.touching && _controller2D.leftEdge.angle % 90 == 0 && _controller2D.rightEdge.touching && _controller2D.rightEdge.angle % 90 == 0)) // stuck between two blocks
        {
            _controller2D.Move(_velocity * Time.deltaTime);
            return;
        }

        _velocity += movementDirection * speed * _slowMod;
        
        _controller2D.Move(_velocity * Time.deltaTime);

        if (_lastPosition == transform.position || (hasGravity && _controller2D.bottomEdge.angle % 90 == 0 && !groundAhead))
        {
            stuckCount++;
        }
        else
        {
            stuckCount = 0;
        }

        var edge = flipFacing ? _controller2D.rightEdge : (_direction == Direction.Right ? _controller2D.rightEdge : _controller2D.leftEdge);
        var tooFar =  _setDistance > 0 && Vector3.Distance(transform.position, (_direction.ToVector2().x > 0 ? _negativeLimit : _positiveLimit)) > _setDistance;
        changeDirection = tooFar || (hasGravity && !groundAhead) || (edge.touching && edge.angle % 90 == 0) || stuckCount > 4;

        if (changeDirection && Time.timeScale > 0)
        {
            ChangeDirection();
        }

        _lastPosition = transform.position;

        if(setLimitsTrigger)
        {
            setLimitsTrigger = false;
            SetLimitsBasedOnPosition();
        }
    }

    private void SetLimitsBasedOnPosition()
    {
        _setDistance = limitDistance;
        _positiveLimit = transform.position + Vector3.right * _setDistance * (1 - limitOffset);
        _negativeLimit = transform.position - Vector3.right * _setDistance * limitOffset;
    }

    public void ToggleGravity(bool toggle)
    {
        hasGravity = toggle;
    }

    public void ChangeDirection()
    {
        onChangeDirection.Invoke();
        var eulerRotation = transform.rotation.eulerAngles;
        _direction = _direction == Direction.Right ? Direction.Left : Direction.Right;
        if (flipFacing)
        {
            transform.rotation *= Constants.flippedFacing;
        }
    }

    public void FacePlayer()
    {
        var player = PlayerManager.instance.GetClosestPlayerTransform(transform.position);
        if (player)
        {
            var delta = player.position - transform.position;
            if (Mathf.Sign(delta.x) != Mathf.Sign(_direction.ToInt2D().x))
            {
                ChangeDirection();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (limitDistance > 0)
        {
            var newPositiveLimit = transform.position + transform.right * limitDistance * (1 - limitOffset);
            var newNegativeLimit = transform.position - transform.right * limitDistance * limitOffset;
            Debug.DrawLine(transform.position, newPositiveLimit);
            Debug.DrawLine(newPositiveLimit - transform.up * 0.5f, newPositiveLimit + transform.up * 0.5f);
            Debug.DrawLine(transform.position, newNegativeLimit);
            Debug.DrawLine(newNegativeLimit - transform.up * 0.5f, newNegativeLimit + transform.up * 0.5f);

            if (!Application.isPlaying)
            {
                SetLimitsBasedOnPosition();
            }

            if (_setDistance > 0)
            {
                Debug.DrawLine(transform.position, _positiveLimit, Color.red);
                Debug.DrawLine(_positiveLimit - transform.up * 0.5f, _positiveLimit + transform.up * 0.5f, Color.red);
                Debug.DrawLine(transform.position, _negativeLimit, Color.red);
                Debug.DrawLine(_negativeLimit - transform.up * 0.5f, _negativeLimit + transform.up * 0.5f, Color.red);
            }
        }
    }

    public void OnAddEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = effect.amount; }
    }

    public void OnRemoveEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = 1; }
    }

    public void OnStackEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = Mathf.Clamp(_slowMod * effect.amount, 0.33f, 1); }
    }
}
