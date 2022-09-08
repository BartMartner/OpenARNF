using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VerticalSmoothPacer : MonoBehaviour
{
    public float time = 2f;
    public UnityEvent onChangeDirection;
    
    [Range(0.125f, 24)]
    public float range;

    private Vector3 _localCenter;

    protected void Start()
    {
        _localCenter = transform.localPosition;        
    }

    public void Update()
    {
        var offset = Mathf.SmoothStep(range * -0.5f, range * 0.5f, Mathf.PingPong(Time.time/time, 1));
        var pos = _localCenter;
        pos.y += offset;
        transform.localPosition = pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (range > 0)
        {
            var up = transform.position + transform.up * 0.5f * range;
            var down = transform.position - transform.up * 0.5f * range;
            Debug.DrawLine(transform.position, up);
            Debug.DrawLine(up - Vector3.right * 0.5f, up + Vector3.right * 0.5f);
            Debug.DrawLine(transform.position, down);
            Debug.DrawLine(down - Vector3.right * 0.5f, down + Vector3.right * 0.5f);
        }
    }
}
