using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SpawnOneChild))]
public class SpawnOneChildEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var spawnOne = target as SpawnOneChild;

        if(spawnOne.weights == null)
        {
            spawnOne.weights = new List<float>();
        }

        if (spawnOne.weights.Count != spawnOne.transform.childCount)
        {
            while (spawnOne.weights.Count < spawnOne.transform.childCount)
            {
                spawnOne.weights.Add(1);
            }

            while (spawnOne.weights.Count > spawnOne.transform.childCount)
            {
                spawnOne.weights.RemoveAt(spawnOne.weights.Count - 1);
            }

            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            serializedObject.ApplyModifiedProperties();
            Debug.LogWarning(spawnOne.name + "'s weights not equal to child count. Correcting");
            return;
        }

        var weights = serializedObject.FindProperty("weights");

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < spawnOne.transform.childCount; i++)
        {
            var child = spawnOne.transform.GetChild(i);
            child.gameObject.SetActive(EditorGUILayout.ToggleLeft(child.name, child.gameObject.activeSelf));

            var weight = weights.GetArrayElementAtIndex(i);
            float sumProbabilityFactor = spawnOne.weights.Sum();
            float probability = sumProbabilityFactor >= 0 ? weight.floatValue * 100f / sumProbabilityFactor : 100f;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Probability (" + Mathf.RoundToInt(probability) + "%)", GUILayout.ExpandWidth(false));
            weight.floatValue = EditorGUILayout.Slider(weight.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        if(GUILayout.Button("Simulate"))
        {
            var salvage = spawnOne.GetOneChild();
            foreach (Transform child in spawnOne.transform)
            {
                child.gameObject.SetActive(child == salvage);
            }
        }

        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
