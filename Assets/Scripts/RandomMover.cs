using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMover : MonoBehaviour
{
    public float minChangeDirTime = 2;
    public float maxChangeDirTime = 8;
    public float speed = 5;
    public bool allowRepeatDirection = false;

    [Header("Velocity Based")]
    public bool applyAsVelocity = false;
    public float acceleration = 5;

    private Vector2 _velocity;
    private Controller2D _controller2D;
    private Vector2 _direction;
    private float _changeDirTimer;
    private bool _ready;

    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
    }

    private void Start()
    {
        _controller2D.TestEdges();
        PickDirection();
        _ready = true;
    }

    void Update()
    {
        if (!_ready) return;

        if(applyAsVelocity)
        {
            _velocity = Vector2.MoveTowards(_velocity, _direction * speed, acceleration * Time.deltaTime);
        }
        else
        {
            _velocity = _direction * speed;
        }

        _controller2D.Move(_velocity * Time.deltaTime);
        _changeDirTimer -= Time.deltaTime;
        _controller2D.TestEdges();
        if(_direction == Vector2.zero || _changeDirTimer < 0 || _controller2D.AnyJustTouched())
        {
            PickDirection();
        }
    }

    void PickDirection()
    {
        _changeDirTimer = Random.Range(minChangeDirTime, maxChangeDirTime);
        var directions = new List<Vector2>();
        if (!_controller2D.topEdge.touching) { directions.Add(Vector2.up); }
        if (!_controller2D.bottomEdge.touching) { directions.Add(Vector2.down); }
        if (!_controller2D.leftEdge.touching) { directions.Add(Vector2.left); }
        if (!_controller2D.rightEdge.touching) { directions.Add(Vector2.right); }
        if (!allowRepeatDirection && directions.Count > 0) { directions.Remove(_direction); }

        if (directions.Count > 0)
        {
            _direction = directions[Random.Range(0, directions.Count)];
        }
        else
        {
            _direction = Vector2.zero;
        }
    }

    public void MultiplySpeed(float amount)
    {
        speed *= amount;
    }
}
