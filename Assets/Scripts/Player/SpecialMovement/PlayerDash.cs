using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "Dash", menuName = "Player Special Moves/Dash", order = 1)]
public class PlayerDash : PlayerSpecialMovement
{
    public SpriteTrail dashFXPrefab;
    public SpriteTrail slideDashFXPrefab;
    public AudioClip dashSound;
    public float damage;
    private SpriteTrail _dashFXInstance;
    private SpriteTrail _slideDashFXInstance;
    private SpriteTrail _playerTrail;
    private DamageCreatureTrigger _damagerTrigger;
    private DamageCreatureTrigger _slideDamagerTrigger;
    private float _baseSpeed = 15;
    private IEnumerator _dashRoutine;
    private DamageType _previousResistances;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _dashFXInstance = Instantiate(dashFXPrefab);
        _dashFXInstance.transform.parent = _player.transform;
        _dashFXInstance.transform.localPosition = Vector3.zero;
        _dashFXInstance.transform.localRotation = Quaternion.identity;
        _dashFXInstance.gameObject.SetActive(false);
        _damagerTrigger = _dashFXInstance.GetComponent<DamageCreatureTrigger>();
        _damagerTrigger.team = _player.team;

         _slideDashFXInstance = Instantiate(slideDashFXPrefab);
        _slideDashFXInstance.transform.parent = _player.transform;
        _slideDashFXInstance.transform.localPosition = Vector3.zero;
        _slideDashFXInstance.transform.localRotation = Quaternion.identity;
        _slideDashFXInstance.gameObject.SetActive(false);
        _slideDamagerTrigger = _slideDashFXInstance.GetComponent<DamageCreatureTrigger>();
        _slideDamagerTrigger.team = _player.team;

        if (_player.team != Team.Player)
        {
            Constants.SetCollisionForTeam(_damagerTrigger.collider2D, _player.team);
            Constants.SetCollisionForTeam(_slideDamagerTrigger.collider2D, _player.team);
        }
        
        _playerTrail = new GameObject().AddComponent<SpriteTrail>();
        _playerTrail.name = "Sprite Trail";
        _playerTrail.spawnDistance = _dashFXInstance.spawnDistance;
        _playerTrail.fadeTime = _dashFXInstance.fadeTime;
        _playerTrail.parentRenderer = _player.GetComponent<SpriteRenderer>();
        _playerTrail.transform.SetParent(_player.transform);
        _playerTrail.transform.localPosition = Vector3.zero;
        _playerTrail.transform.localRotation = Quaternion.identity;
        _playerTrail.gameObject.SetActive(false);

        _allowJumping = true;
        _allowDeceleration = false;

        //slide synergy
        var slide = _player.specialMoves.Find((m) => m is PlayerSlide) as PlayerSlide;
        if (slide)
        {
            slide.playerDash = this;
        }
    }

    public override bool TryToActivate()
    {
        if (_player.spiderForm) return false;

        _dashRoutine = Dash();
        _player.StartCoroutine(_dashRoutine);
        return true;
    }

    public IEnumerator Dash()
    {
        _complete = false;
        _allowMovement = false;
        _allowDeceleration = false;
        _damagerTrigger.damage = damage * (1 + (_player.damageMultiplier-1) * 0.33f);
        _dashFXInstance.Start();
        _dashFXInstance.parentRenderer.enabled = true;
        _playerTrail.Start();
        _player.animator.SetTrigger("Dash");
        _player.animator.SetBool("SpecialMovement", true);
        _player.PlayOneShot(dashSound);
        _player.DamageLatchers(_damagerTrigger.damage, _damagerTrigger.damageType);

        if (_player.spinJumping)
        {
            _player.spinJumping = false;
        }

        if(_player.crouching)
        {
            _player.crouching = false;
        }

        var speed = _baseSpeed + _player.speedUps * 0.5f;

        var timer = 0f;
        var dashTime = 0.33f;
        var mustDash = dashTime * 0.25f;

        if (_previousResistances <= 0) { _previousResistances = _player.resistances; }
        _player.resistances = Constants.allDamageTypes;
        while ((_player.controller.GetButton("SpecialMove") || timer < mustDash) && timer < dashTime)
        {
            if (!LayoutManager.instance || !LayoutManager.instance.transitioning)
            {
                if(timer > 0.03 && timer <= mustDash && _player.controller.GetButtonDown(_player.jumpString))
                {
                    var v = _player.velocity;
                    if (v.y > 0) { v.y = v.y * 1.175f; }
                    _player.velocity = v;
                }

                timer += Time.deltaTime;
                _player.controller2D.Move(_player.transform.right * speed * Time.deltaTime);
                if (_player.controller2D.bottomEdge.touching)
                {
                    FXManager.instance.SpawnFX(FXType.SmokePuffSmall, _player.controller2D.bottomMiddle, false, true);
                }
            }

            yield return null;
        }
        _player.resistances = _previousResistances;
        _previousResistances = 0;

        _player.animator.SetBool("SpecialMovement", false);
        _dashFXInstance.parentRenderer.enabled = false;
        _dashFXInstance.Stop();
        _playerTrail.Stop();

        var xAxis = _player.controller.GetAxis("Horizontal");
        if (_player.confused) { xAxis = -xAxis; }

        if (Mathf.Abs(xAxis) > 0.1f)
        {
            var v = _player.velocity;
            v.x = speed * Mathf.Sign(xAxis);
            _player.velocity = v;
        }

        _allowMovement = true;
        _allowDeceleration = true;

        while (timer < dashTime * 0.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _allowMovement = false;
        _allowDeceleration = false;

        _complete = true;
        _dashRoutine = null;
    }

    public void StartSlideDash()
    {
        _slideDamagerTrigger.damage = damage * (1 + (_player.damageMultiplier - 1) * 0.5f);
        _slideDashFXInstance.Start();
        _slideDashFXInstance.parentRenderer.enabled = true;
        _playerTrail.Start();
        _player.PlayOneShot(dashSound);
    }

    public void StopSlideDash()
    {
        _slideDashFXInstance.parentRenderer.enabled = false;
        _slideDashFXInstance.Stop();
        _playerTrail.Stop();

        if(_player.jumping)
        {
            TryToActivate();
        }
    }

    public override void DeathStop()
    {
        _complete = true;
        if (_dashRoutine != null)
        {
            _player.StopCoroutine(_dashRoutine);
            _dashRoutine = null;
        }
        _player.animator.SetBool("SpecialMovement", false);
        _dashFXInstance.parentRenderer.enabled = false;
        _dashFXInstance.Stop();
        _slideDashFXInstance.parentRenderer.enabled = false;
        _slideDashFXInstance.Stop();
        _playerTrail.Stop();
    }

    public void OnDestroy()
    {
        if (_dashFXInstance)
        {
            Destroy(_dashFXInstance.gameObject);
        }

        if (_slideDashFXInstance)
        {
            Destroy(_slideDashFXInstance.gameObject);
        }

        if(_playerTrail)
        {
            Destroy(_playerTrail.gameObject);
        }
    }

    public override void OnPause() { }
    public override void OnUnpause() { }
}

