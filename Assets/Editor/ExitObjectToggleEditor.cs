using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(ExitObjectToggle))]
public class ExitObjectToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var exitObjectToggle = target as ExitObjectToggle;

        base.OnInspectorGUI();
        if (exitObjectToggle.exitMatch.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle No Exit"))
            {
                exitObjectToggle.exitMatch.SetActive(false);
                exitObjectToggle.exitNotMatch.SetActive(true);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        else if (exitObjectToggle.exitNotMatch.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle Has Exit"))
            {
                exitObjectToggle.exitMatch.SetActive(true);
                exitObjectToggle.exitNotMatch.SetActive(false);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
