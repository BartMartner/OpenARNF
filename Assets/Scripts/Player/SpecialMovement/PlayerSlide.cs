using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "Slide", menuName = "Player Special Moves/Slide", order = 2)]
public class PlayerSlide : PlayerSpecialMovement
{
    public AudioClip slideStartSound;
    public AudioClip slideSound;
    private Vector2 offset = new Vector3(0, -1.05f, 0);
    private Vector2 size = new Vector3(0.9f, 0.9f);

    [Header("DashSynergy")]
    public PlayerDash playerDash;

    public override void Initialize(Player player)
    {
        _allowGravity = false;
        _priority = 1;
        base.Initialize(player);
        size.x = _player.boxCollider2D.size.x;
        playerDash = _player.specialMoves.Find((m) => m is PlayerDash) as PlayerDash;
        _player.onResetAnimatorAndCollision += OnResetAnimatorAndCollision;
    }

    public override bool TryToActivate()
    {
        if (_player.spiderForm) return false;

        var yAxis = _player.GetYAxis();
        var absXAxis = Mathf.Abs(_player.controller.GetAxis("Horizontal"));
        var absYAxis = Mathf.Abs(yAxis);
        var slideAxis = yAxis < -0.1f && absYAxis > absXAxis;

        if (_player.controller2D.bottomEdge.touching && slideAxis)
        {
            _player.StartCoroutine(Slide());
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator Slide()
    {
        Debug.Log("Slide Start");
        _complete = false;
        _allowJumping = false;

        var timer = 0f;
        var minSlideTime = 0.33f;
        var maxSlideTime = 0.66f;

        _player.SetCollisionBounds(offset, size);

        //attempt to deal with single tile areas
        _player.controller2D.Move(_player.transform.right);
        _player.controller2D.Move(-_player.transform.right);

        _player.animator.SetTrigger("Slide");
        _player.animator.SetBool("SpecialMovement", true);

        float lastSmoke = 0f;
        _player.PlayOneShot(slideStartSound);
        _player.loopingAudio.clip = slideSound;
        _player.loopingAudio.Play();

        if (playerDash) { playerDash.StartSlideDash(); }

        var canStand = _player.CanStand(Vector3.zero);
        _allowJumping = canStand;
        while (!canStand || (!_complete && (timer < minSlideTime || (timer < maxSlideTime && _player.controller.GetButton("SpecialMove")))))
        {
            if (!LayoutManager.instance || !LayoutManager.instance.transitioning)
            {
                timer += Time.deltaTime;
                lastSmoke += Time.deltaTime;
                var velocity = new Vector2(_player.transform.right.x * 12f, _player.gravityFlipped ? 12.81f : -12.81f);
                _player.controller2D.Move(velocity * Time.deltaTime);

                if (_player.controller2D.bottomEdge.touching && lastSmoke > 0.05f)
                {
                    lastSmoke = 0;
                    var smokePos = _player.controller2D.bottomMiddle + (Vector2)_player.transform.right;
                    FXManager.instance.SpawnFX(FXType.SmokePuffSmall, smokePos, false, true);
                }

                if(_player.jumping)
                {
                    DeathStop();
                    var newVelocity = _player.velocity;
                    newVelocity.x = _player.maxSpeed * 3 * _player.transform.right.x;
                    newVelocity.y += 1.5f;
                    _player.velocity = newVelocity;
                    _player.spinJumping = true;
                    yield break;
                }
            }
            yield return null;
            canStand = _player.CanStand(Vector3.zero);
            _allowJumping = canStand;
        }

        _player.loopingAudio.Stop();
        _player.ResetCollisionBounds(true);

        if (playerDash) { playerDash.StopSlideDash(); }

        //a way to deal with weird skipping when standing back up from a slope
        _player.controller2D.Move(Vector2.down * 0.125f);
        _player.animator.SetBool("SpecialMovement", false);
        _complete = true;
        Debug.Log("Slide Complete");
    }

    public override void DeathStop()
    {
        _complete = true;
        _player.loopingAudio.Stop();
        _player.animator.SetBool("SpecialMovement", false);

        if (playerDash)
        {
            playerDash.StopSlideDash();
        }

        Debug.Log("Slide Death Stop");
    }

    public void OnResetAnimatorAndCollision()
    {
        if (playerDash) { playerDash.StopSlideDash(); }
    }

    private void OnDestroy()
    {
        if (_player)
        {
            _player.onResetAnimatorAndCollision -= OnResetAnimatorAndCollision;
        }
    }

    public override void OnPause() { }
    public override void OnUnpause() { }
}
