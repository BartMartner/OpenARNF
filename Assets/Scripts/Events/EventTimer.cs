using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class EventTimer : MonoBehaviour, IPausable
{
    [Range(0, 1)]
    public float preWarm;

    public float eventDelay;
    public float eventWarmUp;
    public float eventDuration;
    public bool resetTimerOnPause = true;
    public bool resetPreWarmOnPause = false;
    public bool randomPreWarm;
    public bool startPaused;
    public bool waitUntilVisible;
    public bool doNotRepeat;
    public bool doNotFireIfNotVisible;
    public Renderer visibilityRenderer;
    public Collider2D visibilityBounds;

    public UnityEvent onEventWarmUp;
    public UnityEvent onEventStart;
    public UnityEvent onEventEnd;

    private bool _eventCycleActive;
    public bool eventCycleActive
    {
        get { return _eventCycleActive; }
    }

    [Tooltip("Only link object that impliment IPermitEvent")]
    public GameObject[] permissionChecks;
    private List<IPermitEvent> _permissionChecks = new List<IPermitEvent>();

    private float _eventTimer;

    private bool _paused;
    private bool _canCount;

    private void Awake()
    {
        if (permissionChecks != null)
        {
            foreach (var p in permissionChecks)
            {
                var pCheck = p.GetComponent<IPermitEvent>();
                if (pCheck != null)
                {
                    _permissionChecks.Add(pCheck);
                }
            }
        }

        _canCount = !waitUntilVisible;
    }

    public void Start()
    {
        if(randomPreWarm)
        {
            preWarm = Random.value * 0.5f;
        }

        _eventTimer = preWarm * eventDelay;

        if(startPaused)
        {
            Pause();
        }
    }

    public void Update()
    {
        if (!_canCount && waitUntilVisible) { _canCount = Visible(); }

        if (!_eventCycleActive && _canCount)
        {
            if (_eventTimer < eventDelay)
            {
                _eventTimer += Time.deltaTime;
            }
            else
            {
                if (!doNotFireIfNotVisible || Visible())
                {
                    _eventTimer = 0;
                    StartCoroutine(EventCyle());
                }
            }
        }
    }

    public bool Visible()
    {
        if (visibilityRenderer && visibilityRenderer.isVisible)
        {
            return true;
        }
        
        if (visibilityBounds && MainCamera.instance)
        {
            var camera = PlayerManager.instance.mainCamera.camera;
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, visibilityBounds.bounds);
        }

        return false;
    }

    private IEnumerator EventCyle()
    {
        if(_permissionChecks.Count > 0)
        {
            foreach (var p in _permissionChecks)
            {
                if(!p.PermitEvent()) { yield break; }
            }
        }

        _eventCycleActive = true;

        if (eventWarmUp > 0)
        {
            if (onEventWarmUp != null)
            {
                onEventWarmUp.Invoke();
            }

            yield return new WaitForSeconds(eventWarmUp);
        }

        if (onEventStart != null)
        {
            onEventStart.Invoke();
        }

        if (eventDuration > 0)
        {
            yield return new WaitForSeconds(eventDuration);
        }
        else
        {
            yield return null;
        }

        if (onEventEnd != null)
        {
            onEventEnd.Invoke();
        }

        _eventCycleActive = false;

        if(doNotRepeat)
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (enabled)
        {
            _paused = true;

            StopAllCoroutines();

            if (_eventCycleActive && onEventEnd != null)
            {
                onEventEnd.Invoke();
            }

            _eventCycleActive = false;

            if (resetPreWarmOnPause)
            {
                _eventTimer = preWarm * eventDelay;
            }
            else if (resetTimerOnPause)
            {
                _eventTimer = 0;
            }

            enabled = false;
        }
    }

    public void Unpause()
    {
        if (_paused)
        {
            _paused = false;
            enabled = true;
        }
    }

    public void OnDisable()
    {
        _eventCycleActive = false;
        StopAllCoroutines();
    }
}
