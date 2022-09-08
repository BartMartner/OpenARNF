using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class LineRendererDouble : MonoBehaviour
{
    public LineRenderer match;
    public float widthBonus;
    public LineRenderer lineRenderer { get; private set; }
    private Vector3[] _copy;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!lineRenderer) { lineRenderer = GetComponent<LineRenderer>(); }
        if(match)
        {
            _copy = new Vector3[match.positionCount];
            lineRenderer.positionCount = match.GetPositions(_copy);
            lineRenderer.SetPositions(_copy);
            lineRenderer.widthMultiplier = match.widthMultiplier + widthBonus;
        }
    }
}
