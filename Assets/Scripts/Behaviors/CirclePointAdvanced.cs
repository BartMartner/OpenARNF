using UnityEngine;
using System.Collections;

public class CirclePointAdvanced : BaseMovementBehaviour
{
    public float cycleTime = 2;
    public float baseScale = 1;
    public float perpindicularScale = 1;
    public float parallelScale = 1;
    public float scaleTime = 1;
    public float pivotAngle;
    public bool lockToParent = true;
    public bool randomPhase = true;

    private float _phase;
    private float _2PI = Mathf.PI * 2;
    private Vector3 _pivot;
    private Vector3 _originalLocal;
    private Vector3 _parallelDirection;
    private Vector3 _perpindicularDirection;

    private bool _isRunning;

    private void Awake()
    {
        _isRunning = true;
        _pivot = transform.position;
        _originalLocal = transform.localPosition;
        if (randomPhase)
        {
            _phase = Random.Range(0, _2PI);
        }
    }

    public void Update()
    {
        if (BossFightUI.instance && BossFightUI.instance.getReadyVisible) { return; }
        if (LayoutManager.instance && LayoutManager.instance.transitioning) { return; }

        if (cycleTime == 0)
        {
            return;
        }

        _parallelDirection = Quaternion.Euler(0, 0, pivotAngle) * Vector3.up;
        _perpindicularDirection = Vector3.Cross(_parallelDirection, Vector3.forward);

        if (lockToParent && transform.parent)
        {
            _pivot = transform.parent.TransformPoint(_originalLocal);
        }

        _phase += (_2PI / cycleTime) * Time.deltaTime * _slowMod;

        //not using mod, because cycleTime could be negative...
        if (_phase > _2PI) { _phase -= _2PI; }

        if (_phase < 0) { _phase += _2PI; }

        Vector3 nextPosition = _pivot;
        nextPosition += _parallelDirection * Mathf.Cos(_phase) * baseScale * parallelScale;
        nextPosition += _perpindicularDirection * Mathf.Sin(_phase) * baseScale * perpindicularScale;
        transform.position = nextPosition;
    }

    public void SmoothBaseScale(float newBaseScale)
    {
        if (enabled && gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(LerpBaseScale(newBaseScale));
        }
    }

    public void SmoothBaseScale(float newBaseScale, float newScaleTime)
    {
        scaleTime = newScaleTime;
        if (enabled && gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(LerpBaseScale(newBaseScale));
        }
    }

    public IEnumerator LerpBaseScale(float newBaseScale)
    {
        var timer = 0f;
        var origBaseScale = baseScale;
        while (baseScale != newBaseScale)
        {
            timer += Time.deltaTime * scaleTime;
            baseScale = Mathf.Lerp(origBaseScale, newBaseScale, timer);
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (baseScale == 0) { return; }

        _parallelDirection = Quaternion.Euler(0, 0, pivotAngle) * Vector3.up;
        _perpindicularDirection = Vector3.Cross(_parallelDirection, Vector3.forward);

        Vector3 pivot;
        if (_isRunning)
        {
            pivot = _pivot;
        }
        else
        {
            pivot = transform.position;
        }

        var frame = 1 / 30f;
        var timer = 0f;
        var start = pivot;
        var phase = 0f;
        var isInverted = false;

        while (timer < 2)
        {
            timer += frame;
            phase += _2PI * frame;

            if (phase > _2PI)
            {
                isInverted = !isInverted;
                phase -= _2PI;
            }

            if (phase < 0)
            {
                phase += _2PI;
            }

            Vector3 nextPosition = pivot;
            nextPosition += _parallelDirection * Mathf.Cos(phase) * baseScale * parallelScale;
            nextPosition += _perpindicularDirection * Mathf.Sin(phase) * baseScale * perpindicularScale;
            Debug.DrawLine(start, nextPosition);
            start = nextPosition;
        }
    }
}
