using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaboomerang : TrailFollower
{
    public DamageType damageType;
    public Action onReturn;
    public float damage = 21f;
    
    private float _initialVelocity = 30;
    private float _velocityFactor = 0;
    private float _baseVelocityFactor = 0.25f;
    private float _acceleration = 36;
    private bool _ranging;
    private bool _respawn;

    private Collider2D _collider2D;
    private Vector3 _direction;
    private float _rangVelocity;
    private bool _returning;
    private SimpleAnimator _simpleAnimator;
    private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;

    private Team _team;
    public override Team team
    {
        get
        {
            return _team;
        }

        set
        {
            Debug.LogWarning("Trying to set team on a Follower. This is not supported");
        }
    }

    private void Awake()
    {
        _collider2D = GetComponentInChildren<Collider2D>();
        _simpleAnimator = GetComponent<SimpleAnimator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    public override IEnumerator Start()
    {
        skipAssignLayer = true;
        yield return base.Start();
        _team = player.team;

        switch (_team)
        {
            case Team.Player:
            case Team.DeathMatch0:
            case Team.DeathMatch1:
            case Team.DeathMatch2:
            case Team.DeathMatch3:
                gameObject.layer = LayerMask.NameToLayer("ProjectileIgnoreTerrain");
                break;
            case Team.Enemy:
                gameObject.layer = LayerMask.NameToLayer("EnemyProjectileIgnoreTerrain");
                break;
        }

        if (_team >= Team.DeathMatch0 && _team <= Team.DeathMatch3)
        {
            if (DeathmatchManager.instance)
            {
                DeathmatchManager.instance.SetCollisionIgnore(_team, _collider2D);
            }
            else
            {
                Debug.LogError("A Boomerang is set to a deathmatch team and there's no DeathmatchManager");
            }
        }
    }

    public override void Update()
    {
        DetermineTarget();
        if (_respawn) return;

        if (_ranging)
        {
            if (_returning)
            {
                _rangVelocity += Time.deltaTime * _acceleration;
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _rangVelocity * Time.deltaTime);
                if (Vector3.Distance(transform.position, _targetPosition) < 0.125f)
                {
                    if (onReturn != null) onReturn();
                    _ranging = false;
                    _simpleAnimator.fps = 0;
                    _simpleAnimator.reverse = false;
                    _simpleAnimator.ResetAndSetFrame();
                }
            }
            else
            {
                transform.position += _direction * _rangVelocity * Time.deltaTime;
                _rangVelocity -= _acceleration * Time.deltaTime;
                if (_rangVelocity < 0)
                {
                    _returning = true;
                    _rangVelocity = 0;
                    _velocityFactor = _baseVelocityFactor;
                    _collider2D.enabled = false;
                    _simpleAnimator.reverse = true;
                }
            }

            return;
        }

        var buttonDown = player.controller.GetButton(player.attackString);        
        _lineRenderer.enabled = buttonDown && !_ranging;

        MoveTorwardsTarget();

        if (buttonDown)
        {
            _simpleAnimator.fps = Mathf.Lerp(0, 36, _velocityFactor);

            if (_velocityFactor < 1) { _velocityFactor += Time.deltaTime * 0.33f; }

            var aimingInfo = player.GetAimingInfo();
            var direction = aimingInfo.direction.normalized;
            var v = _initialVelocity * _velocityFactor;
            var distance = (v * v) / (2 * _acceleration);
            var end = transform.position + (direction * distance);
            _lineRenderer.SetPositions(new Vector3[] { transform.position, end });
        }
        else if (player.controller.GetButtonUp(player.attackString))
        {
            Shoot(player.GetAimingInfo());
        }
    }

    public void Shoot(AimingInfo aimingInfo)
    {
        transform.rotation = player.transform.rotation;
        _ranging = true;
        _direction = aimingInfo.direction.normalized;
        _collider2D.enabled = true;
        _returning = false;
        _rangVelocity = _initialVelocity * _velocityFactor;
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (_ranging)
        {
            var hasTeam = collider.gameObject.GetComponent<IHasTeam>();
            if (hasTeam != null && hasTeam.team == _team) { return; }

            var door = collider.gameObject.GetComponent<Door>();
            if (door && (!door.explosiveShield || !door.explosiveShield.isActiveAndEnabled)) { return; }

            var damageable = collider.gameObject.GetComponent<IDamageable>();

            if (damageable != null && damageable.enabled && damageable.state == DamageableState.Alive)
            {
                var rDamage = damage;
                rDamage *= player.damageMultiplier;
                if (_team == Team.Player && PlayerManager.instance)
                {
                    rDamage *= PlayerManager.instance.coOpMod;
                }
                damageable.Hurt(rDamage, gameObject, damageType, true);
                ProjectileManager.instance.SpawnExplosion(transform.position, Explosion.E48x48, _team, 2 * player.damageMultiplier, false, DamageType.Explosive);
                StartCoroutine(OnHit());
            }
        }
    }
    
    private IEnumerator OnHit()
    {
        _respawn = true;
        _ranging = false;
        _collider2D.enabled = false;
        _spriteRenderer.enabled = false;
        _simpleAnimator.fps = 0;
        _simpleAnimator.ResetAndSetFrame();
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 18; i++)
        {
            transform.position = _targetPosition;
            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            yield return new WaitForSeconds(0.033333f);
        }
        transform.position = _targetPosition;
        _simpleAnimator.fps = 0;
        _simpleAnimator.ResetAndSetFrame();
        _spriteRenderer.enabled = true;
        _velocityFactor = _baseVelocityFactor;
        _respawn = false;
    }
}