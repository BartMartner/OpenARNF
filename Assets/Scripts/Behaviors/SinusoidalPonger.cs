using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class SinusoidalPonger : BaseMovementBehaviour
{
    public float speed = 5f;
    public float amplitude = 0.5f;
    public float period = 1;
    public bool justSwitchedDirections;
    public Vector3 startingDirection = new Vector3(1, 1, 0);
    private float _sineTime = 0;
    private float _sine;
    private float _lastSine;
    private Controller2D _controller2D;
    private Vector3 _direction;
    private float _yDirection;
    private float _xDirection;

    public bool faceX;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();

        _direction = startingDirection;
        _direction.Normalize();
        _yDirection = Mathf.Abs(_direction.y);
        _xDirection = Mathf.Abs(_direction.x);
    }

    public void Update()
    {
        if (justSwitchedDirections)
        {
            justSwitchedDirections = !(_controller2D.bottomEdge.touching || _controller2D.leftEdge.touching || _controller2D.rightEdge.touching || _controller2D.topEdge.touching);
        }

        if (!justSwitchedDirections)
        {
            if (_controller2D.topEdge.touching)
            {
                _direction.y = -_yDirection;
                justSwitchedDirections = true;
            }
            else if (_controller2D.bottomEdge.touching)
            {
                _direction.y = _yDirection;
                justSwitchedDirections = true;
            }
            else
            {
                _direction.y = _direction.y < 0 ? -_yDirection : _yDirection;
            }

            if (faceX)
            {
                if (_controller2D.rightEdge.touching)
                {
                    if (transform.rotation == Quaternion.identity)
                    {
                        _direction.x = -_xDirection;
                        justSwitchedDirections = true;
                        transform.rotation = Constants.flippedFacing;
                    }
                    else
                    {
                        _direction.x = _xDirection;
                        justSwitchedDirections = true;
                        transform.rotation = Quaternion.identity;
                    }
                }
            }
            else
            {
                if (_controller2D.rightEdge.touching)
                {
                    _direction.x = -_xDirection;
                    justSwitchedDirections = true;
                }
                else if (_controller2D.leftEdge.touching)
                {
                    _direction.x = _xDirection;
                    justSwitchedDirections = true;
                }
                else
                {
                    _direction.x = _direction.x < 0 ? -_xDirection : _xDirection;
                }
            }
        }

        _direction.Normalize();

        _sineTime += Time.deltaTime / period;
        if (_sineTime > 1)
        {
            _sineTime = 0;
        }

        _lastSine = _sine;
        _sine = Mathf.Sin(_sineTime * (2 * Mathf.PI)); //multiply by speed, because that's the distance moved in that amount of time

        var movement = (Vector3.right * speed * _slowMod * Time.deltaTime) + Vector3.up * (_sine - _lastSine) * amplitude;
        movement = Quaternion.FromToRotation(Vector3.right, _direction) * movement;
        _controller2D.Move(movement);

        //if (_controller2D.rightEdge.touching)
        //{
        //  
        //}
    }

    public void OnDrawGizmosSelected()
    {
        var time = 0f;
        var lastPoint = transform.position;
        var sineTime = 0f;
        var sine = 0f;
        var lastSine = 0f;
        var frame = 1 / 60f;

        while (time < period)
        {
            time += frame;

            sineTime += frame / period;

            if (sineTime > 1)
            {
                sineTime = 0;
            }

            lastSine = sine;
            sine = Mathf.Sin(sineTime * (2 * Mathf.PI));

            var movement = (transform.right * speed * frame) + Vector3.up * (sine - lastSine) * amplitude;

            Debug.DrawLine(lastPoint, lastPoint + movement);
            lastPoint = lastPoint + movement;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
