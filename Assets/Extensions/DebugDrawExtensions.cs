using UnityEngine;

public static partial class Extensions
{
    public static void DrawX(Vector3 position, float size, Color color, float duration)
    {
#if UNITY_EDITOR
        var halfSize = size * 0.5f;
        Debug.DrawLine(position + new Vector3(-halfSize, halfSize), position + new Vector3(halfSize, -halfSize), color, duration);
        Debug.DrawLine(position + new Vector3(halfSize, halfSize), position + new Vector3(-halfSize, -halfSize), color, duration);
#endif
    }

    public static void DrawDashedLine(Vector3 start, Vector3 end, float space, Color color, float duration, float offset = 0)
    {
#if UNITY_EDITOR
        var delta = (end - start);
        var direction = delta.normalized;
        var magnitude = delta.magnitude;
        var i = 0;
        var s = start;
        var remainingDistance = magnitude;

        var o = Mathf.Clamp01(offset) * space;
        if (o > 0)
        {
            s += o * direction;
            remainingDistance -= o;
        }

        while (remainingDistance > 0)
        {
            var d = Mathf.Min(remainingDistance, space);
            var e = s + (direction * d);
            if (i % 2 == 0) { Debug.DrawLine(s, e, color, duration); }
            s = e;

            remainingDistance -= d;
            i++;
        }
#endif
    }

    public static void DrawCircle(Vector3 position, float radius)
    {
#if UNITY_EDITOR        
        UnityEditor.Handles.DrawWireDisc(position, Vector3.forward, radius);
#endif
    }

    public static void DrawSquare(Rect rect, Color outline)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin), outline);
            Debug.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax), outline);
            Debug.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax), outline);
            Debug.DrawLine(new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax), outline);
        }
        else
        {
            UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, Color.clear, outline);
        }
#endif
    }
}
