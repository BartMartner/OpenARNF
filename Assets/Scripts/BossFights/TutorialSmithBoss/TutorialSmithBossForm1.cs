using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TutorialSmithBossForm1 : TutorialSmithBossForm
{
    public Shooter shootAtPlayer;
    public Shooter shootPlayerBurst;
    public Shooter circleShooter;
    public Shooter fragmentShooter;
    public AudioClip summonAudio;

    private SpriteRenderer[] _spriteRenderers;
    private ChildDamagable _childDamagable;
    private BoxCollider2D _boxCollider2D;
    private Figure8Movement _figure8Movement;

    private bool _fadedOut;

    private float _restTimer = 3f;
    private int timesShot;

    protected override void Awake()
    {
        base.Awake();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _childDamagable = GetComponent<ChildDamagable>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _figure8Movement = GetComponentInChildren<Figure8Movement>();

        Debug.Log("TutorialSmithBossForm1 Line 33");

        //set to faded out
        _fadedOut = true;
        _childDamagable.enabled = false;
        _boxCollider2D.enabled = false;
        SetSpriteColors(Color.clear);

        Debug.Log("TutorialSmithBossForm1 Line 41");

        _originalPosition = transform.localPosition;
        
        Debug.Log("TutorialSmithBossForm1 Line 46");

        RefreshHovering();
    }

    public IEnumerator Start()
    {
        _parentController.enemy.altPosition = transform;

        yield return new WaitForSeconds(1);

        while(!enabled)
        {
            yield return null;
        }

        StartCoroutine(FadeIn());
    }

    public void Update()
    {
        if (_acting || _fadedOut) return;

        UpdateHovering();

        _restTimer -= Time.deltaTime;
        if (_restTimer < 0)
        {
            if (timesShot < 2)
            {
                var pattern = Random.Range(0, 4);
                switch (pattern)
                {
                    case 0:
                        StartCoroutine(Figure8Shot());
                        break;
                    case 1:
                        StartCoroutine(ShootPlayerBurst());
                        break;
                    case 2:
                        StartCoroutine(CircleShoot());
                        break;
                    case 3:
                        StartCoroutine(FragmentShoot());
                        break;
                }
                _restTimer = 1.5f;
                timesShot++;
            }
            else
            {
                timesShot = 0;
                StartCoroutine(SpawnMonsters());
                _restTimer = 3f;
            }
        }
    }

    public IEnumerator FragmentShoot()
    {
        _acting = true;

        yield return StartCoroutine(ReturnToCenter());

        _animator.SetTrigger("StartShooting");
        yield return new WaitForSeconds(1);

        fragmentShooter.Shoot();
        yield return new WaitForSeconds(fragmentShooter.burstTime);
        _animator.SetTrigger("EndShooting");

        yield return new WaitForSeconds(2f); // give shots time to clear
        _acting = false;
    }

    public IEnumerator CircleShoot()
    {
        _acting = true;

        yield return StartCoroutine(ReturnToCenter());

        _animator.SetTrigger("StartShooting");
        yield return new WaitForSeconds(1);

        _parentController.RaiseFarPlatforms();
        circleShooter.Shoot();
        yield return new WaitForSeconds(circleShooter.burstTime + 0.25f);
        _animator.SetTrigger("EndShooting");

        _parentController.LowerFarPlatforms();

        _acting = false;
    }

    public IEnumerator ShootPlayerBurst()
    {
        _acting = true;

        yield return StartCoroutine(ReturnToCenter());

        var shootInterval = shootPlayerBurst.burstTime / shootPlayerBurst.burstCount;
        var animationWarmUp = 8f / 12f;

        _animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(animationWarmUp);
        shootPlayerBurst.Shoot();
        for (int i = 0; i < shootPlayerBurst.burstCount -1; i++)
        {
            yield return new WaitForSeconds(shootInterval-animationWarmUp);
            _animator.SetTrigger("Shoot");
            yield return new WaitForSeconds(animationWarmUp);
        }

        _acting = false;
    }

    public IEnumerator Figure8Shot()
    {
        _acting = true;

        yield return StartCoroutine(ReturnToCenter());

        _figure8Movement.enabled = true;

        var time = _figure8Movement.cycleTime * 2;
        var timer = 0f;
        var shotInterval = time / 12f;
        int shotsFired = 0;
        while(timer < time)
        {
            timer += Time.deltaTime;

            if(timer >= shotInterval * (shotsFired+0.5f))
            {
                shootAtPlayer.Shoot();
                shotsFired++;
            }

            yield return null;
        }

        _figure8Movement.enabled = false;

        yield return StartCoroutine(ReturnToCenter());

        _acting = false;
    }

    public void SetSpriteColors(Color color)
    {
        foreach (var renderer in _spriteRenderers)
        {
            if (renderer) { renderer.color = color; }
        }
    }

    public IEnumerator FadeOut()
    {
        _fadedOut = true;

        yield return StartCoroutine(ReturnToCenter());

        _animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);

        _childDamagable.enabled = false;
        _boxCollider2D.enabled = false;
        _parentController.enemy.notTargetable = true;
        var timer = 0f;
        var time = 1f;
        while(timer < time)
        {
            timer += Time.deltaTime;
            SetSpriteColors(Color.Lerp(Color.white, Color.clear, timer / time));
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        _animator.SetTrigger("FadeIn");
        _animator.speed = 0;

        var timer = 0f;
        var time = 1f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            SetSpriteColors(Color.Lerp(Color.clear, Color.white, timer / time));
            yield return null;
        }

        _animator.speed = 1;

        yield return new WaitForSeconds(1f);

        _childDamagable.enabled = true;
        _boxCollider2D.enabled = true;
        _parentController.enemy.notTargetable = false;
        _fadedOut = false;
    }

    public IEnumerator SpawnMonsters()
    {
        _acting = true;

        _audioSource.PlayOneShot(summonAudio);

        yield return StartCoroutine(FadeOut());

        _parentController.SpawnForm1Monsters();

        while (_parentController.enemies.Count > 0)
        {
            _parentController.enemies.RemoveAll(e => !e);
            yield return null;
        };

        yield return StartCoroutine(FadeIn());
        _acting = false;
    }
}
