using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEventTimer : MonoBehaviour, IPausable
{
    public List<BaseEvent> events;

    [Range(0, 1)]
    public float preWarm;
    public bool resetTimerOnPause = true;
    public bool resetPreWarmOnPause = false;
    public bool randomPreWarm;

    public float eventDelay;

    private float _eventTimer;
    private bool _paused;

    private bool _eventCycleActive;
    public bool eventCycleActive
    {
        get { return _eventCycleActive; }
    }

    public void Start()
    {
        if (randomPreWarm)
        {
            preWarm = Random.value * 0.5f;
        }

        _eventTimer = preWarm * eventDelay;
    }

    public void Update()
    {
        if (!_eventCycleActive)
        {
            if (_eventTimer < eventDelay)
            {
                _eventTimer += Time.deltaTime;
            }
            else
            {
                _eventTimer = 0;
                StartCoroutine(EventCyle());
            }
        }
    }

    private IEnumerator EventCyle()
    {
        _eventCycleActive = true;
        var eventPick = events[Random.Range(0, events.Count)];
        eventPick.StartEvent();
        while(eventPick.eventCycleAtive)
        {
            yield return null;
        }
        _eventCycleActive = false;
    }

    public void Pause()
    {
        if (enabled)
        {
            _paused = true;

            StopAllCoroutines();

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
