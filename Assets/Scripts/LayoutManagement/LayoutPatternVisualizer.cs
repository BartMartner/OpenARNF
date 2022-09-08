using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class LayoutPatternVisualizer : MonoBehaviour
{
    public LayoutPattern pattern;

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (!pattern) return;

        var offset = (Vector2)transform.position;

        Handles.DrawSolidRectangleWithOutline(new Rect(offset.x, offset.y, pattern.width, pattern.height), Color.clear, Color.white);

        foreach (var limit in pattern.environmentLimits)
        {
            Color32 color = GetEnvironmentColor(limit.type);
            var rect = limit.bounds;
            rect.position += offset;

            Handles.DrawSolidRectangleWithOutline(rect, color, Color.white);
        }

        //draw mega beast bounds
        var mbrect = pattern.finalBossZone;
        mbrect.position += offset;
        Handles.DrawSolidRectangleWithOutline(mbrect, Color.clear, Color.red);

        Vector2 lastEnd = Vector2.zero;
        var pointsToConnect = new List<Vector2[]>();

        foreach (var segment in pattern.segments)
        {
            var envColor = Color.Lerp(GetEnvironmentColor(segment.environmentType), Color.white, 0.33f);

            //Draw segment start position
            Vector2 start = segment.sourcePosition != Int2D.negOne ? segment.sourcePosition.Vector2() + offset : lastEnd;

            if(segment.sourcePosition == Int2D.negOne && segment.environmentType == EnvironmentType.Cave && segment.finalTraversalStart)
            {
                start.y = pattern.environmentLimits.First((l) => l.type == EnvironmentType.Cave).bounds.yMax;
            }

            Handles.color = Color.green;
            Handles.DrawSolidDisc(start, new Vector3(0, 0, 1), 0.5f);

            //draw predicted segment end and a line between it and start
            Vector2 end = start + segment.hubDirection.normalized * segment.hubPathLength;
            Handles.color = Color.red;
            Handles.DrawSolidDisc(end, Vector3.forward, 0.5f);

            Debug.DrawLine(start, end, envColor);
            Handles.color = envColor;
            Handles.ArrowHandleCap(0, start, Quaternion.LookRotation((end - start).normalized, Vector3.up), 3, EventType.Repaint);

            Handles.color = new Color(1, 1, 1, 0.25f);
            Handles.DrawSolidArc(start, Vector3.forward, (end - start).normalized, segment.hubDirectionDeviation, Vector2.Distance(start, end));
            Handles.DrawSolidArc(start, Vector3.forward, (end - start).normalized, -segment.hubDirectionDeviation, Vector2.Distance(start, end));

            var midPoint = Vector2.Lerp(start, end, 0.5f);

            for (int i = 0; i < pattern.segmentsToConnect.Count; i++)
            {
                var pair = pattern.segmentsToConnect[i];

                if(i >= pointsToConnect.Count)
                {
                    pointsToConnect.Add(new Vector2[2]);
                }

                if (pair.from == segment.id)
                {
                    pointsToConnect[i][0] = midPoint;
                }

                if (pair.to == segment.id)
                {
                    pointsToConnect[i][1] = midPoint;
                }
            }

            lastEnd = end;
        }

        foreach (var pair in pointsToConnect)
        {
            Handles.color = Color.yellow;
            Vector2 start = pair[0];
            Vector2 end = pair[1];
            Handles.DrawDottedLine(start, end, 3);
            Handles.ArrowHandleCap(0, start, Quaternion.LookRotation((end-start).normalized, Vector3.up), 3, EventType.Repaint);            
        }
    }

    public Color GetEnvironmentColor(EnvironmentType type)
    {
        switch (type)
        {
            case EnvironmentType.BeastGuts:
                return new Color32(92, 12, 23, 255);
            case EnvironmentType.Cave:
                return new Color32(133, 93, 58, 255);
            case EnvironmentType.Factory:
                return new Color32(144, 145, 156, 255);
            case EnvironmentType.Surface:
                return new Color32(52, 117, 151, 255);
            case EnvironmentType.BuriedCity:
                return new Color32(32, 12, 0, 255);
            case EnvironmentType.Glitch:
                return new Color32(0, 196, 0, 255);
            case EnvironmentType.ForestSlums:
                return new Color32(66, 190, 153, 255);
            case EnvironmentType.CoolantSewers:
                return new Color32(0, 64, 192, 255);
            case EnvironmentType.CrystalMines:
                return new Color32(0, 128, 128, 255);
            default:
                return Color.white;
        }
    }
#endif
}
