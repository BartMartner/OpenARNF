using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSequence : MonoBehaviour
{
    public bool singleFire;
    public float interval = 1.616667f;
    public TriggerableEvent[] events;
    private int _currentIndex;
    private float _timer;    

    public void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > interval)
        {
            _timer -= interval;
            if (events[_currentIndex])
            {
                events[_currentIndex].StartEvent();
            }
            _currentIndex = (_currentIndex + 1) % events.Length;
            if(singleFire && _currentIndex == 0)
            {
                enabled = false;
            }
        }
    }

    public void Reset()
    {
        _timer = 0f;
        _currentIndex = 0;
        enabled = true;
    }
}
