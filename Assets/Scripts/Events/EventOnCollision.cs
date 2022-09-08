using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EventOnCollision : BaseEvent
{
    public bool destroyOnEnd = true;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_eventCycleActive)
        {
            StartEvent();
        }
    }

    protected override IEnumerator EventCycle()
    {
        yield return base.EventCycle();

        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
    }
}
