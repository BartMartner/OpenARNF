using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ToggleBasedOnCorruption))]
public class ToggleBasedOnCorruptionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var corruptionToggle = target as ToggleBasedOnCorruption;

        base.OnInspectorGUI();
        if (corruptionToggle.overCorruption)
        {
            if (corruptionToggle.overCorruption.activeInHierarchy)
            {
                if (GUILayout.Button("Toggle Under Corruption"))
                {
                    corruptionToggle.overCorruption.SetActive(false);
                    if (corruptionToggle.underCorruption)
                    {
                        corruptionToggle.underCorruption.SetActive(true);
                    }
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }
            }
            else
            {
                if (GUILayout.Button("Toggle Over Corruption"))
                {
                    corruptionToggle.overCorruption.SetActive(true);
                    if(corruptionToggle.underCorruption)
                    {
                        corruptionToggle.underCorruption.SetActive(false);
                    }
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }
        else
        {
            GUILayout.Label("Please link in overCorruption gameobject!");
        }
    }
}
