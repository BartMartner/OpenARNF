using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveParallax : MonoBehaviour
{
    [Range(0,1)]
    public float depth = 0;
    private float _lastDepth = -1;

    public void Update()
    {
        if (_lastDepth != depth)
        {
            var pos = transform.position;
            pos.z = 280 * depth;
            transform.position = pos;
            transform.localScale = Vector3.one * (Mathf.Lerp(1,33,depth));
            _lastDepth = depth;
        }
    }
}
