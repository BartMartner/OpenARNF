using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGroup : TriggerableEvent
{
    public TriggerableEvent[] events;

    public override void StartEvent()
    {
        foreach (var e in events)
        {
            if (e)
            {
                e.StartEvent();
            }
        }
    }
}
