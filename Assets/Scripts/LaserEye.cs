using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEye : MonoBehaviour, IHasOnDestroy
{
    public LaserStats laserStats;
    public AudioClip blinkSound;
    public AudioClip shootSound;

    public DamageableState state
    {
        get { return _damageable.state; }
    }

    private Animator _animator;
    public Action onDestroy { get; set; }
    private AudioSource _audioSource;
    private Damageable _damageable;

    public void Awake()
    {
        _damageable = GetComponent<Damageable>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponentInParent<AudioSource>();
    }

    public void Blink()
    {
        if (state == DamageableState.Alive)
        {
            _animator.Play("Blink");
            if (_audioSource && blinkSound) _audioSource.PlayOneShot(blinkSound);
        }
    }

    public void Shoot(Vector3 target)
    {
        if (state == DamageableState.Alive)
        {
            StartCoroutine(ShootRoutine(target));
        }
    }

    public void BlinkShoot(Vector3 target)
    {
        StartCoroutine(BlinkShootRoutine(target));
    }

    public IEnumerator BlinkShootRoutine(Vector3 target)
    {
        Blink();
        yield return new WaitForSeconds(0.5f);
        if (state == DamageableState.Alive)
        {
            yield return StartCoroutine(ShootRoutine(target));
        }
    }

    public IEnumerator ShootRoutine(Vector3 target)
    {
        _animator.Play("Shoot");
        yield return new WaitForSeconds(0.583333f);
        if (_audioSource && shootSound) _audioSource.PlayOneShot(shootSound);
        var dir = (target - transform.position).normalized;
        dir.z = 0;
        var angle = Quaternion.FromToRotation(Vector3.right, dir);
        LaserManager.instance.AttachAndFireLaser(laserStats, Vector3.zero, angle, 0.125f, this);
    }

    public void OnDestroy()
    {
        if (onDestroy != null) { onDestroy(); }
    }
}
