using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class LatchOnPlayer : MonoBehaviour
{
    public Transform latchingTransform;
    private Collider2D _collider2D;
    public bool slowPlayer;
    public bool confusePlayer;
    public float damageAmount;
    public float damageInterval;
    public DamageType damageType;
    private float _damageTimer;
    private Player _player;
    private Enemy _enemy;

    public UnityEvent onLatch;

    public Vector3 targetLocalPosition;
    public float tlpSpeed = 1;
    public float tlpDistanceTolerance = 0;
    public bool snapToLocal;

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _enemy = GetComponentInParent<Enemy>();

        if (!latchingTransform)
        {
            latchingTransform = _enemy ? _enemy.transform : transform;
        }
    }

    private void Update()
    {
        if(_enemy != null && !_enemy)
        {
            enabled = false;
            Debug.LogError("A destroyed enemy is still having update latcher called");
        }

        if(_player && _player.state == DamageableState.Alive) 
        {
            if (damageAmount > 0)
            {
                _damageTimer += Time.deltaTime;
                if (_damageTimer > damageInterval)
                {
                    _damageTimer = 0;
                    _player.Hurt(damageAmount, gameObject, damageType, true);
                }
            }

            if (slowPlayer) { _player.slowed = true; }
            if (confusePlayer && !_player.confused) { _player.confused = true; }

            var delta = targetLocalPosition - latchingTransform.localPosition;
            if (delta.magnitude > tlpDistanceTolerance)
            {
                latchingTransform.localPosition += delta.normalized * tlpSpeed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<Player>();

        if(player)
        {
            _player = player;
            _player.attachedLatchers.Add(this);

            if (damageAmount > 0)
            {
                _player.Hurt(damageAmount, gameObject, damageType, true);
            }

            targetLocalPosition = player.transform.InverseTransformPoint(player.position);

            if(snapToLocal)
            {
                transform.localPosition = targetLocalPosition;
            }

            _collider2D.enabled = false;

            var controller2D = latchingTransform.GetComponent<Controller2D>();
            if(controller2D)
            {
                controller2D.enabled = false;
            }

            latchingTransform.transform.parent = player.transform;
            
            if(onLatch != null)
            {
                onLatch.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        if(_player)
        {
            if (slowPlayer) { _player.slowed = false; }
            if (confusePlayer && _player.confused) { _player.confused = false; }
            _player.attachedLatchers.Remove(this);
        }
    }

    public void Hurt(float damage, GameObject source, DamageType damageType)
    {
        if (_enemy)
        {
            _enemy.Hurt(damage, source, damageType, true);
        }
    }
}
