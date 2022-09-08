using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Water))]
public class WaterEditor : Editor
{
    public Material waterMaterial;

    [MenuItem("GameObject/Water", false, 10)]
    static void CreateWater(MenuCommand menuCommand)
    {
        GameObject obj = new GameObject("New Water");
        obj.AddComponent<Water>();
        if (menuCommand.context is GameObject)
            obj.transform.SetParent((menuCommand.context as GameObject).transform);
        Selection.activeGameObject = obj;
    }

    public override void OnInspectorGUI()
    {
        var water = target as Water;
        serializedObject.Update();
        var size = serializedObject.FindProperty("size");
        var allowSplash = serializedObject.FindProperty("allowSplash");
        var splashFX = serializedObject.FindProperty("splashFX");
        var conducts = serializedObject.FindProperty("conductsElectric");
        var electric = serializedObject.FindProperty("electricCycling");
        var damageTrigger = serializedObject.FindProperty("damageCreatureTrigger");

        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(allowSplash);
        if(allowSplash.boolValue)
        {
            EditorGUILayout.PropertyField(splashFX);
        }
        EditorGUILayout.PropertyField(conducts);
        if (conducts.boolValue)
        {
            EditorGUILayout.PropertyField(electric);
        }
        EditorGUILayout.PropertyField(damageTrigger);

        var newSize = EditorGUILayout.Vector2Field("Size", water.size);
        if (newSize.x < 1) newSize.x = 1;
        if (newSize.y < 1) newSize.y = 1;

        if (water.size != newSize)
        {
            Undo.RegisterCompleteObjectUndo(target, "Change Size (" + target.name + ")");
            size.vector2Value = newSize;
            water.SetSize(newSize);
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }
}
