using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerEffectTrigger : MonoBehaviour
{
    public bool water;
    public bool slowPlayer;
    public bool confusePlayer;
    public float damageAmount;
    public float damageInterval;
    public float delay;
    public float jumpGravityMod = 1;
    public DamageType damageType;
    public FXType splashFX;
    private float _damageTimer;
    private Player _player;
    private bool _wait;
    private Collider2D _collider2D;

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if(_wait)
        {
            return;
        }

        if (_player && _player.state == DamageableState.Alive)
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
            if (water) { _player.inLiquid = true; }
            if (confusePlayer && !_player.confused) { _player.confused = true; }
        }
    }

    private IEnumerator Wait(float time)
    {
        _wait = true;
        yield return new WaitForSeconds(time);
        _wait = false;
    }

    public void Splash(Collider2D collision)
    {
        if (splashFX != FXType.None)
        {
            var d = collision.Distance(_collider2D);
            if (Mathf.Abs(d.pointA.y - _collider2D.bounds.max.y) < 8f / 16f)
            {
                var point = d.pointA;
                point.y = _collider2D.bounds.max.y;
                FXManager.instance.SpawnFX(splashFX, point);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_wait) { return; }

        Splash(collision);

        var player = collision.gameObject.GetComponent<Player>();

        if (player)
        {
            _player = player;

            if (damageAmount > 0)
            {
                _player.Hurt(damageAmount, gameObject, damageType, true);
            }            

            if(delay > 0)
            {
                StartCoroutine(Wait(delay));
            }
        }
    }

    public void EndEffect()
    {
        if (_player)
        {
            if (slowPlayer)
            {
                _player.slowed = false;
            }

            if(water)
            {
                _player.inLiquid = false;
            }

            if (confusePlayer && _player.confused)
            {
                _player.confused = false;
            }

            if (_player.jumpTimeMod != 1)
            {
                _player.jumpTimeMod = 1;
            }

            _player = null;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<Player>();

        Splash(collision);

        if (player)
        {
            if (delay > 0)
            {
                StartCoroutine(WaitEndEffect(delay));
            }
            else
            {
                EndEffect();
            }
        }
    }

    public IEnumerator WaitEndEffect(float time)
    {
        yield return new WaitForSeconds(time);
        EndEffect();
    }

    private void OnDestroy()
    {
        if (_player)
        {
            if (slowPlayer)
            {
                _player.slowed = false;
            }

            if (confusePlayer && _player.confused)
            {
                _player.confused = false;
            }
        }
    }
}
