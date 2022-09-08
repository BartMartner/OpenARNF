using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SpawnPrefab))]
public class SpawnPrefabEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var spawnPrefab = target as SpawnPrefab;

        if(spawnPrefab.weights == null)
        {
            spawnPrefab.weights = new List<float>();
        }

        if (spawnPrefab.weights.Count != spawnPrefab.prefabs.Length)
        {
            while (spawnPrefab.weights.Count < spawnPrefab.prefabs.Length)
            {
                spawnPrefab.weights.Add(1);
            }

            while (spawnPrefab.weights.Count > spawnPrefab.prefabs.Length)
            {
                spawnPrefab.weights.RemoveAt(spawnPrefab.weights.Count - 1);
            }

            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            serializedObject.ApplyModifiedProperties();
            Debug.LogWarning(spawnPrefab.name + "'s weights not equal to child count. Correcting");
            return;
        }

        var weights = serializedObject.FindProperty("weights");

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < spawnPrefab.prefabs.Length; i++)
        {
            var child = spawnPrefab.prefabs[i];
            EditorGUILayout.LabelField(child.name);

            var weight = weights.GetArrayElementAtIndex(i);
            float sumProbabilityFactor = spawnPrefab.weights.Sum();
            float probability = sumProbabilityFactor >= 0 ? weight.floatValue * 100f / sumProbabilityFactor : 100f;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Probability (" + Mathf.RoundToInt(probability) + "%)", GUILayout.ExpandWidth(false));
            weight.floatValue = EditorGUILayout.Slider(weight.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        //if(GUILayout.Button("Simulate"))
        //{
        //    var salvage = spawnPrefab.GetOneChild();
        //    foreach (Transform child in spawnPrefab.transform)
        //    {
        //        child.gameObject.SetActive(child == salvage);
        //    }
        //}

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
