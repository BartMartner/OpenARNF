using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WhiteWyrm : MonoBehaviour, IPausable
{
    public float distance = 7;
    public float showTime = 2;    
    public float hideTime = 2;

    public Transform wormBody;
    public LaserShooter laser;
    public Transform topStart;
    public Transform bottomStart;
    public Transform rightStart;
    public Transform leftStart;
    public Transform rubbleLimitLeft;
    public Transform rubbleLimitRight;
    public AudioSource rumbleSound;
    public DangerRubble dangerRubblePrefab;
    public SpriteRenderer[] wyrmRenderers;
    public Material[] damageMaterials;

    public bool showTop;
    public bool showBottom;
    public bool showRight;
    public bool showLeft;

    [Header("Shooting")]
    public ProjectileStats projectileStats;
    public Transform[] shootPoints;

    private Enemy _enemy;
    private Animator _animator;
    private Transform _currentAnchor;

    private bool _visible;
    private float _hiddenDelay = 2f;
    private float _actionTimer;
    private int _currentDamageLevel;
    private float _backAndForthSpeed = 3;

    public void Awake()
    {
        _currentAnchor = topStart;
        _enemy = GetComponent<Enemy>();
        _enemy.onHurt.AddListener(OnHurt);

        _animator = GetComponentInChildren<Animator>();
        wormBody.gameObject.SetActive(false);
    }

    public void Update()
    {
        if(!_visible)
        {
            _actionTimer += Time.deltaTime;
            if(_actionTimer > _hiddenDelay)
            {
                _visible = true;
                _actionTimer = 0f;
                Transform anchor;
                IEnumerator action;

                if (Random.value > 0.5f)
                {
                    if(Random.value > 0.5f)
                    {
                        anchor = topStart;
                        action = PickAction();
                    }
                    else
                    {
                        anchor = bottomStart;
                        action = PickActionBottom();
                    }
                    
                }
                else
                {
                    anchor = Random.value > 0.5f ? leftStart : rightStart;
                    action = PickAction();
                }

                StartCoroutine(ShowHide(anchor, action));
            }
        }
    }

    public void OnHurt()
    {
        var newDamageLevel = 4 - Mathf.CeilToInt((_enemy.health / _enemy.maxHealth) * 4);
        if(newDamageLevel != _currentDamageLevel)
        {
            _currentDamageLevel = newDamageLevel;
            if (_currentDamageLevel < damageMaterials.Length)
            {
                SetMaterial(damageMaterials[_currentDamageLevel]);
            }

            switch (_currentDamageLevel)
            {
                case 0:
                    showTime = 2f;
                    hideTime = 2f;
                    _backAndForthSpeed = 3;
                    _hiddenDelay = 2f;
                    break;
                case 1:
                    showTime = 1.5f;
                    hideTime = 1.5f;
                    _backAndForthSpeed = 4f;
                    _hiddenDelay = 1.75f;
                    break;
                case 2:
                    showTime = 0.75f;
                    hideTime = 1f;
                    _backAndForthSpeed = 5f;
                    _hiddenDelay = 1.25f;
                    break;
                case 3:
                    showTime = 0.25f;
                    hideTime = 0.75f;
                    _backAndForthSpeed = 6f;
                    _hiddenDelay = 0.75f;
                    break;
            }
        }
    }

    public void SetMaterial(Material mat)
    {
        foreach (var r in wyrmRenderers)
        {
            r.material = mat;
        }
    }

    public IEnumerator PickAction()
    {
        var damageMod = Mathf.Clamp(_currentDamageLevel, 0,2);
        var pick = Random.Range(0, 3+damageMod);
        switch (pick)
        {
            case 4:
                return BackAndForthPuke();
            case 3:
                return BackAndForthShoot();
            case 2:
                return Puke(3);
            case 1:
                return Shoot();
            case 0:
            default:
                return Roar();
        }
    }

    public IEnumerator PickActionBottom()
    {
        var damageMod = Mathf.Clamp(_currentDamageLevel-1, 0, 1);
        var pick = Random.Range(0, 3 + damageMod);
        switch (pick)
        {
            case 3:
                return BackAndForthShoot();
            case 2:
                return BackAndForth();
            case 1:
                return Shoot();
            case 0:
            default:
                return Roar();
        }
    }

    public IEnumerator Shoot()
    {
        _animator.Play("Shoot");
        yield return new WaitForSeconds(1); // start animation

        foreach (var shooter in shootPoints)
        {
            var target = PlayerManager.instance.GetClosestPlayerTransform(shooter.position);
            AimingInfo aim = new AimingInfo() { origin = shooter.position, direction = (target.position - shooter.position).normalized };
            ProjectileManager.instance.Shoot(projectileStats, aim);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1); //end animation
    }

    public IEnumerator BackAndForthPuke()
    {
        _animator.SetTrigger("PukeStart");
        yield return new WaitForSeconds(0.5f);
        laser.Shoot();
        yield return StartCoroutine(BackAndForth());
        laser.Stop();
        _animator.SetTrigger("PukeEnd");
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator BackAndForthShoot()
    {
        StartCoroutine(Shoot());
        yield return StartCoroutine(BackAndForth());
    }

    public IEnumerator SpawnDangerRubble(float time, int amount)
    {
        float delay = (time / amount) * 0.5f;
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(delay);
            var point = Vector3.Lerp(rubbleLimitLeft.position, rubbleLimitRight.position, Random.value);
            Instantiate(dangerRubblePrefab, point, Quaternion.identity, transform.parent);
            yield return new WaitForSeconds(delay);
        }
    }

    public IEnumerator BackAndForth()
    {
        var range = (_currentAnchor == topStart || _currentAnchor == bottomStart) ? 7 : 2.5f;
        var origin = wormBody.position;
        var point1 = wormBody.position + wormBody.right * range;
        var point2 = wormBody.position - wormBody.right * range;

        while(Vector2.Distance(wormBody.position, point1) > float.Epsilon)
        {
            wormBody.position = Vector3.MoveTowards(wormBody.position, point1, _backAndForthSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector2.Distance(wormBody.position, point2) > float.Epsilon)
        {
            wormBody.position = Vector3.MoveTowards(wormBody.position, point2, _backAndForthSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector2.Distance(wormBody.position, origin) > float.Epsilon)
        {
            wormBody.position = Vector3.MoveTowards(wormBody.position, origin, _backAndForthSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator Roar()
    {
        _animator.Play("Roar");
        MainCamera.instance.Shake(3,0.2f);
        yield return new WaitForSeconds(5f / 12f);
        yield return StartCoroutine(SpawnDangerRubble(1.5f, 8));
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator Puke(float time)
    {
        _animator.SetTrigger("PukeStart");
        yield return new WaitForSeconds(0.5f);
        laser.Shoot();
        yield return new WaitForSeconds(time - laser.stats.stopTime);
        laser.Stop();
        _animator.SetTrigger("PukeEnd");
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator ShowHide(Transform start, IEnumerator action)
    {
        Debug.Log("ShowHide Start!");
        _currentAnchor = start;

        StartCoroutine(RumbleFadeIn(showTime + 1));
        MainCamera.instance.Shake(showTime+1, 0.1f,4);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SpawnDangerRubble(showTime, 4));
        yield return StartCoroutine(Show());
        StartCoroutine(RumbleFadeOut(0.5f));
        yield return StartCoroutine(action);

        StartCoroutine(RumbleFadeOut(hideTime));
        MainCamera.instance.Shake(hideTime, 0.1f, 4);
        StartCoroutine(SpawnDangerRubble(hideTime-1, 3));
        yield return StartCoroutine(Hide());
        Debug.Log("ShowHide End!");
    }

    private IEnumerator RumbleFadeIn(float time)
    {
        rumbleSound.volume = 0;
        rumbleSound.Play();
        var timer = 0f;
        while(timer < time)
        {
            timer += Time.deltaTime;
            rumbleSound.volume = Mathf.SmoothStep(0, 1, timer / time);
            yield return null;
        }
        rumbleSound.volume = 1;
    }

    private IEnumerator RumbleFadeOut(float time)
    {
        rumbleSound.volume = 1;
        rumbleSound.Play();
        var timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            rumbleSound.volume = Mathf.SmoothStep(1, 0, timer / time);
            yield return null;
        }
        rumbleSound.volume = 0;
        rumbleSound.Stop();
    }

    private IEnumerator Show()
    {
        _visible = true;
        wormBody.gameObject.SetActive(true);

        wormBody.transform.position = _currentAnchor.position;
        wormBody.transform.rotation = _currentAnchor.rotation;

        var timer = 0f;
        var target = _currentAnchor.position - _currentAnchor.up * distance;
        var localShowTime = showTime;

        while(timer < localShowTime)
        {
            timer += Time.deltaTime;
            wormBody.transform.position = Vector3.Slerp(_currentAnchor.position, target, timer / localShowTime);
            yield return null;
        }

        wormBody.transform.position = target;
    }

    private IEnumerator Hide()
    {
        var timer = 0f;
        var start = wormBody.position;

        var localHideTime = hideTime;
        while (timer < localHideTime)
        {
            timer += Time.deltaTime;
            wormBody.transform.position = Vector3.Slerp(start,  _currentAnchor.position, timer / localHideTime);
            yield return null;
        }

        wormBody.gameObject.SetActive(false);
        _visible = false;
    }

    public void Pause()
    {
        StopAllCoroutines();
        _animator.Play("Default");
        if(laser.gameObject.activeInHierarchy)
        {
            laser.Stop();
        }
        enabled = false;
    }

    public void Unpause()
    {
        enabled = true;
    }
}