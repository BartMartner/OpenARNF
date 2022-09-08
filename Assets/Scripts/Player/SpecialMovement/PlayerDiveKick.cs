using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiveKick", menuName = "Player Special Moves/DiveKick", order = 1)]
public class PlayerDiveKick : PlayerSpecialMovement
{
    public SpriteTrail diveKickFXPrefab;
    public Animator landingPrefab;
    public AudioClip diveKickSound;
    public AudioClip diveKickLandSound;
    public float landDamage;
    public float diveDamage;
    private Animator _landFXInstance;
    private SpriteTrail _diveKickFXInstance;
    private SpriteRenderer[] _diveKickRenderers;
    private DamageCreatureTrigger _diveDamagerTrigger;
    private SpriteRenderer[] _landingRenderers;
    private DamageCreatureTrigger _landDamagerTrigger;
    private SpriteTrail _playerTrail;    
    private float _baseSpeed = 20;

    private IEnumerator _activeKick;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _diveKickFXInstance = Instantiate(diveKickFXPrefab);
        _diveKickFXInstance.transform.parent = _player.transform;
        _diveKickFXInstance.transform.localPosition = Vector3.zero;
        _diveKickFXInstance.transform.localRotation = Quaternion.identity;
        _diveKickFXInstance.gameObject.SetActive(false);
        var sr = _diveKickFXInstance.GetComponent<SpriteRenderer>();
        sr.sortingOrder = _player.mainRenderer.sortingOrder + 1;
        _diveDamagerTrigger = _diveKickFXInstance.GetComponent<DamageCreatureTrigger>();
        _diveDamagerTrigger.team = _player.team;
        _diveDamagerTrigger.onDamageParam += OnDamage;
        _diveKickRenderers = _diveKickFXInstance.GetComponentsInChildren<SpriteRenderer>();

        _playerTrail = new GameObject().AddComponent<SpriteTrail>();
        _playerTrail.name = "Sprite Trail";
        _playerTrail.Copy(_diveKickFXInstance);
        _playerTrail.parentRenderer = _player.GetComponent<SpriteRenderer>();
        _playerTrail.transform.SetParent(_player.transform);
        _playerTrail.transform.localPosition = Vector3.zero;
        _playerTrail.transform.localRotation = Quaternion.identity;
        _playerTrail.gameObject.SetActive(false);

        _landFXInstance = Instantiate(landingPrefab);
        _landFXInstance.transform.parent = _player.transform;
        _landFXInstance.transform.localPosition = Vector3.zero;
        _landFXInstance.transform.localRotation = Quaternion.identity;
        _landDamagerTrigger = _landFXInstance.GetComponentInChildren<DamageCreatureTrigger>(true);
        _landDamagerTrigger.team = _player.team;
        _landingRenderers = _landFXInstance.GetComponentsInChildren<SpriteRenderer>();

        if (_player.team != Team.Player)
        {
            Constants.SetCollisionForTeam(_diveDamagerTrigger.collider2D, _player.team);
            Constants.SetCollisionForTeam(_landDamagerTrigger.collider2D, _player.team);
        }

