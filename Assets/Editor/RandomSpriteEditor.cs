using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(RandomSprite))]
public class RandomSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var randomSprite = target as RandomSprite;

        if (randomSprite.weights == null)
        {
            randomSprite.weights = new List<float>();
        }

        if (randomSprite.weights.Count != randomSprite.sprites.Length)
        {
            while (randomSprite.weights.Count < randomSprite.sprites.Length)
            {
                randomSprite.weights.Add(1);
            }

            while (randomSprite.weights.Count > randomSprite.sprites.Length)
            {
                randomSprite.weights.RemoveAt(randomSprite.weights.Count - 1);
            }

            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            serializedObject.ApplyModifiedProperties();
            Debug.LogWarning(randomSprite.name + "'s weights not equal to child count. Correcting");
            return;
        }

        var weights = serializedObject.FindProperty("weights");

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < randomSprite.sprites.Length; i++)
        {
            var weight = weights.GetArrayElementAtIndex(i);
            float sumProbabilityFactor = randomSprite.weights.Sum();
            float probability = sumProbabilityFactor >= 0 ? weight.floatValue * 100f / sumProbabilityFactor : 100f;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Probability (" + Mathf.RoundToInt(probability) + "%)", GUILayout.ExpandWidth(false));
            weight.floatValue = EditorGUILayout.Slider(weight.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Simulate"))
        {
            randomSprite.Randomize();            
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
