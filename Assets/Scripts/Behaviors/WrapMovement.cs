using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapMovement : BaseMovementBehaviour
{
    private Renderer _renderer;
    private Collider2D _collider2D;
    private Room _room;
    private bool _hasEntered;

    public bool useCollider;
    public bool applyTransform;
    public bool swapDirection;
    public float speed;
    public float sineAmplitude = 0;
    public float sinePeriod = 0;
    public Vector3 direction;

    private float _sineTime = 0;
    private float _sine;
    private float _lastSine;

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _renderer = GetComponent<Renderer>();
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(OnStart());
    }

    public IEnumerator OnStart()
    {
        if (LayoutManager.instance && LayoutManager.instance.transitioning) { yield return null; }
        _room = GetComponentInParent<Room>() ?? LayoutManager.CurrentRoom;
        if (!_room) { _room = FindObjectOfType<Room>(); }

        if(_room)
        {
            var bounds = useCollider ? _collider2D.bounds : _renderer.bounds;
            _hasEntered = _room.worldBounds.Intersects(bounds);
        }
    }

    public void Update()
    {
        if (_room)
        {
            var tDirection = applyTransform ? transform.TransformDirection(direction.normalized) : direction.normalized;
            var movement = tDirection * speed * _slowMod * Time.deltaTime;

            if (sineAmplitude > 0)
            {
                _sineTime += Time.deltaTime / sinePeriod;
                if (_sineTime > 1) { _sineTime = 0; }
                _lastSine = _sine;
                _sine = Mathf.Sin(_sineTime * (2 * Mathf.PI)); //multiply by speed, because that's the distance moved in that amount of time
                movement += Vector3.Cross(Vector3.forward, tDirection) * (_sine - _lastSine) * sineAmplitude;
            }
            
            transform.position += movement;

            var bounds = useCollider ? _collider2D.bounds : _renderer.bounds;

            var worldBounds = _room.worldBounds;
            var inRoom = worldBounds.Intersects(bounds);

            if (!_hasEntered)
            {
                _hasEntered = inRoom;
            }
            else if (!inRoom)
            {
                _hasEntered = false;
                var position = transform.position;
                if (position.x > worldBounds.max.x || position.x < worldBounds.min.x)
                {
                    var distanceFromCenter = Mathf.Abs(worldBounds.center.x - position.x);
                    position.x = worldBounds.center.x + ((position.x < worldBounds.center.x) ? distanceFromCenter : -distanceFromCenter);
                    if (swapDirection) direction.y = -direction.y;
                }

                if (position.y > worldBounds.max.y || position.y < worldBounds.min.y)
                {
                    var distanceFromCenter = Mathf.Abs(worldBounds.center.y - position.y);
                    position.y = worldBounds.center.y + ((position.y < worldBounds.center.y) ? distanceFromCenter : -distanceFromCenter);
                    if (swapDirection) direction.x = -direction.x;
                }

                transform.position = position;
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        Debug.DrawLine(transform.position, transform.position + direction.normalized * speed);
    }
}
