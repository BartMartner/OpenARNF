using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivatedScreenEffect : MonoBehaviour
{
    public bool shouldDestroy;
    public float warmUp;
    public float duration;
    public UnityEvent onWarpUp;
    public UnityEvent onActivate;

    private bool _active;
    public bool active { get { return _active; } }

    public void Activate()
    {
        StartCoroutine(StartEffect());
    }

    public void Update()
    {
        if(shouldDestroy && !_active)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator StartEffect()
    {
        _active = true;
        onWarpUp.Invoke();
        yield return new WaitForSeconds(warmUp);
        onActivate.Invoke();
        yield return new WaitForSeconds(duration - warmUp);
        _active = false;
    }
}
