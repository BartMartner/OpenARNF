using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamEventTrigger : BaseEvent
{
    public float coolDown = 1f;
    public bool singleFire;
    public bool destroyOnTriggered;
    public Team teamThatTriggers;

    private Collider2D _collider2D;
    private bool _ready;

    public void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        var hasTeam = collision.GetComponent<IHasTeam>();
        if (hasTeam != null && hasTeam.team == teamThatTriggers && enabled && _ready)
        {
            StartEvent();
        }
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        _ready = true;
    }

    protected override IEnumerator EventCycle()
    {
        _collider2D.enabled = false;
        yield return base.EventCycle();
        yield return StartCoroutine(WaitReset());
    }

    public override void HardCancel()
    {
        base.HardCancel();
        StartCoroutine(WaitReset());
    }

    private IEnumerator WaitReset()
    {
        yield return new WaitForSeconds(coolDown);

        if (destroyOnTriggered)
        {
            Destroy(gameObject);
        }
        else if (!singleFire)
        {
            _collider2D.enabled = true;
        }
        else
        {
            enabled = false;
        }
    }

    public override void Pause()
    {
        if (enabled)
        {
            _collider2D.enabled = false;
        }

        base.Pause();
    }

    public override void Unpause()
    {
        if (!enabled && !_eventCycleActive)
        {
            _collider2D.enabled = true;
        }

        base.Unpause();
    }
}
