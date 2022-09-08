using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformPulse : MonoBehaviour
{
    public float cycleTime;
    public Vector3 scaleMod;
    public bool smoothStep;

    private float _timer;
    private float _halfCycle;
    private Vector3 _originalScale;
    private Vector3 _targetScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void Refresh()
    {
        _originalScale = transform.localScale;
    }
         
    void Update ()
    {
        if(cycleTime <= 0)
        {
            return;
        }

        _timer += Time.deltaTime;
        _targetScale = new Vector3(_originalScale.x * scaleMod.x, _originalScale.y * scaleMod.y, _originalScale.z * scaleMod.z);
        _halfCycle = cycleTime / 2;

        if (_timer > cycleTime)
        {
            _timer -= cycleTime;
        }

        var t = _timer <= _halfCycle ? _timer / _halfCycle : (_timer - _halfCycle) / _halfCycle;

        if (smoothStep) t = Mathf.Lerp(t * t, 1 - ((1-t) * (1-t)), t);

        if (_timer <= _halfCycle)
        {
            transform.localScale = Vector3.Lerp(_originalScale, _targetScale, t);
        }
        else
        {
            transform.localScale = Vector3.Lerp(_targetScale, _originalScale, t);
        }
    }
}
