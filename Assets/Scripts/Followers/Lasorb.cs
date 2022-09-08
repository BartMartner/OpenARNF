using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasorb : TrailFollower
{
    public LaserStats laserStats;
    public float baseDamage;
    public AudioClip laserStartClip;
    private bool _lasering;
    private Laser _laser;
    private float _timer;
    private float _chargeTime = 1.5f;
    private Animator _animator;
    private Flasher _flasher;

    public override IEnumerator Start()
    {
        _animator = GetComponent<Animator>();
        _flasher = gameObject.AddComponent<Flasher>();
        _flasher.SetDefaultFlashColor(Color.white, 0);
        yield return base.Start();
        laserStats.team = player.team;
    }

    public override void Update()
    {
        base.Update();

        _animator.SetFloat("Charge", _timer / _chargeTime);

        if (_lasering) return;

        if (player.controller.GetButton(player.attackString))
        {
            _timer += Time.deltaTime;
            if(!_flasher.flashing && _timer > _chargeTime)
            {
                _flasher.StartFlash(1, 0.25f, Constants.blasterGreen, 0.75f, false);
            }
        }
        else
        {
            if(_timer > _chargeTime)
            {
                StartCoroutine(FireLaser());
            }

            if (_flasher.flashing) { _flasher.StopFlash(); }

            _timer = 0;
        }
    }

    private IEnumerator FireLaser()
    {
        _lasering = true;
        laserStats.damage = baseDamage * player.damageMultiplier;
        _animator.Play("Shoot");
        var duration = 0.5f;
        LaserManager.instance.AttachAndFireLaser(laserStats, transform.right * 0.5f, transform.rotation, duration, this);
        _audioSource.PlayOneShot(laserStartClip);        
        yield return new WaitForSeconds(duration + laserStats.stopTime);
        _lasering = false;
    }
}
