using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Events;

public class Projectile : MonoBehaviour, IProjectile, IHasTeam, ILiquidSensitive
{    
    private bool _alive;
    public bool alive { get { return _alive; } }
    public float deathTime = 0.16667f;
    public AudioClip deathSound;
    public bool hasDeathAnimation;
    public List<Sprite> sprites1p5x;
    public List<Sprite> sprites2x;
    public List<Sprite> sprites4x;

    private Animator _animator;
    private Collider2D _collider2D;
    private Vector3 _direction;
    public Vector3 direction { get { return _direction; } }
    private Vector3 _realDirection;
    private Vector3 _tangentDirection;
    private Vector3 _origin;
    private float _speed;
    private float _gravity;
    private ProjectileStats _stats = new ProjectileStats();
    public ProjectileStats stats { get { return _stats; } }

    private float _lifeCounter;
    public float lifeCounter { get { return _lifeCounter; } }

    private SpriteRenderer _renderer;
    new public SpriteRenderer renderer { get { return _renderer; } }

    public float currentSize
    {
        get
        {
            return stats.size + _lifeCounter * (stats.sizePerSecond + _envSizeChangePerSecond);
        }
    }

    public Team team
    {
        get { return _stats.team; }
        set
        {
            _stats.team = value;
            MatchStatsTeam();
        }
    }

    public bool inLiquid { get; set; }
    public bool electrifiesWater { get { return stats != null && stats.damageType.HasFlag(DamageType.Electric); } }

    private Material _material;
    private Color _orignalColor;
    private float _destroyDistance;
    private AnimationCurve _motionPattern;

    public Action onDeath;
    public Action onRecycle;
    public Action onSize;
    public UnityEvent onBounce;

    private Dictionary<float, Dictionary<string, Sprite>> _sizeSprites = new Dictionary<float, Dictionary<string, Sprite>>();
    private Dictionary<string, Sprite> _currentSizeSprites;

    private IEnumerator _deathRoutine;
    private IEnumerator _bounceRoutine;
    private bool _justDeflected;
    private ProjectileSubCollider _subCollider;
    private HashSet<GameObject> _justCollided = new HashSet<GameObject>();
    private Rigidbody2D _rigidbody2D;

    private float _envSizeChangePerSecond;
    private float _envDamageGainPerSecond;

    private void Awake()
    {
        _destroyDistance = 30;
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _material = _renderer.material;
        _orignalColor = _material.color;
        _animator = GetComponentInChildren<Animator>();
        _collider2D = GetComponentInChildren<Collider2D>();
        _subCollider = GetComponentInChildren<ProjectileSubCollider>();
        _rigidbody2D = GetComponent<Rigidbody2D>();

        if (sprites1p5x.Count > 0)
        {
            var dict = new Dictionary<string, Sprite>();
            foreach (var sprite in sprites1p5x) { dict[sprite.name] = sprite; }
            _sizeSprites.Add(1.5f, dict);
        }

        if (sprites2x.Count > 0)
        {
            var dict = new Dictionary<string, Sprite>();
            foreach (var sprite in sprites2x) { dict[sprite.name] = sprite; }
            _sizeSprites.Add(2f, dict);
        }

        if (sprites4x.Count > 0)
        {
            var dict = new Dictionary<string, Sprite>();
            foreach (var sprite in sprites4x) { dict[sprite.name] = sprite; }
            _sizeSprites.Add(4f, dict);
        }
    }

    private void Start()
    {
        if (_animator)
        {
            _animator.logWarnings = false;
        }
    }

