using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class SinusoidalPacer : BaseMovementBehaviour
{
    public float speed = 5f;
    public float amplitude = 0.5f;
    public float period = 1;
    public float limit = 0;
    public bool flip = true;
    private float _sineTime = 0;
    private float _sine;
    private float _lastSine;
    private Vector3 _lastFlipPoint;
    private Vector3 _direction;
    private Controller2D _controller2D;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _direction = transform.right;
    }

    public void Update()
    {
        if (_controller2D.leftEdge.touching && _controller2D.rightEdge.touching)
        {
            return;
        }

        _sineTime += Time.deltaTime/period;
        if (_sineTime > 1) { _sineTime = 0; }

        _lastSine = _sine;
        _sine = Mathf.Sin(_sineTime * (2 * Mathf.PI)); //multiply by speed, because that's the distance moved in that amount of time

        var movement = (_direction * speed * _slowMod * Time.deltaTime) + Vector3.up * (_sine - _lastSine) * amplitude;
        _controller2D.Move(movement);

        var hitLimit = (limit > 0 && Vector3.Distance(transform.position, _lastFlipPoint) > limit);
        if (flip && (_controller2D.rightEdge.touching || hitLimit))
        {
            var eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y == 0 ? 180 : 0, eulerRotation.z);
            _direction = transform.right;
            _lastFlipPoint = transform.position;
        }
        else if (!flip && (hitLimit || (_direction == Vector3.right && _controller2D.rightEdge.touching) || (_direction == Vector3.left && _controller2D.leftEdge.touching)))
        {
            _direction = _direction * -1;
            _lastFlipPoint = transform.position;
        }
    }

    public void OnDrawGizmosSelected()
    {
        var timer = 0f;
        var lastPoint = transform.position;
        var sineTime = 0f;
        var sine = 0f;
        var lastSine = 0f;
        var frame = 1 / 60f;

        var time = limit <= 0 ? period : limit/speed;

        while (timer < time)
        {
            timer += frame;

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
}
