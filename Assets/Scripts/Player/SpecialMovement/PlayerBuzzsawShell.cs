using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuzzsawShell", menuName = "Player Special Moves/Buzzsaw Shell", order = 1)]
public class PlayerBuzzsawShell : PlayerSpecialMovement
{
    public DamageCreatureTrigger spinJumpFXPrefab;
    public AudioClip spinJumpSound;
    public AudioClip rollChargeSound;
    public float damageMultiplier = 8;
    private Vector2 offset = new Vector3(0, -1.05f, 0);
    private Vector2 size = new Vector3(0.75f, 0.85f);
    private DamageCreatureTrigger _spinJumpFXInstance;
    private bool _paused;
    private DamageType? _previousResistances;

    public override void Initialize(Player player)
    {
        _priority = 3;
        base.Initialize(player);
        _spinJumpFXInstance = Instantiate(spinJumpFXPrefab);
        _spinJumpFXInstance.transform.parent = _player.transform;
        _spinJumpFXInstance.transform.localScale = Vector3.one;
        _spinJumpFXInstance.transform.localPosition = Vector3.zero;
        _spinJumpFXInstance.transform.localRotation = Quaternion.identity;
        _spinJumpFXInstance.gameObject.SetActive(false);
        _spinJumpFXInstance.team = _player.team;

        if (_player.team != Team.Player)
        {
            Constants.SetCollisionForTeam(_spinJumpFXInstance.collider2D, _player.team);
        }

        _player.onSpinJumpStart += OnSpinJumpStart;
        _player.onSpinJumpEnd += OnSpinJumpEnd;
        _player.onStartDeath.AddListener(DeathStop);
        _player.onResetAnimatorAndCollision += OnSpinJumpEnd;

        _allowKnockback = false;
        _supressMorph = true;
    }