    private void Update()
    {
        if (_alive)
        {
            _justCollided.Clear();
            _justDeflected = false;
            _lifeCounter += Time.deltaTime;

            if (_stats.sizePerSecond + _envSizeChangePerSecond != 0) { SetSize(); }

            var dm = DeathmatchManager.instance;
            var camDelta = PlayerManager.instance.cameraPosition - transform.position;
            camDelta.z = 0;
            var offScreen = !dm && (camDelta).sqrMagnitude > _destroyDistance * _destroyDistance;
            if (offScreen || _lifeCounter > _stats.lifeSpan || stats.size + (stats.sizePerSecond + _envSizeChangePerSecond) * _lifeCounter < 0.01f )
            {
                Die(offScreen);
            }
        }
    }

    public void LateUpdate()
    {
        Sprite sprite;
        if (_currentSizeSprites != null && _renderer.sprite != null &&
           _currentSizeSprites.TryGetValue(_renderer.sprite.name, out sprite))
        {
            _renderer.sprite = sprite;
        }
    }

    private void FixedUpdate()
    {
        if (!_alive)
        {
            return;
        }

        if(stats.projectileMotion)
        {
            var newPosition = _origin + (_direction*_speed*_lifeCounter) + (Vector3.down * stats.gravity * 0.5f * _lifeCounter * _lifeCounter);
            newPosition.z = 0;

            if (!_stats.lockRotation && transform.position != newPosition)
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.right, newPosition - transform.position);
            }

