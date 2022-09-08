using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaBeastDoubleJaw : MonoBehaviour
{
    [Header ("Double Jaw")]
    public Vector3 raycastOffset;
    public float dropSpeed = 15;
    public Transform doubleJaw;
    public AudioClip jawLandSound;
    public AudioClip jawSlerpUp;
    public AudioClip jawSlerpDown;

    [Header("Shooting")]
    public ProjectileStats projectileStats;
    public Transform[] shootPoints;

    private Animator _animator;
    private AudioSource _audioSource;
    private MegaBeastComponent _megaBeastComponent;

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _megaBeastComponent = GetComponent<MegaBeastComponent>();
    }

    public void StartDropSequence()
    {
        StartCoroutine(DropShootRise());
    }

    public IEnumerator DropShootRise()
    {
        if (_megaBeastComponent.megaBeast)
        {
            _megaBeastComponent.megaBeast.animator.SetTrigger("Stop");
        }

        _animator.Play("Warning");

        yield return new WaitForSeconds(2f);

        _animator.Play("DownIdle");
        _audioSource.PlayOneShot(jawSlerpDown);

        var raycast = Physics2D.Raycast(doubleJaw.transform.position + raycastOffset, Vector2.down, 100, LayerMask.GetMask("Default"));
        var target = doubleJaw.transform.position + Vector3.down * raycast.distance;
        while (doubleJaw.transform.position != target)
        {
            doubleJaw.transform.position = Vector3.MoveTowards(doubleJaw.transform.position, target, dropSpeed * Time.deltaTime);
            yield return null;
        }

        _audioSource.PlayOneShot(jawLandSound);

        for (int i = 0; i < 8; i++)
        {
            Vector3 position = doubleJaw.transform.position + raycastOffset + Vector3.Lerp(Vector3.left*2, Vector3.right*2, i/8f);
            FXManager.instance.SpawnFX(FXType.SmokePuffSmall, position, true);
        }

        for (int i = 0; i < 3; i++)
        {
            _animator.Play("Shoot");
            yield return new WaitForSeconds(7f / 12f); // start animation

            foreach (var shooter in shootPoints)
            {
                var shootTarget = PlayerManager.instance.GetClosestPlayerDamageable(shooter.position);
                if (shootTarget != null)
                {
                    AimingInfo aim = new AimingInfo() { origin = shooter.position, direction = (shootTarget.position - shooter.position).normalized };
                    ProjectileManager.instance.Shoot(projectileStats, aim);
                }
                yield return new WaitForSeconds(3f / 12f);
            }

            yield return new WaitForSeconds(10f / 12f); //end animation
        }

        _audioSource.PlayOneShot(jawSlerpUp);
        _animator.Play("DownIdle");

        var velocity = Vector3.zero;
        while (doubleJaw.transform.localPosition != Vector3.zero)
        {
            doubleJaw.transform.localPosition = Vector3.SmoothDamp(doubleJaw.transform.localPosition, Vector3.zero, ref velocity, 1.2f);
            if (Vector3.Distance(doubleJaw.transform.localPosition, Vector3.zero) < 0.5f)
            {
                doubleJaw.transform.localPosition = Vector3.zero;
            }
            yield return null;
        }

        _animator.Play("Close");

        if (_megaBeastComponent.megaBeast)
        {
            _megaBeastComponent.megaBeast.animator.SetTrigger("Go");
        }

        yield return new WaitForSeconds(1f);
    }

    public void OnDrawGizmosSelected()
    {
        if (doubleJaw)
        {
            var pos = doubleJaw.transform.position + raycastOffset;
            Debug.DrawLine(pos + Vector3.up, pos + Vector3.down);
            Debug.DrawLine(pos + Vector3.left, pos + Vector3.right);
        }
    }
}
