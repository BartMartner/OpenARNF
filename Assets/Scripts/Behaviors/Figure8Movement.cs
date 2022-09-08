using UnityEngine;
using System.Collections;

public class Figure8Movement : BaseMovementBehaviour
{
    public float cycleTime = 2;
    public float perpindicularScale = 2;
    public float parallelScale = 3;
    public float pivotAngle;
    public float pivotRotateSpeed = 0f;
    public bool randomPhase = true;
         
    public float phase;
    private float _2PI = Mathf.PI * 2;
    private Vector3 _pivot;
    private Vector3 _originalLocal;
    private Vector3 _pivotOffset;
    private Vector3 _parallelDirection;
    private Vector3 _perpindicularDirection;
    private bool _isInverted = false;    
    private bool _isRunning;

    private void Awake()
    {
        if(randomPhase)
        {
            phase = Random.Range(0, _2PI);
            _isInverted = Random.value > 0.5f;
        }
    }

    protected override void Start()
    {
        base.Start();    
        _originalLocal = transform.localPosition;
        _isRunning = true;
    }

    public void Update()
    {
        if(cycleTime == 0)
        {
            return;
        }

        if(pivotRotateSpeed > 0)
        {
            pivotAngle = (pivotAngle + Time.deltaTime * pivotRotateSpeed) % 360;
        }

        _parallelDirection = Quaternion.Euler(0, 0, pivotAngle) * Vector3.up;
        _perpindicularDirection = Vector3.Cross(_parallelDirection, Vector3.forward);
        _pivotOffset =  _parallelDirection * parallelScale;

        if (transform.parent)
        {
            _pivot = transform.parent.TransformPoint(_originalLocal);
        }
        else
        {
            _pivot = _originalLocal;
        }

        phase += (_2PI / cycleTime) * Time.deltaTime * 2 * _slowMod;

        if (phase > _2PI)
        {
            _isInverted = !_isInverted;
            phase -= _2PI;
        }

        if (phase < 0) { phase += _2PI; }

        Vector3 nextPosition = _pivot + (_isInverted ? _pivotOffset : -_pivotOffset);
        nextPosition += _parallelDirection * Mathf.Cos(phase) * (_isInverted ? -1 : 1) * parallelScale;
        nextPosition += _perpindicularDirection * Mathf.Sin(phase) * perpindicularScale;
        transform.position = nextPosition;
    }

    private void OnDrawGizmosSelected()
    {
        _parallelDirection = Quaternion.Euler(0, 0, pivotAngle) * Vector3.up;
        _perpindicularDirection = Vector3.Cross(_parallelDirection, Vector3.forward);
        _pivotOffset = _parallelDirection * parallelScale;

        Vector3 pivot;
        if(_isRunning)
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

        while(timer < 2)
        {
            timer += frame;
            phase += _2PI * frame;

            if (phase > _2PI)
            {
                isInverted = !isInverted;
                phase -= _2PI;
            }

            if (phase < 0) { phase += _2PI; }

            Vector3 nextPosition = pivot + (isInverted ? _pivotOffset : -_pivotOffset);
            nextPosition += _parallelDirection * Mathf.Cos(phase) * (isInverted ? -1 : 1) * parallelScale;
            nextPosition += _perpindicularDirection * Mathf.Sin(phase) * perpindicularScale;
            Debug.DrawLine(start, nextPosition);
            start = nextPosition;
        }
    }
}