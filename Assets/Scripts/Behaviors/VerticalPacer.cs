using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VerticalPacer : BaseMovementBehaviour
{
    public float speed = 5f;
    public bool up;
    public UnityEvent onChangeDirection;

    [Tooltip("Range of 0 means pace until collision")]
    [Range(0,24)]
    public float range;

    private Controller2D _controller2D;
    private Vector3 _lastDirectionFlipPosition;
    
    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        SetPaceRangeCentered();

        if(!_controller2D && range <= 0)
        {
            Debug.LogWarning(gameObject.name + "'s vertical pacer found no _controller2D and has a range of 0");
        }
    }

    public void Update()
    {
        bool changeDirection = false;

        if (_controller2D)
        {
            if (_controller2D.topEdge.touching && _controller2D.bottomEdge.touching)
            {
                return;
            }

            _controller2D.Move((up ? transform.up : -transform.up) * speed * _slowMod * Time.deltaTime);

            changeDirection = (up && _controller2D.topEdge.touching) || (!up && _controller2D.bottomEdge.touching) || (range > 0 && Vector3.Distance(transform.position, _lastDirectionFlipPosition) > range);
        }
        else
        {
            transform.position += (up ? transform.up : -transform.up) * speed * _slowMod * Time.deltaTime;
            changeDirection = (range > 0 && Vector3.Distance(transform.position, _lastDirectionFlipPosition) > range);
        }

        if (changeDirection)
        {
            onChangeDirection.Invoke();
            up = !up;
            _lastDirectionFlipPosition = transform.position;
        }
    }

    public void SetPaceRangeCentered()
    {
        _lastDirectionFlipPosition = transform.position + (up ? -1 : 1) * transform.up* range * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        if(range > 0)
        {
            var up = transform.position + transform.up * 0.5f * range;
            var down = transform.position - transform.up * 0.5f * range;
            Debug.DrawLine(transform.position, up);
            Debug.DrawLine(up - Vector3.right * 0.5f, up + Vector3.right * 0.5f);
            Debug.DrawLine(transform.position, down);
            Debug.DrawLine(down - Vector3.right * 0.5f, down + Vector3.right * 0.5f);
        }
    }
}
