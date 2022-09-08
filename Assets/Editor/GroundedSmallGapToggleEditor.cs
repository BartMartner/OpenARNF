using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GroundedSmallGapToggle))]
public class GroundedSmallGapToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var groundedSmallGapToggle = target as GroundedSmallGapToggle;

        base.OnInspectorGUI();
        if (!groundedSmallGapToggle.needsSmallGap.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle Needs Gap"))
            {
                if (groundedSmallGapToggle.doesNotNeedSmallGap)
                {
                    groundedSmallGapToggle.doesNotNeedSmallGap.SetActive(false);
                }
                groundedSmallGapToggle.needsSmallGap.SetActive(true);
            }
        }
        else if (groundedSmallGapToggle.needsSmallGap.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle Does Not Need Gap"))
            {
                if (groundedSmallGapToggle.doesNotNeedSmallGap)
                {
                    groundedSmallGapToggle.doesNotNeedSmallGap.SetActive(true);
                }
                groundedSmallGapToggle.needsSmallGap.SetActive(false);
            }
        }
    }
}