    public override bool TryToActivate()
    {
        //if (_player.spiderForm) return false;
        if(_player.itemsPossessed.Contains(MajorItem.Arachnomorph) && !_player.spiderForm)
        {
            return false;
        }

        var yAxis = _player.GetYAxis();            
        var absXAxis = Mathf.Abs(_player.controller.GetAxis("Horizontal"));
        var absYAxis = Mathf.Abs(yAxis);
        var slideAxis = yAxis < -0.1f && absYAxis > absXAxis;

        if (_player.controller2D.bottomEdge.touching && slideAxis)
        {
            _player.StartCoroutine(Roll());
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator Roll()
    {
        _complete = false;
        _allowJumping = false;

        _player.SetCollisionBounds(offset, size);

        if(_player.spiderForm)
        {
            _player.animator.SetLayerWeight(1, _player.facing == Direction.Left ? 1 : 0);
            _player.animator.SetLayerWeight(2, 0);
        }

        _player.animator.SetTrigger("SpinDashCharge");
        _player.animator.SetBool("SpecialMovement", true);
        _player.audioSource.PlayOneShot(rollChargeSound);

        FXManager.instance.SpawnFX(FXType.DashPush, _player.controller2D.bottomMiddle, false, false, _player.facing == Direction.Left, _player.gravityFlipped);
        var yAxis = _player.GetYAxis();
        var initialVelocity = 12f;
        var velocityMod = initialVelocity;
        var minVelocity = 4;
        var minButtonVelocity = 6 * _player.speedRatio;
        var maxVelocity = 48 * _player.speedRatio;

        while(yAxis < 0)
        {
            yield return null;
            yAxis = _player.GetYAxis();
            velocityMod -= Time.deltaTime * 2;

            if (_player.controller.GetButtonDown("SpecialMove"))
            {
                velocityMod += 2 * _player.speedRatio;
                velocityMod = Mathf.Clamp(velocityMod, minButtonVelocity, maxVelocity);
                _player.audioSource.pitch = velocityMod / initialVelocity;
                _player.audioSource.PlayOneShot(rollChargeSound);
                FXManager.instance.SpawnFX(FXType.DashPush, _player.controller2D.bottomMiddle, false, false, _player.facing == Direction.Left, _player.gravityFlipped);
            }
            else
            {
                velocityMod = Mathf.Clamp(velocityMod, minVelocity, maxVelocity);
            }
            _player.animator.speed = velocityMod / initialVelocity;
        }

        _player.audioSource.pitch = 1;
        _player.animator.SetTrigger("SpinDash");

        _player.audioSource.PlayOneShot(spinJumpSound);

        var canStand = _player.CanStand(Vector3.zero);
        _allowJumping = canStand;
        _allowTurning = false;
        _allowSpinJumping = false;

        _spinJumpFXInstance.transform.localPosition = offset;
        var damageMod = 1 + ((velocityMod - minVelocity) / maxVelocity) * 4;
        _spinJumpFXInstance.damage = _player.projectileStats.damage * damageMultiplier * damageMod;
        _spinJumpFXInstance.perSecond = true;
        _spinJumpFXInstance.gameObject.SetActive(true);

        var lastPosition = _player.transform.position;
        var stuckCounter = 0;
        var cameraTimer = 0f;

        if(!_previousResistances.HasValue)
        {
            _previousResistances = _player.resistances;
        }
        _player.resistances = Constants.allDamageTypes;

        while (!canStand || velocityMod > 0)
        {
            if(_paused)
            {
                yield return null;
                continue;
            }

            cameraTimer += Time.deltaTime;
            MainCamera.instance.activeTracking = cameraTimer > 0.15f;
            
            if (!LayoutManager.instance || !LayoutManager.instance.transitioning)
            {
                var velocity = new Vector2(_player.transform.right.x * velocityMod, 0);
                _player.controller2D.Move(velocity * Time.deltaTime);
                _allowTurning = _player.jumping;
                _allowMovement = !_player.grounded;
            }

            yield return null;
            canStand = _player.CanStand(new Vector3(0, 0.05f, 0));

            if (Vector3.Distance(_player.transform.position, lastPosition) < 0.01f)
            {
                stuckCounter++;
            }
            else
            {
                stuckCounter = 0;
            }

            if (canStand)
            {
                var drag = Mathf.Clamp(velocityMod / 2, 8, 12);
                velocityMod -= Time.deltaTime * drag;
                var xAxis = _player.GetXAxis();
                if (!_player.grounded || (xAxis != 0 && xAxis != _player.facing.ToInt2D().x))
                {
                    velocityMod -= Time.deltaTime * drag * 2;
                }

                if (_player.controller2D.rightEdge.touching)
                {
                    velocityMod -= Time.deltaTime * drag * 4;
                }
            }
            else
            {
                if (_player.controller2D.rightEdge.touching && stuckCounter > 4)
                {
                    _player.StartCoroutine(_player.ChangeFacing(_player.facing == Direction.Left ? Direction.Right : Direction.Left));
                }

                if (velocityMod < 8)
                {
                    velocityMod += 16 * Time.deltaTime;
                }
            }

            velocityMod = Mathf.Clamp(velocityMod, 0, 32);

            _player.animator.speed = velocityMod / initialVelocity;

            _allowJumping = canStand;

            _player.DamageLatchers(spinJumpFXPrefab.damage, spinJumpFXPrefab.damageType);

            lastPosition = _player.transform.position;
        }

        if (_previousResistances.HasValue)
        {
            _player.resistances = _previousResistances.Value;
            _previousResistances = null;
        }

        MainCamera.instance.activeTracking = true;

        _allowSpinJumping = true;

        _spinJumpFXInstance.gameObject.SetActive(false);
        _spinJumpFXInstance.transform.localPosition = Vector3.zero;

        _player.animator.SetBool("SpecialMovement", false);
        if (_player.spiderForm)
        {
            _player.animator.SetLayerWeight(2, 1);
            _player.animator.SetLayerWeight(1, 0);
            _player.SetSpiderCollisionBounds();
        }
        else
        {
            _player.ResetCollisionBounds(true);
        }

        //a way to deal with weird skipping when standing back up from a slope
        _player.controller2D.Move(Vector2.down * 0.125f);

        _complete = true;
    }

    public override void DeathStop()
    {
        _complete = true;
        _player.audioSource.pitch = 1;
        _player.loopingAudio.Stop();
        _player.animator.SetBool("SpecialMovement", false);
        MainCamera.instance.activeTracking = true;
        _allowSpinJumping = true;
        _spinJumpFXInstance.gameObject.SetActive(false);
        _spinJumpFXInstance.transform.localPosition = Vector3.zero;
    }

    public void OnSpinJumpStart()
    {
        if (!_previousResistances.HasValue)
        {
            _previousResistances = _player.resistances;
        }
        _player.resistances = Constants.allDamageTypes;
        _spinJumpFXInstance.gameObject.SetActive(true);
        _spinJumpFXInstance.damage = _player.projectileStats.damage * damageMultiplier;
        _spinJumpFXInstance.perSecond = true;
        _player.DamageLatchers(1000, DamageType.Velocity);
        _player.audioSource.PlayOneShot(spinJumpSound);
    }

    public void OnSpinJumpEnd()
    {
        _spinJumpFXInstance.gameObject.SetActive(false);
        if(_previousResistances.HasValue)
        {
            _player.resistances = _previousResistances.Value;
            _previousResistances = null;
        }
    }

    private void OnDestroy()
    {
        if (_player)
        {
            _player.audioSource.pitch = 1;
            _player.onSpinJumpStart -= OnSpinJumpStart;
            _player.onSpinJumpEnd -= OnSpinJumpEnd;
            _player.onResetAnimatorAndCollision -= OnSpinJumpEnd;
        }

        if(_spinJumpFXInstance)
        {
            Destroy(_spinJumpFXInstance.gameObject);
        }
    }

    public override void OnPause() { _paused = true; }
    public override void OnUnpause() { _paused = false; }
}
