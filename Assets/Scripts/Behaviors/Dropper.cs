using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

[RequireComponent(typeof(Controller2D))]
public class Dropper : BaseMovementBehaviour
{
    public LayerMask dropLayerMask;
    public float dropWait;
    public Vector2 raycastOffset;
    public float acceleration = 12;
    public bool flipOnDropStart;
    public bool rotateOnDropStart;
    public float speed = 5f;

    [Header("Events")]
    public UnityEvent onDropStart;
    public UnityEvent onDropWaitEnd;
    public UnityEvent onDropEnd;

    private Controller2D _controller2D;
    private bool _dropping, _waitingToDrop;
    private float _currentSpeed;

    protected override void Start()
    {
        base.Start();
        _controller2D = GetComponent<Controller2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        if(_currentSpeed < speed)
        {
            _currentSpeed += acceleration * Time.deltaTime;
        }

        if (_dropping)
        {
            _controller2D.Move(Vector2.down * Time.deltaTime * speed);

            if (_controller2D.collisions.below)
            {
                if (onDropEnd != null)
                {
                    onDropEnd.Invoke();
                }

                enabled = false;
            }
        }
        else if (!_waitingToDrop)
        {
            var raycast = Physics2D.Raycast((Vector2)transform.position + raycastOffset, Vector2.down, 100, dropLayerMask);
            if (raycast.transform)
            {
                var player = raycast.transform.GetComponent<Player>();
                if (player && !player.notTargetable)
                {
                    Drop();
                }
            }
        }
    }

    public void Drop()
    {
        if(enabled && ! _dropping && !_waitingToDrop)
        {
            if (onDropStart != null)
            {
                onDropStart.Invoke();
            }

            StartCoroutine(WaitToDrop());
        }
    }

    private IEnumerator WaitToDrop()
    {
        _waitingToDrop = true;
        if (dropWait > 0)
        {
            yield return new WaitForSeconds(dropWait);
        }

        if (onDropWaitEnd != null)
        {
            onDropWaitEnd.Invoke();
        }
        _waitingToDrop = false;
        _dropping = true;

        if (flipOnDropStart)
        {
            var scale = transform.localScale;
            scale.y *= -1;
            transform.localScale = scale;
        }

        if(rotateOnDropStart)
        {
            transform.rotation *= Quaternion.Euler(0, 0, -180);
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawIcon(transform.position + (Vector3)raycastOffset, "DownArrow", true);
    }
}
