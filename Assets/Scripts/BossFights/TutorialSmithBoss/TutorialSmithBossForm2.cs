using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSmithBossForm2 : TutorialSmithBossForm
{
    public LaserShooter laser;
    public AudioClip laserSound;
    public AudioClip myNameIsDeathSound;

    private float _restTimer = 3f;
    private int timesShot;

    public void Update()
    {
        if (_acting) return;

        UpdateHovering();

        _restTimer -= Time.deltaTime;
        if (_restTimer < 0)
        {
            if (timesShot < 3 || _parentController.enemies.Count > 0)
            {
                var pattern = Random.Range(0, 2);
                switch (pattern)
                {
                    case 0:
                        StartCoroutine(FireLazer());
                        break;
                    case 1:
                        StartCoroutine(Confusion());
                        break;
                }
                _restTimer = 1f;
                timesShot++;
            }
            else
            {
                timesShot = 0;
                StartCoroutine(SummonMonsters());
                _restTimer = 1f;
            }
        }
    }

    public IEnumerator Death()
    {
        _acting = true;
        var timer = 0f;
        var time = 5f;
        MainCamera.instance.Shake(time);

        var explosions = 30f;
        var explosionInterval = time / explosions;
        int currentExplosions = 0;

        while (timer < time)
        {
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(_originalPosition, Vector3.down * 13, timer / time);

            if(timer > currentExplosions * explosionInterval)
            {
                FXManager.instance.SpawnFX(FXType.ExplosionMedium, transform.position + (Vector3)Random.insideUnitCircle * 3);
                currentExplosions++;
            }

            yield return null;    
        }

        _acting = false;
    }

    public IEnumerator Arise()
    {
        _acting = true;
        _audioSource.PlayOneShot(myNameIsDeathSound);
        var timer = 0f;
        var time = 3f;
        var originalPosition = transform.localPosition;
        MainCamera.instance.Shake(time);

        while(timer < time)
        {
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPosition, Vector3.zero, timer / time);
            yield return null;
        }

        _originalPosition = transform.localPosition;
        _acting = false;
    }

    public IEnumerator Confusion ()
    {
        _acting = true;
        yield return StartCoroutine(ReturnToCenter());
        _animator.SetTrigger("Confuse");
        yield return new WaitForSeconds(54f / 12f);
        _acting = false;
    }

    public IEnumerator FireLazer()
    {
        _acting = true;
        yield return StartCoroutine(ReturnToCenter());
        var preTargetDelay = 7f / 18f;
        _animator.SetTrigger("LaserStart");
        yield return new WaitForSeconds(preTargetDelay);

        var delay = (14f / 18f) - preTargetDelay;
        var closestTarget = PlayerManager.instance.GetClosestPlayerTransform(laser.transform.position);
        var targetPosition = closestTarget ? closestTarget.position : laser.transform.position + Vector3.down;
        laser.transform.rotation = Quaternion.FromToRotation(Vector3.right, targetPosition - laser.transform.position);
        yield return new WaitForSeconds(delay);

        laser.Shoot();
        _audioSource.PlayOneShot(laserSound);
        yield return new WaitForSeconds(0.5f);
        _animator.SetTrigger("LaserEnd");
        _acting = false;
    }

    public IEnumerator SummonMonsters()
    {
        _acting = true;
        yield return StartCoroutine(ReturnToCenter());
        _animator.SetTrigger("Summon");
        yield return new WaitForSeconds(10f / 18f);
        _parentController.SpawnForm2Monsters();
        yield return new WaitForSeconds(10f / 18f);
        _acting = false;
    }
}