            _realDirection = (newPosition - transform.position).normalized;
            if (_rigidbody2D) { _rigidbody2D.MovePosition(newPosition); }
            transform.position = newPosition;
        }
        else if (_motionPattern != null)
        {
            if (_gravity > 0 && _direction.y > -1) { _direction.y -= _gravity * Time.deltaTime; }
            if (_gravity < 0 && _direction.y < 1) { _direction.y -= _gravity * Time.deltaTime; }

            var value = _motionPattern.Evaluate(_lifeCounter);
            var newPosition = _origin + _direction * _lifeCounter * _speed + _tangentDirection * value;
            newPosition.z = 0;

            if (!_stats.lockRotation && transform.position != newPosition)
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.right, newPosition - transform.position);
            }

            if (_rigidbody2D)
            {
                _realDirection = (newPosition - transform.position).normalized;
                _rigidbody2D.MovePosition(newPosition);
            }
            else
            {
                _realDirection = (newPosition - transform.position).normalized;
                transform.position = newPosition;
            }
        }
        else
        {
            if (_stats.homing > 0)
            {
                Vector3 newDirection = _direction;
                float distance;

                if (_stats.team == Team.Enemy)
                {
                    var pPosition = Player.instance.position;
                    pPosition.z = 0;
                    var pAngle = Mathf.Abs(Quaternion.FromToRotation(_direction, (pPosition - transform.position).normalized).eulerAngles.z);
                    if (pAngle > 180) { pAngle = 360 - pAngle; }
                    var pDelta = pPosition - transform.position;
                    pDelta.z = 0;

                    distance = pDelta.magnitude;

                    if (pAngle < _stats.homingArc * 0.5f && pDelta.magnitude < _stats.homingRadius)
                    {
                        newDirection = pDelta.normalized;
                    }
                }
                else
                {
                    var closestEnemy = EnemyManager.instance.GetClosestInArc(transform.position, _direction, _stats.homingRadius, _stats.homingArc);
                    var closest = closestEnemy ? closestEnemy.position : Vector3.zero;
                    float eDistance = closestEnemy ? Vector3.Distance(closest, transform.position) : _stats.homingRadius;
                    Damageable closestDamageable = closestEnemy;

                    if (_stats.team == Team.None)
                    {
                        var pPosition = Player.instance.position;
                        pPosition.z = 0;
                        var pAngle = Mathf.Abs(Quaternion.FromToRotation(_direction, (pPosition - transform.position).normalized).eulerAngles.z);
                        if (pAngle > 180) { pAngle = 360 - pAngle; }
                        var pDistance = Vector3.Distance(pPosition, transform.position);

                        if(pAngle < _stats.homingArc * 0.5f && pDistance < eDistance)
                        {
                            closestDamageable = Player.instance;
                            closest = pPosition;
                        }
                    }

                    distance = Vector3.Distance(transform.position, closest);

                    if (closestDamageable != null && distance <= _stats.homingRadius)
                    {
                        newDirection = (closest - transform.position);
                        newDirection.z = 0;
                        newDirection.Normalize();
                    }
                }

                if (newDirection != _direction)
                {
                    //var homing = Mathf.Lerp(0, _stats.homing, distance / _stats.homingRadius);
                    _direction = Vector3.Lerp(_direction, newDirection, _stats.homing).normalized;
                }

                if (!_stats.lockRotation)
                {
                    transform.rotation = Quaternion.FromToRotation(Vector3.right, _direction);
                }
            }

            if ((_gravity > 0 && _direction.y > -1) || (_gravity < 0 && _direction.y < 1))
            {
                _direction.y -= _gravity * Time.deltaTime;

                if (!_stats.lockRotation)
                {
                    transform.rotation = Quaternion.FromToRotation(Vector3.right, _direction);
                }
            }

            var newPosition = transform.position + _direction * _speed * Time.deltaTime;
            if (_rigidbody2D) { _rigidbody2D.MovePosition(newPosition); }
            //this has to happen regardless, _rigidbody2D.MovePosition seems to happen less frequently / or somehow make the bullet move slower
            _realDirection = (newPosition - transform.position).normalized;
            transform.position = newPosition;
        }
    }

    public void Shoot(ProjectileStats stats, Vector3 origin, Vector3 direction)
    {
        direction.z = 0; //don't shoot bullets on z access

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }

        _alive = true;
        inLiquid = false;
        _envSizeChangePerSecond = 0;
        transform.parent = null;
        transform.position = _origin = origin;
        _stats = stats;
        _direction = direction.normalized;
        _realDirection = _direction;
        _lifeCounter = 0;
        _material.color = _orignalColor;
        _collider2D.enabled = true;
        _justDeflected = stats.isFragment;
        _justCollided.Clear();
        _speed = _stats.speed + UnityEngine.Random.Range(-_stats.speedDeviation, _stats.speedDeviation);
        _gravity = stats.gravity + UnityEngine.Random.Range(-_stats.gravityDeviation, _stats.gravityDeviation);                

        SetSize();

        gameObject.SetActive(true);

        if (stats.shootSound)
        {
            AudioManager.instance.PlayClipAtPoint(stats.shootSound, transform.position, 1, 1, 255);
        }

        if (_renderer)
        {
            if (_stats.preShotInvisTime > 0)
            {
                StartCoroutine(PreShotInvisibility());
            }
            else
            {
                _renderer.enabled = true;
            }
        }

        if (_stats.motionPattern != null && _stats.motionPattern.length > 0)
        {
            _motionPattern = _stats.motionPattern;
            _tangentDirection = Vector3.Cross(Vector3.back, _direction);
        }
        else
        {
            _motionPattern = null;
        }

        if (!stats.lockRotation)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        }

        MatchStatsTeam();
    }

    public void SetSize()
    {
        var size = currentSize;
        var sizeVector = Vector3.one * size;

        if (_subCollider)
        {
            transform.localScale = Vector2.one;
            _subCollider.transform.localScale = sizeVector;
            if (size <= 1.25f)
            {
                _currentSizeSprites = null;
                _renderer.transform.localScale = sizeVector;
            }
            else
            {
                float closestSize;

                if (_sizeSprites != null && _sizeSprites.Count > 0)
                {
                    closestSize = _sizeSprites.Keys.OrderBy(k => Mathf.Abs(k - size)).First();
                    _currentSizeSprites = _sizeSprites[closestSize];
                }
                else
                {
                    _currentSizeSprites = null;
                    closestSize = 1;
                }

                _renderer.transform.localScale = Vector3.one * (size / closestSize);
            }
        }
        else
        {
            transform.localScale = sizeVector;

            if (stats.size <= 1.25f)
            {
                _currentSizeSprites = null;
                _renderer.transform.localScale = Vector2.one;
            }
            else
            {
                if (_sizeSprites.Count == 0)
                {
                    _renderer.transform.localScale = sizeVector;
                }
                else
                {
                    var closestSize = _sizeSprites.Keys.OrderBy(k => Mathf.Abs(k - size)).First();
                    _currentSizeSprites = _sizeSprites[closestSize];
                    _renderer.transform.localScale = Vector2.one * (1 / closestSize);
                }
            }
        }

        if (onSize != null) onSize();
    }

    public void MatchStatsTeam()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");

        if (_subCollider)
        {
            Physics2D.IgnoreCollision(_collider2D, _subCollider.collider2D);
            switch (_stats.team)
            {
                case Team.Enemy:
                    _subCollider.gameObject.layer = LayerMask.NameToLayer("EnemyProjectileIgnoreTerrain");
                    break;
                default:
                    _subCollider.gameObject.layer = LayerMask.NameToLayer("ProjectileIgnoreTerrain");
                    break;
            }
        }

        if (_stats.ignoreTerrain)
        {
            switch (_stats.team)
            {
                case Team.Enemy:
                    gameObject.layer = LayerMask.NameToLayer("EnemyProjectileIgnoreTerrain");
                    break;
                default:
                    gameObject.layer = LayerMask.NameToLayer("ProjectileIgnoreTerrain");
                    break;
            }
        }
        else
        {
            switch (_stats.team)
            {
                case Team.Enemy:
                    gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");
                    break;
                default:
                    gameObject.layer = LayerMask.NameToLayer("Projectile");
                    break;
            }
        }

        if (stats.team >= Team.DeathMatch0 && stats.team <= Team.DeathMatch3)
        {
            if (DeathmatchManager.instance)
            {
                DeathmatchManager.instance.SetCollisionIgnore(stats.team, _collider2D);
                if (_subCollider) { DeathmatchManager.instance.SetCollisionIgnore(stats.team, _subCollider.collider2D); }
            }
            else
            {
                Debug.LogError("A Projectile is set to a deathmatch team and there's no DeathmatchManager");
            }
        }

        //Let this get created after the projectile's layer is set.

        foreach (var childEffect in _stats.childEffects)
        {
            var effect = Instantiate(childEffect, transform);
            effect.Size();
            var eCollider2D = effect.GetComponent<Collider2D>();
            if (eCollider2D) { Physics2D.IgnoreCollision(eCollider2D, _collider2D); }
        }
    }

    public IEnumerator PreShotInvisibility()
    {
        _renderer.enabled = false;
        yield return new WaitForSeconds(stats.preShotInvisTime);
        _renderer.enabled = true;
    }

    public void Die(bool offScreen = false, Vector3? contact = null)
    {
        if (_deathRoutine == null)
        {
            _deathRoutine = DieRoutine(offScreen, contact);
            StartCoroutine(_deathRoutine);
        }
    }

    public IEnumerator DieRoutine(bool offScreen = false, Vector3? contact = null)
    {
        yield return new WaitForEndOfFrame();

        if (_justDeflected || !_alive)
        {
            _deathRoutine = null;
            yield break;
        }

        if (stats.fragment)
        {
            var newStats = new ProjectileStats(stats)
            {
                isFragment = true,
                canOpenDoors = false,
                fragment = stats.fragmentStats.recursion > 0,
                lifeSpan = stats.fragmentStats.lifeSpan,
            };
            newStats.fragmentStats.recursion--;
            newStats.damage *= stats.fragmentStats.damageMod;
            newStats.size *= stats.fragmentStats.sizeMod;
            var pos = transform.position - _direction * 0.125f;
            ProjectileManager.instance.ArcShoot(newStats, pos, -_direction, stats.fragmentStats.amount, stats.fragmentStats.arc);
        }

        if (stats.explosion != Explosion.None)
        {
            var explosion = stats.explosion;
            var damage = stats.explosionDamage * currentSize;

            if (currentSize >= 2)
            {
                switch (stats.explosion)
                {
                    case Explosion.E16x16:
                    case Explosion.E32x32:                    
                    case Explosion.E48x48:
                        var e16 = stats.explosion == Explosion.E16x16;
                        if (currentSize < 3) explosion = e16 ? Explosion.E32x32 : Explosion.E48x48;
                        else if (currentSize < 4) explosion = e16 ? Explosion.E48x48 : Explosion.E64x64;
                        else explosion = Explosion.E64x64;
                        break;
                    case Explosion.Elec16x16:
                    case Explosion.Elec32x32:
                    case Explosion.Elec48x48:
                        var elec16 = stats.explosion == Explosion.Elec16x16;
                        if (currentSize < 3) explosion = elec16 ? Explosion.Elec32x32 : Explosion.Elec48x48;
                        else if (currentSize < 4) explosion = elec16 ? Explosion.Elec48x48 : Explosion.Elec64x64;
                        else explosion = Explosion.Elec64x64;
                        break;
                }
            }

            ProjectileManager.instance.SpawnExplosion(transform.position, explosion, stats.team, damage, !stats.canOpenDoors, stats.damageType);
        }

        _collider2D.enabled = false;

        if (_stats.spawnOnHit)
        {
            var position = transform.position;
            if (_stats.spawnAtContact && contact.HasValue && Vector3.Distance(position, contact.Value) < _stats.size) { position = contact.Value; }
            var effect = Instantiate(_stats.spawnOnHit, position, Quaternion.identity);
            var hasTeam = effect.GetComponent<IHasTeam>();
            if (hasTeam != null)
            {
                hasTeam.team = _stats.team;
            }
            else
            {
                effect.layer = gameObject.layer;
            }
        }

        if (onDeath != null) { onDeath(); }

        _alive = false;

        if (!offScreen && deathSound)
        {
            AudioManager.instance.PlayClipAtPoint(deathSound, transform.position, 1, 1, 255);
        }

        if (_stats.explosion == Explosion.None)
        {
            if (hasDeathAnimation)
            {
                if (_animator) { _animator.SetTrigger("Die"); }
                yield return new WaitForSeconds(deathTime);
            }
            else
            {

                var targetColor = _material.color;
                targetColor.a = 0;
                var halfDeath = deathTime / 2;
                yield return new WaitForSeconds(halfDeath);
                var timer = 0f;
                while (timer < halfDeath)
                {
                    timer += Time.deltaTime;
                    _material.color = Color.Lerp(_orignalColor, targetColor, timer / halfDeath);
                    yield return null;
                }
            }

        }

        Recycle();
    }

    public void Recycle()
    {
        _alive = false;

        if(_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }

        if (_bounceRoutine != null)
        {
            StopCoroutine(_bounceRoutine);
            _bounceRoutine = null;
        }

        _justCollided.Clear();
        _collider2D.enabled = false;
        _stats = null;
        transform.parent = ProjectileManager.instance.transform;
        gameObject.SetActive(false);

        if(onRecycle != null) { onRecycle(); }
    }

    public void HandleCollision(Collider2D collider)
    {
        var colliderGO = collider.gameObject;

        if (!_alive || _justDeflected || _justCollided.Contains(colliderGO) || colliderGO.CompareTag("IgnoreProjectiles")) { return; }

        if (stats.team != Team.None)
        {
            var hasTeam = colliderGO.GetComponent<IHasTeam>();
            if (hasTeam != null && hasTeam.team == stats.team) { return; }
        }

        _justCollided.Add(colliderGO);

        var deflector = colliderGO.GetComponent<ProjectileDeflector>();
        var damagable = colliderGO.GetComponent<IDamageable>();        
        var door = colliderGO.GetComponent<Door>();
        var projectile = colliderGO.GetComponent<IProjectile>();
        var layer = LayerMask.LayerToName(colliderGO.layer);
        bool ignore = (!stats.canOpenDoors && door) || (stats.ignoreDoorLayer && layer == "Door") || 
            (damagable != null && !damagable.enabled) || (projectile != null && (!stats.shootable || !projectile.alive));

        if (ignore) { damagable = null; }

        var damage = _stats.damage + (_stats.damageGainPerSecond + _envDamageGainPerSecond) * lifeCounter;
        if(_stats.team == Team.Player && PlayerManager.instance) { damage *= PlayerManager.instance.coOpMod; }

        Vector3? contact = null;

        if(_stats.spawnAtContact)
        {
            var hit = GetContact(1 << colliderGO.layer);
            if (hit) { contact = hit.point; }
        }

        if (deflector)
        {
            var deflect = true;
            foreach (var d in stats.damageType.GetFlags())
            {
                if (!deflector.deflectDamage.HasFlag(d)) { deflect = false; }
            }

            if(!deflect && damagable != null && damagable.state == DamageableState.Alive &&
                deflector.enabled && deflector.deflectAllUntilDead)
            {
                if (damagable.Hurt(damage, gameObject, _stats.damageType, _stats.ignoreAegis))
                {
                    Debug.Log("Deflecting!");
                    if (_stats.statusEffects != null && stats.statusEffects.Count > 0) damagable.ApplyStatusEffects(_stats.statusEffects, _stats.team);
                    if (_stats.onHurt != null) { _stats.onHurt(damagable); }
                    deflect = true;
                }
            }

            if (deflect && deflector.enabled)
            {
                var hit = GetContact(1 << deflector.gameObject.layer);
                var angle = Vector2.Angle(hit.normal, Vector2.up);

                _stats = new ProjectileStats(stats);
                _stats.homing = 0;

                if(_stats.projectileMotion)
                {
                    _stats.lifeSpan = _stats.lifeSpan - _lifeCounter;
                    _lifeCounter = 0;
                    _origin = transform.position;
                }

                if (deflector.radialDeflection)
                {
                    _direction = -(deflector.transform.position - transform.position);
                    _direction.z = 0;
                    _direction.Normalize();
                    _realDirection = _direction;
                }
                else
                {
                    SetBounceDirectionByNormalAngle(angle);
                }

                if (deflector.setTeam != Team.None)
                {
                    _stats = new ProjectileStats(_stats);
                    team = deflector.setTeam;
                }

                transform.rotation = Quaternion.FromToRotation(Vector3.right, _direction);

                _justDeflected = true;

                deflector.OnDeflect();
            }
            //otherwise ignore the deflector and check for damagable
            else if (damagable != null && damagable.state == DamageableState.Alive)
            {
                if (damagable.Hurt(damage, gameObject, _stats.damageType, _stats.ignoreAegis) && !_stats.penetrative)
                {
                    if (_stats.statusEffects != null && stats.statusEffects.Count > 0) damagable.ApplyStatusEffects(_stats.statusEffects, _stats.team);
                    if (_stats.onHurt != null) { _stats.onHurt(damagable); }
                    Die(false, contact);
                }
            }
            return;
        }

        if (damagable != null && damagable.state == DamageableState.Alive)
        {
            if(damagable.Hurt(damage, gameObject, _stats.damageType, _stats.ignoreAegis))
            {
                if(_stats.statusEffects != null && stats.statusEffects.Count > 0) damagable.ApplyStatusEffects(_stats.statusEffects, _stats.team);
                if (_stats.onHurt != null) { _stats.onHurt(damagable); }
            }
        }

        if (!ignore && (!_stats.penetrative || (damagable == null && !colliderGO.CompareTag("Penetrable"))))
        {
            if (_stats.bounce && damagable == null && projectile == null)
            {
                //Debug.Log("Bounce off " + colliderGO.name);
                if (_bounceRoutine != null) { StopCoroutine(_bounceRoutine); }
                _bounceRoutine = HandleBounce(colliderGO);
                StartCoroutine(_bounceRoutine);
            }
            else
            {
                if (stats.spawnCreep)
                {
                    FXManager.instance.TrySpawnCreep(transform.position, _direction, 1, stats.creepStats, _collider2D.bounds.extents.y);
                }
                Die(false, contact);
            }
        }
    }

    public RaycastHit2D GetContact(int layerMask)
    {
        var previous = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = true;
        var hit = Physics2D.Raycast(transform.position, _realDirection, stats.speed, layerMask);
        Physics2D.queriesHitTriggers = previous;
        return hit;
    }

    public void SetBounceDirectionByNormalAngle(float angle)
    {
        if (angle % 90 == 0)
        {
            if (angle <= 45 || angle >= 135)
            {
                _direction.x = _direction.x >= 0 ? 1 : -1;
                _direction.y = _direction.y >= 0 ? -1 : 1;
            }
            else
            {
                _direction.y = _direction.y >= 0 ? 1 : -1;
                _direction.x = _direction.x > 0 ? -1 : 1;
            }
        }
        else
        {
            _direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
            _direction.z = 0;
        }

        _direction.Normalize();
    }

    private IEnumerator HandleBounce(GameObject colliderGO)
    {
        if (!colliderGO)
        {
            Debug.LogError("Projectile " + gameObject.name + " passed a null colliderGO to HandleBunce");
            yield break;
        }
        
        var extents = _subCollider ? _subCollider.collider2D.bounds.extents.x : _collider2D.bounds.extents.x;
        var bouncing = true;

        while(bouncing && colliderGO)  //keep checking for bounces until the projectile stops intersecting the collider or collider is destroyed
        {
            _collider2D.isTrigger = false; //trick collider into resetting?
            _collider2D.isTrigger = true;
            var hitTriggers = Physics2D.queriesHitTriggers;
            var startInTriggers = Physics2D.queriesStartInColliders;
            Physics2D.queriesHitTriggers = true;
            var hit = Physics2D.Raycast(transform.position, _direction, extents + _speed * Time.deltaTime, 1 << colliderGO.layer);
            Physics2D.queriesHitTriggers = hitTriggers;

            if (hit.collider && hit.collider.gameObject == colliderGO)
            {
                var angle = Vector2.Angle(hit.normal, Vector2.up);
                SetBounceDirectionByNormalAngle(angle);
                transform.rotation = Quaternion.FromToRotation(Vector3.right, _direction);
                if (onBounce != null) { onBounce.Invoke(); }
                yield return null;
            }
            else
            {
                bouncing = false;
            }
        }

        _bounceRoutine = null;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.enabled)
        {
            HandleCollision(collider);
        }
    }

    public bool OnEnterLiquid(Water water)
    {
        if(stats != null)
        {
            switch(stats.type)
            {
                case ProjectileType.FireBolt:
                    inLiquid = true;
                    _envSizeChangePerSecond = -1;
                    _envDamageGainPerSecond = -0.5f;
                    break;
                case ProjectileType.ElectroBolt:
                    inLiquid = true;
                    _envSizeChangePerSecond = 1;
                    _envDamageGainPerSecond = 0.5f;
                    break;
            }
        }

        if (!inLiquid && electrifiesWater) { inLiquid = true; }

        return inLiquid;
    }

    public void OnExitLiquid()
    {
        if (!inLiquid) return;

        if (stats != null)
        {
            switch (stats.type)
            {
                case ProjectileType.FireBolt:
                case ProjectileType.ElectroBolt:
                    _envSizeChangePerSecond = 0;
                    _envDamageGainPerSecond = 0;
                    break;
            }
        }

        inLiquid = false;
    }
}
