using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(MonsterSpawnPoint))]
public class MonsterSpawnPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var monsterSpawnPoint = target as MonsterSpawnPoint;        

        float sumProbabilityFactor = 0;
        if (monsterSpawnPoint.spawns == null) return;

        for (int i = 0; i < monsterSpawnPoint.spawns.Length; i++)
        {
             sumProbabilityFactor += monsterSpawnPoint.spawns[i].weight;
        }

        EditorGUILayout.LabelField("Nothing (" + monsterSpawnPoint.spawnNothingChance * 100 + "%)");

        for (int i = 0; i < monsterSpawnPoint.spawns.Length; i++)
        {
            var child = monsterSpawnPoint.spawns[i];
            if (child.prefab != null)
            {
                float probability = sumProbabilityFactor >= 0 ? child.weight * 100f / sumProbabilityFactor : 100f;
                probability *= (1 - monsterSpawnPoint.spawnNothingChance);
                probability *= (1 - 0.05f);
                EditorGUILayout.LabelField(child.prefab.name + " (" + probability + "%)");
                var original = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;
                var champWeightSum = 0f;
                for (int j = 0; j < child.championVariants.Length; j++)
                {
                    champWeightSum += child.championVariants[j].weight;
                }

                for (int j = 0; j < child.championVariants.Length; j++)
                {
                    var champ = child.championVariants[j];
                    if (champ.prefab)
                    {
                        probability = (champWeightSum >= 0 ? champ.weight * 100f / champWeightSum : 100f);
                        probability *= (1 - monsterSpawnPoint.spawnNothingChance);
                        probability *= 0.05f;                        
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = champ.stats ? Color.white : Color.red;
                        EditorGUILayout.LabelField(champ.prefab.name + " (" + probability + "%)", style);
                    }
                }
                EditorGUI.indentLevel = original;
            }
        }        
    }
}
