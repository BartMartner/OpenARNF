using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedMinerTrack : MonoBehaviour
{
    [Range(0, 24)]
    public float limitDistance;
    [Range(0, 1)]
    public float limitOffset = 0.5f;
    public float speed = 5f;
    public SpriteRenderer track;

    private Vector3 _velocity;
    private Vector3 _positiveLimit;
    private Vector3 _negativeLimit;
    private float _setDistance;
    private Direction _direction = Direction.Right;
    private bool _entering;

    protected void Awake()
    {
        SetLimitsBasedOnPosition();
    }

    public void FixedUpdate()
    {
        Move();
    }

    public void Move()
    {
        if (_entering) return;

        _velocity = _direction.ToVector2() * speed;
        transform.position += _velocity * Time.deltaTime;

        bool changeDirection = _setDistance > 0 && Vector3.Distance(transform.position, (_direction.ToVector2().x > 0 ? _negativeLimit : _positiveLimit)) > _setDistance;

        if (changeDirection && Time.timeScale > 0)
        {
            _direction = _direction == Direction.Right ? Direction.Left : Direction.Right;
            track.flipX = _direction == Direction.Left;
        }
    }

    private void SetLimitsBasedOnPosition()
    {
        _setDistance = limitDistance;
        _positiveLimit = transform.position + Vector3.right * _setDistance * (1 - limitOffset);
        _negativeLimit = transform.position - Vector3.right * _setDistance * limitOffset;
    }

    public void OnDrawGizmosSelected()
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

    public IEnumerator EnterRoomRoutine(Vector3 start, Vector3 end)
    {
        _entering = true;

        transform.position = start;
        _direction = start.x > end.x ? Direction.Right : Direction.Left;
        track.flipX = _direction == Direction.Left;

        while (transform.position != end)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);
            yield return null;
        }

        SetLimitsBasedOnPosition();
        _entering = false;
    }
}
