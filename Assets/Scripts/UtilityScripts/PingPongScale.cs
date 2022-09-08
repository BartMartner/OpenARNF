using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongScale : MonoBehaviour
{
    public Vector3 minScale = Vector3.one;
    public Vector3 maxScale = Vector3.one * 2;
    public float halfTime = 1;
    public bool slerp = true;

    private bool _torwards;
    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime / (_torwards ? halfTime : - halfTime);
        if(_timer > 1 || _timer < 0)
        {
            Mathf.Clamp01(_timer);
            _torwards = !_torwards;
        }

        transform.localScale = slerp ? Vector3.Slerp(minScale, maxScale, _timer) : Vector3.Lerp(minScale, maxScale, _timer);
    }
}