        _priority = 3;
        _allowMovement = false;
        _allowJumping = false;
        _allowDeceleration = false;
    }

    public override bool TryToActivate()
    {
        if (_player.spiderForm) return false;

        var v = _player.traversalCapabilities.damageTypes.HasFlag(DamageType.Velocity);
        if (v)
        {
            var color = v ? Constants.damageTypeColors[DamageType.Velocity] : Constants.blasterGreen;
            var damageType = v ? DamageType.Velocity | DamageType.Generic : DamageType.Generic;
            foreach (var s in _diveKickRenderers) { s.color = color; }
            foreach (var s in _landingRenderers) { s.color = color; }
            _diveDamagerTrigger.damageType = damageType;
            _landDamagerTrigger.damageType = damageType;
        }

        var yAxis = _player.GetYAxis();
        var absXAxis = Mathf.Abs(_player.controller.GetAxis("Horizontal"));
        var absYAxis = Mathf.Abs(yAxis);
        var downAxis = yAxis < -0.1f && absYAxis + 0.5f > absXAxis;

        if (!_player.controller2D.bottomEdge.near && downAxis)
        {
            _activeKick = DiveKick();
            _player.StartCoroutine(_activeKick);
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator DiveKick()
    {
        _complete = false;
        _allowMovement = false;
        _allowDeceleration = false;
        _diveDamagerTrigger.damage = diveDamage * (1 + (_player.damageMultiplier - 1) * 0.5f);
        _diveKickFXInstance.Start();
        _diveKickFXInstance.parentRenderer.enabled = true;
        _playerTrail.Start();
        _player.animator.SetTrigger("DiveKick");
        _player.animator.SetBool("SpecialMovement", true);
        _player.PlayOneShot(diveKickSound);
        _player.DamageLatchers(_diveDamagerTrigger.damage, _diveDamagerTrigger.damageType);
        _player.invincible = true;

        if (_player.spinJumping) { _player.spinJumping = false; }
        if (_player.crouching) { _player.crouching = false; }

        var speed = _baseSpeed;// + _player.speedUps * 0.5f;

        var velocityOverride = Mathf.Sign(-_player.transform.up.y) * speed;
        bool stop = false;
        int stopFrames = 0;
        while (!stop)
        {
            _player.invincible = true;
            if (!LayoutManager.instance || !LayoutManager.instance.transitioning)
            {
                var v = _player.velocity;
                velocityOverride += _player.gravity * Time.deltaTime;
                v.y = velocityOverride;
                _player.velocity = v;
                if (_player.controller2D.bottomEdge.touching)
                {
                    stopFrames++;
                } 
                else
                {
                    stopFrames = 0;
                }
                stop = stopFrames > 2;
            }

            yield return null;
        }

        //TODO: Spawn Shock Wave
        _activeKick = null;

        _player.StartCoroutine(SpawnShockWave());
        _player.PlayOneShot(diveKickLandSound);
        _player.mainCamera.Shake(0.2f);

        Complete();
    }

    public void Complete()
    {
        _player.animator.SetBool("SpecialMovement", false);
        _diveKickFXInstance.parentRenderer.enabled = false;
        _diveKickFXInstance.Stop();
        _playerTrail.Stop();
        _complete = true;
    }

    public IEnumerator SpawnShockWave()
    {
        _landFXInstance.transform.parent = null;
        _landFXInstance.transform.position = _player.controller2D.bottomMiddle;
        _landDamagerTrigger.damage = landDamage * (1 + (_player.damageMultiplier - 1) * 0.5f);
        _landFXInstance.SetTrigger("Burst");

        yield return new WaitForSeconds(0.1f);

        _player.invincible = false;

        yield return new WaitForSeconds(0.9f);

        if (_landFXInstance)
        {
            _landFXInstance.transform.parent = _player.transform;
            _landFXInstance.transform.localRotation = Quaternion.identity;
        }
    }

    public void OnDamage(IDamageable damageable)
    {
        _player.StartCoroutine(OnDamageRoutine(damageable));
    }

    private IEnumerator OnDamageRoutine(IDamageable damageable)
    {
        yield return new WaitForEndOfFrame();
        if (damageable.state != DamageableState.Alive) yield break;
        if (_player.jumpInputOverride || _player.grounded) yield break;
        
        var door = damageable as Door;
        if (door) yield break;

        if (_activeKick != null)
        {
            _player.StopCoroutine(_activeKick);
            _activeKick = null;
            Complete();
        }

        _player.jumping = true;
        _player.jumpInputOverride = true;
        var velocity = _player.velocity;
        velocity.y = _player.transform.up.y * _player.maxJumpVeloctiy;
        _player.velocity = velocity;

        var timer = 0f;
        var time = _player.timeToJumpApex;
        bool _setInvincibility = false;

        while (timer < time)
        {
            if (timer > 0.1f && !_setInvincibility)
            {
                _setInvincibility = true;
                _player.invincible = false;
            }

            if (_player.controller2D.topEdge.touching)
            {
                _player.jumpInputOverride = false;
                yield return new WaitForSeconds(0.5f); //Waiting 1/2 second prevents player from repeatedly jumping off updog in 3 unity high hall
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        _player.invincible = false;
        _player.jumpInputOverride = false;
    }

    public override void DeathStop()
    {
        _complete = true;
        if (_activeKick != null)
        {
            _player.StopCoroutine(_activeKick);
            _activeKick = null;
        }
        _player.animator.SetBool("SpecialMovement", false);
        _diveKickFXInstance.parentRenderer.enabled = false;
        _diveKickFXInstance.Stop();
        _playerTrail.Stop();
    }

    public void OnDestroy()
    {
        if (_landFXInstance)
        {
            Destroy(_landFXInstance.gameObject);
        }

        if (_diveKickFXInstance)
        {
            Destroy(_diveKickFXInstance.gameObject);
        }

        if (_playerTrail)
        {
            Destroy(_playerTrail.gameObject);
        }
    }

    public override void OnPause() { }
    public override void OnUnpause() { }
}
