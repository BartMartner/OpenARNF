using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(IgnoreTerrainToggle))]
public class IgnoreTerrainToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var groundedSmallGapToggle = target as IgnoreTerrainToggle;

        base.OnInspectorGUI();
        if (!groundedSmallGapToggle.requiresIgnoreTerrain.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle Needs IgnoreTerrain"))
            {
                if (groundedSmallGapToggle.doesNotRequireIgnoreTerrain)
                {
                    groundedSmallGapToggle.doesNotRequireIgnoreTerrain.SetActive(false);
                }
                groundedSmallGapToggle.requiresIgnoreTerrain.SetActive(true);
            }
        }
        else if (groundedSmallGapToggle.requiresIgnoreTerrain.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle Does Not Need Ignore Terrain"))
            {
                if (groundedSmallGapToggle.doesNotRequireIgnoreTerrain)
                {
                    groundedSmallGapToggle.doesNotRequireIgnoreTerrain.SetActive(true);
                }
                groundedSmallGapToggle.requiresIgnoreTerrain.SetActive(false);
            }
        }
    }
}
