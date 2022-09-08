using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Riser : MonoBehaviour
{
    public float riseSpeed = 3f;
    public float dashSpeed = 12f;
    public float acceleration = 0;
    public float dashWait = 0.15f;
    public bool collide = true;
    public string risenSortingOrder = "AboveFadeAways";
    private bool _rising;
    private float _riseTarget;
    private Enemy _enemy;
    private Animator _animator;
    private float _waitCounter;
    private float _actualSpeed;
    private float _riseDistance;
    private SpriteRenderer _spriteRenderer;
    private bool _sortingOrderSet;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (string.IsNullOrEmpty(risenSortingOrder)) { _sortingOrderSet = true; }
    }

    private void Update()
    {
        var prevPos = transform.position;
        if (_rising && Mathf.Abs(transform.position.y - _riseTarget) > float.Epsilon)
        {
            var pos = prevPos;
            pos.y = Mathf.MoveTowards(pos.y, _riseTarget, riseSpeed * Time.deltaTime);
            transform.position = pos;
            _riseDistance += pos.y - prevPos.y;
            if(!_sortingOrderSet && _riseDistance > 1.5f && _spriteRenderer)
            {
                _sortingOrderSet = true;
                _spriteRenderer.sortingLayerName = risenSortingOrder;
            }
            if (Mathf.Abs(pos.y - _riseTarget) <= float.Epsilon) { _animator.Play("Dash"); }
            _actualSpeed = acceleration > 0 ? 0 : dashSpeed;
        }
        else if(_waitCounter < dashWait)
        {
            _waitCounter += Time.deltaTime;
        }
        else
        {
            if(acceleration > 0 && _actualSpeed < dashSpeed)
            {
                _actualSpeed += acceleration * Time.deltaTime;
            }
            var distance = _actualSpeed * Time.deltaTime;
            transform.position += transform.right * distance;
            if (collide)
            {
                var result = Physics2D.CircleCast(prevPos, 0.25f, transform.right, distance, Constants.defaultMask);
                if (result.collider) { _enemy.StartDeath(); }
            }
        }
    }

    public void Rise(float y)
    {
        _animator.Play("Rise");
        _rising = true;
        _riseTarget = y;
        _riseDistance = 0;
    }
}
