using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TemporarySpeedUp : BaseMovementBehaviour
{
    public float warmUp;
    public float duration;
    public float fastSpeed = 10;
    public SinusoidalPonger sinusoidalPonger;
    public PaletteCycling paletteCycling;
    public AudioClip warningSound;
    public AudioSource loopingSound;
    public EventTimer[] eventsToWaitFor;
    public UnityEvent onStart;
    public UnityEvent onStop;

    private SpeedUpSegment[] _speedUpSegments;
    private float _originalSpeed;
    private bool _spedUp;

    private void Awake()
    {
        _originalSpeed = sinusoidalPonger.speed;
        _speedUpSegments = GetComponentsInChildren<SpeedUpSegment>();
    }

    public void SpeedUp()
    {
        if (!_spedUp) { StartCoroutine(SpeedUpCoroutine()); }
    }

    public IEnumerator SpeedUpCoroutine()
    {
        _spedUp = true;

        //finish events
        var eventWaitTime = 0f;
        var eventsInProgress = true;

        while (eventsInProgress)
        {
            eventsInProgress = false;
            foreach (var e in eventsToWaitFor)
            {
                if(e.eventCycleActive)
                {
                    eventsInProgress = true;
                }
                else
                {
                    e.Pause();
                }
            }
            yield return null;
            eventWaitTime += Time.deltaTime;
        }

        if (loopingSound && warningSound) { loopingSound.PlayOneShot(warningSound); }

        if (onStart != null) { onStart.Invoke(); }

        foreach (var segement in _speedUpSegments)
        {
            segement.OnSpeedUpStart();
        }

        _originalSpeed = sinusoidalPonger.speed;

        //begin flashing and pause
        paletteCycling.enabled = true;
        sinusoidalPonger.enabled = false;
        sinusoidalPonger.SetSpeed(fastSpeed);

        yield return new WaitForSeconds(warmUp - eventWaitTime);

        if (loopingSound) { loopingSound.Play(); }

        sinusoidalPonger.enabled = true;

        yield return new WaitForSeconds(duration);

        paletteCycling.enabled = false;

        foreach (var segement in _speedUpSegments)
        {
            segement.OnSpeedUpEnd();
        }

        sinusoidalPonger.SetSpeed(_originalSpeed);

        foreach (var e in eventsToWaitFor)
        {
            e.Unpause();
        }

        if (onStop != null) { onStop.Invoke(); }

        if (loopingSound)
        {
            var timer = 1f;
            while(timer > 0)
            {
                loopingSound.pitch = timer;
                timer -= Time.deltaTime;
                yield return null;
            }
            loopingSound.Stop();
            loopingSound.pitch = 1;
        }

        _spedUp = false;
    }

    public void Stop()
    {
        StopAllCoroutines();
        loopingSound.Stop();
        paletteCycling.enabled = false;
        sinusoidalPonger.SetSpeed(_originalSpeed);
        onStop.Invoke();
    }
}
