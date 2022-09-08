using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEnterExitTrigger : TriggerableEvent, IPausable
{
    public float eventWarmUp;
    public float eventMinDuration;

    public UnityEvent onEventWarmUp;
    public UnityEvent onEventStart;
    public UnityEvent onEventEnd;
    
    private Collider2D _collider2D;
    private bool _ready;
    private bool _paused;
    private List<Player> _presentPlayers = new List<Player>();

    private IEnumerator _eventCycle;
    protected bool _eventCycleActive;
    public bool eventCycleAtive { get { return _eventCycleActive; } }

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        _ready = true;
    }

    public override void StartEvent()
    {
        if (!_eventCycleActive && enabled && gameObject.activeInHierarchy)
        {
            _eventCycle = EventCycle();
            StartCoroutine(_eventCycle);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if (player && !player.notTargetable && enabled && _ready)
        {
            _presentPlayers.Add(player);
            if (!_eventCycleActive) { StartEvent(); }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if (player && _presentPlayers.Contains(player)) { _presentPlayers.Remove(player); }
    }

    protected virtual IEnumerator EventCycle()
    {
        _eventCycleActive = true;

        if (eventWarmUp > 0)
        {
            if (onEventWarmUp != null) { onEventWarmUp.Invoke(); }
            yield return new WaitForSeconds(eventWarmUp);
        }

        if (onEventStart != null) { onEventStart.Invoke(); }
        if (eventMinDuration > 0) { yield return new WaitForSeconds(eventMinDuration); }

        while (_presentPlayers.Count > 0) { yield return null; }
        if (onEventEnd != null) { onEventEnd.Invoke(); }
        _eventCycleActive = false;
    }

    public virtual void Pause()
    {
        if (enabled)
        {
            _paused = true;
            enabled = false;
            _collider2D.enabled = false;
        }
    }

    public virtual void Unpause()
    {
        if (_paused && _eventCycleActive)
        {
            _paused = false;
            enabled = true;
            _collider2D.enabled = true;
        }
    }
}
