using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(CityBackgroundGenerator))]
public class SurfaceCityBackgroundGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var generator = target as CityBackgroundGenerator;

        if (GUILayout.Button("Generate Background"))
        {
            generator.GenerateBackground();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
