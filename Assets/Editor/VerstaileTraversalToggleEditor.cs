using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VersatileTraversalToggle))]
public class VersatileTraversalToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var toggle = target as VersatileTraversalToggle;

        base.OnInspectorGUI();
        bool noTraversal = !((toggle.smallGap && toggle.smallGap.activeInHierarchy) || 
            (toggle.phaseWall && toggle.phaseWall.activeInHierarchy) ||
            (toggle.ignoreTerrain && toggle.ignoreTerrain.activeInHierarchy));

        var orig = GUI.backgroundColor;
        GUI.backgroundColor = orig;
        if (noTraversal) { GUI.backgroundColor = Color.red; }
        if (GUILayout.Button("None"))
        {
            if (toggle.noTraversal) { toggle.noTraversal.SetActive(true); }
            if (toggle.phaseWall) { toggle.phaseWall.SetActive(false); }
            if (toggle.ignoreTerrain) { toggle.ignoreTerrain.SetActive(false); }
            if (toggle.smallGap) { toggle.smallGap.SetActive(false); }
        }
        GUI.backgroundColor = orig;

        if (toggle.smallGap)
        {
            if (toggle.smallGap.activeInHierarchy) { GUI.backgroundColor = Color.red; }
            if (GUILayout.Button("Small Gap"))
            {
                if (toggle.noTraversal) { toggle.noTraversal.SetActive(false); }
                if (toggle.phaseWall) { toggle.phaseWall.SetActive(false); }
                if (toggle.ignoreTerrain) { toggle.ignoreTerrain.SetActive(false); }
                toggle.smallGap.SetActive(true);
            }
        }

        GUI.backgroundColor = orig;
        if (toggle.phaseWall)
        {
            if (toggle.phaseWall.activeInHierarchy) { GUI.backgroundColor = Color.red; }
            if (GUILayout.Button("Phase Wall"))
            {
                if (toggle.noTraversal) { toggle.noTraversal.SetActive(false); }
                if (toggle.smallGap) { toggle.smallGap.SetActive(false); }
                if (toggle.ignoreTerrain) { toggle.ignoreTerrain.SetActive(false); }
                toggle.phaseWall.SetActive(true);
            }
        }

        GUI.backgroundColor = orig;
        if (toggle.ignoreTerrain)
        {
            if (toggle.ignoreTerrain.activeInHierarchy) { GUI.backgroundColor = Color.red; }
            if (GUILayout.Button("Ignore Terrain"))
            {
                if (toggle.noTraversal) { toggle.noTraversal.SetActive(false); }
                if (toggle.phaseWall) { toggle.phaseWall.SetActive(false); }
                if (toggle.smallGap) { toggle.smallGap.SetActive(false); }
                toggle.ignoreTerrain.SetActive(true);
            }
        }
    }
}
