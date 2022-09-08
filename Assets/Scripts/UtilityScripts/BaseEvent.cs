using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BaseEvent : TriggerableEvent, IPausable
{
    public float eventWarmUp;
    public float eventDuration;

    public UnityEvent onEventWarmUp;
    public UnityEvent onEventStart;
    public UnityEvent onEventEnd;

    private IEnumerator _eventCycle;
    protected bool _eventCycleActive;
    public bool eventCycleAtive { get { return _eventCycleActive; } }

    private bool _paused;

    public override void StartEvent()
    {
        _eventCycle = EventCycle();
        if (!_eventCycleActive && enabled && gameObject.activeInHierarchy)
        {
            StartCoroutine(_eventCycle);
        }
    }

    protected virtual IEnumerator EventCycle()
    {
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
    }

    public virtual void HardCancel()
    {
        if (_eventCycle != null) { StopCoroutine(_eventCycle); }
        _eventCycleActive = false;
    }

    public virtual void Pause()
    {
        if (enabled)
        {
            _paused = true;
            enabled = false;
        }
    }

    public virtual void Unpause()
    {
        if (_paused)
        {
            _paused = false;
            enabled = true;
        }
    }
}
