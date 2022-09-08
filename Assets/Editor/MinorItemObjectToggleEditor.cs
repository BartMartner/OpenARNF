using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MinorItemObjectToggle))]
public class MinorItemObjectToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var minorItemObjectToggle = target as MinorItemObjectToggle;

        base.OnInspectorGUI();
        if (minorItemObjectToggle.minorItemExists)
        {
            if (minorItemObjectToggle.minorItemExists.activeInHierarchy)
            {
                if (GUILayout.Button("Toggle No Minor Item"))
                {
                    minorItemObjectToggle.minorItemExists.SetActive(false);
                    if (minorItemObjectToggle.minorItemDoesNotExists)
                    {
                        minorItemObjectToggle.minorItemDoesNotExists.SetActive(true);
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Toggle Has Minor Item"))
                {
                    minorItemObjectToggle.minorItemExists.SetActive(true);
                    if(minorItemObjectToggle.minorItemDoesNotExists)
                    {
                        minorItemObjectToggle.minorItemDoesNotExists.SetActive(false);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label("Please link in minorItemExists gameobject!");
        }
    }
}
