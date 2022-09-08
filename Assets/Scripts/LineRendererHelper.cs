using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineRendererHelper : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    public Transform[] nodes;

    void Update()
    {
        if(!_lineRenderer)
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        if (nodes != null)
        {
            _lineRenderer.positionCount = nodes.Length;
            for (int i = 0; i < nodes.Length; i++)
            {
                _lineRenderer.SetPosition(i, nodes[i].position);
            }
        }
    }
}
