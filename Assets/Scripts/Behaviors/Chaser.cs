using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Chaser : BaseMovementBehaviour
{
    public float speed = 5;
    public float facingCheckTime = 0.5f;
    private float _facingCheckTimer;
    public bool stopAtEdges;
    private Controller2D _controller2D;
    private Quaternion _targetFacing;
    private bool _stopped;
    private IDamageable _target;
    public UnityEvent onStop;
    public UnityEvent onStartMove;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
    }

    public void FixedUpdate()
    {
        if (!PlayerManager.CanTarget(_target))
        {
            _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
        }

        if (_target == null || !_controller2D.collisions.below)
        {
            _controller2D.Move(transform.up * Physics2D.gravity.y * Time.deltaTime);
            return;
        }

        float targetDeltaX;
        if (!_target.targetable)
        {
            if (!_controller2D.autoTestEdges)
            {
                _controller2D.UpdateRaycastOrigins();
                _controller2D.TestRight();
            }
            targetDeltaX = _controller2D.rightEdge.near ? -transform.right.x : transform.right.x;
        }
        else
        {
            targetDeltaX = _target.position.x - transform.position.x;
        }

        _targetFacing = targetDeltaX > 0 ? Quaternion.identity : Constants.flippedFacing;

        if (_facingCheckTimer < facingCheckTime)
        {
            _facingCheckTimer += Time.deltaTime;
        }
        else
        {
            transform.rotation = _targetFacing;
            _facingCheckTimer = 0;    
        }

        if(!stopAtEdges)
        {
            _controller2D.Move(transform.right * speed * _slowMod * Time.deltaTime);
        }
        else
        {
            var groundAheadAhead = _controller2D.CheckForGroundAhead(0.25f, 0.5f);

            if(groundAheadAhead)
            {
                if(_stopped)
                {
                    _stopped = false;
                    if(onStartMove != null) onStartMove.Invoke();
                }

                _controller2D.Move(transform.right * speed * _slowMod * Time.deltaTime);
            }
            else if(!_stopped)
            {
                _stopped = true;
                if (onStop != null) onStop.Invoke();
            }
        }   
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _stopped = false;
    }
}
