using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalseBeastProjector : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject falseBeast;
    public AudioSource projectorSound;
    public float minBeastSpeed = 2;
    public float hurtMod = 4;

    private SinusoidalPacer _movement;
    private SimpleAnimator _beastAnimator;
    private Gravitator _beastMove;
    private Animator _animator;

    private bool _dying;
    private bool _surging;
    private float _surgeTime;

    private float _flickerTimer;
    private float _flickerCycleTime = 1 / 12f;
    private Color _origStartColor;
    private Color _startColorLow;
    private Color _surgeStartColorHigh;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _beastAnimator = falseBeast.GetComponent<SimpleAnimator>();
        _beastMove = falseBeast.GetComponent<Gravitator>();
        _movement = GetComponent<SinusoidalPacer>();

        _beastAnimator.fps = 6;
        _beastMove.maxMagnitude = minBeastSpeed;

        lineRenderer.sortingLayerName = "AboveFadeAways";
        lineRenderer.sortingOrder = 1;

        _origStartColor = lineRenderer.startColor;
        _startColorLow = _origStartColor;
        _startColorLow.a = 0.25f;
        _surgeStartColorHigh = Color.white;
        _surgeStartColorHigh.a = 0.9f;
    }

    public void LineToBeast()
    {
        if(lineRenderer && falseBeast)
        {
            lineRenderer.SetPositions(new Vector3[] { lineRenderer.transform.position, falseBeast.transform.position });
        }
    }

    public void TrySurge()
    {
        if (_dying) return;

        _surgeTime += 0.25f;

        if(!_surging)
        {
            _surgeTime += 0.5f;
            StartCoroutine(Surge());
        }
    }

    public IEnumerator Surge()
    {
        _surging = true;
        _animator.SetTrigger("StartSurge");
        _movement.enabled = false;

        var timer = 0f;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            _beastAnimator.fps = Mathf.Lerp(6, 6 * hurtMod, timer / 0.5f);
            _beastMove.maxMagnitude = Mathf.Lerp(minBeastSpeed, minBeastSpeed * hurtMod, timer / 0.5f);
            projectorSound.pitch = Mathf.Lerp(1, hurtMod, timer / 0.5f);
            yield return null;
        }

        timer = 0f;
        while (timer < _surgeTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _animator.SetTrigger("EndSurge");

        timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            _beastAnimator.fps = Mathf.Lerp(6 * hurtMod, 6, timer / 0.5f);
            _beastMove.maxMagnitude = Mathf.Lerp(hurtMod*minBeastSpeed, minBeastSpeed, timer / 0.5f);
            projectorSound.pitch = Mathf.Lerp(hurtMod, 1, timer / 0.5f);
            yield return null;
        }

        _movement.enabled = true;
        _surging = false;
    }

    public void OnStartDeath()
    {
        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    public IEnumerator DeathRoutine()
    {
        _movement.enabled = false;
        _dying = true;
        _surging = false;
        projectorSound.Stop();

        _animator.Play("StartSurge");

        yield return new WaitForSeconds(0.1f);
        falseBeast.gameObject.SetActive(false);
        lineRenderer.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        falseBeast.gameObject.SetActive(true);
        lineRenderer.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        falseBeast.gameObject.SetActive(true);
        lineRenderer.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        falseBeast.gameObject.SetActive(false);
        lineRenderer.gameObject.SetActive(false);

        var active = true;
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.05f);
            falseBeast.gameObject.SetActive(active);
            lineRenderer.gameObject.SetActive(active);
            active = !active;
        }

        Destroy(falseBeast);
        Destroy(lineRenderer);
    }

    public void Update()
    {
        LineToBeast();

        if (_flickerCycleTime <= 0)
        {
            return;
        }

        if (lineRenderer)
        {
            _flickerTimer += Time.deltaTime;
            var _halfCycle = _flickerCycleTime / 2;

            if (_flickerTimer > _flickerCycleTime)
            {
                _flickerTimer -= _flickerCycleTime;
            }

            var high = _surging ? _surgeStartColorHigh : _origStartColor;

            if (_flickerTimer <= _halfCycle)
            {
                lineRenderer.startColor = Color.Lerp(high, _startColorLow, _flickerTimer / _halfCycle);
            }
            else
            {
                lineRenderer.startColor = Color.Lerp(_startColorLow, high, (_flickerTimer - _halfCycle) / _halfCycle);
            }
        }
    }

    private void OnDrawGizmos()
    {
        LineToBeast();
    }
}
